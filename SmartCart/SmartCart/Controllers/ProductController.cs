using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCart.Core.Interfaces;

namespace SmartCart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _unitOfWork.Products
            .Query()
            .Include(p => p.Category)
            .Select(p => new
            {
                p.ProductId,
                p.Name,
                p.Price,
                p.AvailableQuantity,
                p.ImageUrl,
                category = p.Category.Name
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _unitOfWork.Products
            .Query()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null) return NotFound();

        return Ok(new
        {
            product.ProductId,
            product.Name,
            product.Price,
            product.AvailableQuantity,
            product.ImageUrl,
            category = product.Category.Name
        });
    }

    [HttpGet("rfid/{uid}")]
    public async Task<IActionResult> GetByRfidUid(string uid)
    {
        var item = await _unitOfWork.ProductRfidItems
            .Query()
            .Include(r => r.Product)
            .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(r => r.Uid == uid);

        if (item == null) return NotFound();

        return Ok(new
        {
            uid = item.Uid,
            status = item.Status.ToString(),
            product = new
            {
                item.Product.ProductId,
                item.Product.Name,
                item.Product.Price,
                item.Product.AvailableQuantity,
                item.Product.ImageUrl,
                category = item.Product.Category.Name
            }
        });
    }
}
