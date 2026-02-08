FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /build

# Copy entire solution for build context
COPY . .

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef --version 8.0.0

# Restore dependencies
RUN dotnet restore "Crypto/Crypto.API/Crypto.API.csproj"

# Build and publish
RUN dotnet publish "Crypto/Crypto.API/Crypto.API.csproj" -c Release -o /output

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /webapp
COPY --from=build /output .

# Copy migrations if they exist (for future use)
# COPY --from=build /build/Crypto/Crypto.API/Migrations ./Migrations/

# Add dotnet-ef to PATH (for migrations in container)
ENV PATH="$PATH:/root/.dotnet/tools"

# Expose port
EXPOSE 80

ENTRYPOINT ["dotnet", "Crypto.API.dll"]
