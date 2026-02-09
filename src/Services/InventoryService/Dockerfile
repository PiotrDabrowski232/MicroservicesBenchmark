FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/BuildingBlocks/Messaging/Messaging.csproj", "src/BuildingBlocks/Messaging/"]
COPY ["src/BuildingBlocks/Observability/Observability.csproj", "src/BuildingBlocks/Observability/"]
COPY ["src/BuildingBlocks/SharedKernel/SharedKernel.csproj", "src/BuildingBlocks/SharedKernel/"]

COPY ["src/Services/InventoryService/InventoryService.Domain/InventoryService.Domain.csproj", "src/Services/InventoryService/InventoryService.Domain/"]
COPY ["src/Services/InventoryService/InventoryService.Application/InventoryService.Application.csproj", "src/Services/InventoryService/InventoryService.Application/"]
COPY ["src/Services/InventoryService/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj", "src/Services/InventoryService/InventoryService.Infrastructure/"]

COPY ["src/Services/InventoryService/InventoryService.Api/InventoryService.Api.csproj", "src/Services/InventoryService/InventoryService.Api/"]

RUN dotnet restore "src/Services/InventoryService/InventoryService.Api/InventoryService.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/InventoryService/InventoryService.Api"
RUN dotnet build "InventoryService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InventoryService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryService.Api.dll"]