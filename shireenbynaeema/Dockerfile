FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY webapp.sln .
COPY Directory.Build.props .
COPY webapp.Server/webapp.Server.csproj webapp.Server/
COPY webapp.Application/webapp.Application.csproj webapp.Application/
COPY webapp.Domain/webapp.Domain.csproj webapp.Domain/
COPY webapp.Infrastructure/webapp.Infrastructure.csproj webapp.Infrastructure/
COPY webapp.SharedServices/webapp.SharedServices.csproj webapp.SharedServices/
COPY webapp.Tests/webapp.Tests.csproj webapp.Tests/

# Restore dependencies
RUN dotnet restore webapp.sln

# Copy everything else and build
COPY . .
RUN dotnet publish webapp.Server/webapp.Server.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Create uploads directory with proper permissions
RUN mkdir -p /app/wwwroot/uploads/products /app/wwwroot/uploads/categories /app/wwwroot/uploads/returns \
    && chmod -R 755 /app/wwwroot/uploads

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}

ENTRYPOINT ["dotnet", "webapp.Server.dll"]
