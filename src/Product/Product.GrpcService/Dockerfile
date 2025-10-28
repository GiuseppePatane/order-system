FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Product/Product.GrpcService/Product.GrpcService.csproj", "src/Product/Product.GrpcService/"]
COPY ["src/Product/Product.Proto/Product.Proto.csproj", "src/Product/Product.Proto/"]
COPY ["src/Shared/Shared.Proto/Shared.Proto.csproj", "src/Shared/Shared.Proto/"]
COPY ["src/Shared/Shared.GrpcInfrastructure/Shared.GrpcInfrastructure.csproj", "src/Shared/Shared.GrpcInfrastructure/"]
COPY ["ProductionSystem.ServiceDefaults/ProductionSystem.ServiceDefaults.csproj", "ProductionSystem.ServiceDefaults/"]

RUN dotnet restore "src/Product/Product.GrpcService/Product.GrpcService.csproj"

# Copy everything else
COPY . .

WORKDIR "/src/src/Product/Product.GrpcService"
RUN dotnet build "Product.GrpcService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Product.GrpcService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Product.GrpcService.dll"]