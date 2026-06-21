FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ProductApiAssessment.sln", "./"]
COPY ["src/ProductApi.API/ProductApi.API.csproj", "src/ProductApi.API/"]
COPY ["src/ProductApi.Application/ProductApi.Application.csproj", "src/ProductApi.Application/"]
COPY ["src/ProductApi.Domain/ProductApi.Domain.csproj", "src/ProductApi.Domain/"]
COPY ["src/ProductApi.Infrastructure/ProductApi.Infrastructure.csproj", "src/ProductApi.Infrastructure/"]
COPY ["tests/ProductApi.API.Tests/ProductApi.API.Tests.csproj", "tests/ProductApi.API.Tests/"]
COPY ["tests/ProductApi.Application.Tests/ProductApi.Application.Tests.csproj", "tests/ProductApi.Application.Tests/"]
RUN dotnet restore "ProductApiAssessment.sln"

COPY . .
RUN dotnet publish "src/ProductApi.API/ProductApi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProductApi.API.dll"]
