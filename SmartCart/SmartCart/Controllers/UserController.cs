using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCart.Core.Domain.Models;
using SmartCart.Core.Interfaces;
using SmartCart.Core.Interfaces.IServices.Application;

namespace SmartCart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;

    public UserController(IUnitOfWork unitOfWork, IOtpService otpService)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
    }

    [HttpPost("send-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        await _otpService.GenerateAndSendOtpAsync(request.MobileNumber);
        return Ok(new { message = "OTP sent successfully." });
    }

    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (!_otpService.VerifyOtp(request.MobileNumber, request.Otp))
            return BadRequest(new { message = "Invalid or expired OTP." });

        _otpService.InvalidateOtp(request.MobileNumber);

        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber);
        if (user != null)
        {
            user.IsVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { exists = true, userId = user.UserId, name = user.Name, email = user.Email });
        }

        return Ok(new { exists = false });
    }

    [HttpPost("upsert")]
    [AllowAnonymous]
    public async Task<IActionResult> UpsertUser([FromBody] UpsertUserRequest request)
    {
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber);
        if (user == null)
        {
            user = new User
            {
                MobileNumber = request.MobileNumber,
                Name = request.Name,
                Email = request.Email,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Users.AddAsync(user);
        }
        else
        {
            user.Name = request.Name;
            user.Email = request.Email;
            user.IsVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
        }

        await _unitOfWork.SaveChangesAsync();
        return Ok(new { userId = user.UserId, name = user.Name, email = user.Email });
    }
}

public record SendOtpRequest(string MobileNumber);
public record VerifyOtpRequest(string MobileNumber, string Otp);
public record UpsertUserRequest(string MobileNumber, string Name, string Email);
