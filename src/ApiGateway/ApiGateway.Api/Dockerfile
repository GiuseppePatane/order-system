FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore
COPY ["src/ApiGateway/ApiGateway.Api/ApiGateway.Api.csproj", "src/ApiGateway/ApiGateway.Api/"]
COPY ["ApiGateway/ApiGateway.Core/ApiGateway.Core.csproj", "ApiGateway/ApiGateway.Core/"]
COPY ["ApiGateway/ApiGateway.infrastructure.GrcpClient/ApiGateway.infrastructure.GrcpClient.csproj", "ApiGateway/ApiGateway.infrastructure.GrcpClient/"]
COPY ["Product/Product.Proto.GrcpClient/Product.Proto.GrcpClient.csproj", "Product/Product.Proto.GrcpClient/"]
COPY ["src/Product/Product.Proto/Product.Proto.csproj", "src/Product/Product.Proto/"]
COPY ["src/Shared/Shared.Proto/Shared.Proto.csproj", "src/Shared/Shared.Proto/"]
COPY ["ProductionSystem.ServiceDefaults/ProductionSystem.ServiceDefaults.csproj", "ProductionSystem.ServiceDefaults/"]

RUN dotnet restore "src/ApiGateway/ApiGateway.Api/ApiGateway.Api.csproj"

# Copy everything else
COPY . .

WORKDIR "/src/src/ApiGateway/ApiGateway.Api"
RUN dotnet build "ApiGateway.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ApiGateway.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.Api.dll"]