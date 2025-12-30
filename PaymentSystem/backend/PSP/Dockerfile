FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY PSP.sln ./
COPY src/PSP.Domain/PSP.Domain.csproj ./src/PSP.Domain/
COPY src/PSP.Application/PSP.Application.csproj ./src/PSP.Application/
COPY src/PSP.Infrastructure/PSP.Infrastructure.csproj ./src/PSP.Infrastructure/
COPY src/PSP.Api/PSP.API.csproj ./src/PSP.Api/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish
WORKDIR /src
RUN dotnet publish src/PSP.Api/PSP.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "PSP.API.dll"]
