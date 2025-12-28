FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /build

COPY . .

RUN dotnet tool install --global dotnet-ef --version 8.0.0

RUN dotnet restore "WebShop/WebShop.API/WebShop.API.csproj"
RUN dotnet publish "WebShop/WebShop.API/WebShop.API.csproj" -c Release -o /output

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /webapp
COPY --from=build /output .

COPY --from=build /build/WebShop/WebShop.API/Migrations ./Migrations/

ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "WebShop.API.dll"]