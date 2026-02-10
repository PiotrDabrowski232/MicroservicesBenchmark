FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/BuildingBlocks/Messaging/Messaging.csproj", "src/BuildingBlocks/Messaging/"]
COPY ["src/BuildingBlocks/Observability/Observability.csproj", "src/BuildingBlocks/Observability/"]
COPY ["src/BuildingBlocks/SharedKernel/SharedKernel.csproj", "src/BuildingBlocks/SharedKernel/"]

COPY ["src/Services/OrderService/OrderService.Domain/OrderService.Domain.csproj", "src/Services/OrderService/OrderService.Domain/"]
COPY ["src/Services/OrderService/OrderService.Application/OrderService.Application.csproj", "src/Services/OrderService/OrderService.Application/"]
COPY ["src/Services/OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj", "src/Services/OrderService/OrderService.Infrastructure/"]

COPY ["src/Services/OrderService/OrderService.Api/OrderService.Api.csproj", "src/Services/OrderService/OrderService.Api/"]

RUN dotnet restore "src/Services/OrderService/OrderService.Api/OrderService.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/OrderService/OrderService.Api"
RUN dotnet build "OrderService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]