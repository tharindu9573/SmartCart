FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SmartCart/SmartCart.csproj", "SmartCart/"]
COPY ["SmartCart.Core/SmartCart.Core.csproj", "SmartCart.Core/"]
COPY ["SmartCart.Infrastructure/SmartCart.Infrastructure.csproj", "SmartCart.Infrastructure/"]

RUN dotnet restore "SmartCart/SmartCart.csproj"

COPY . .

WORKDIR "/src/SmartCart"
RUN dotnet build "SmartCart.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartCart.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartCart.dll"]
