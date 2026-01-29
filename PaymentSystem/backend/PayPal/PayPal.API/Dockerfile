FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /build

COPY . .
RUN dotnet tool install --global dotnet-ef --version 8.0.0

RUN dotnet restore "PayPal/PayPal.API/PayPal.API.csproj"
RUN dotnet publish "PayPal/PayPal.API/PayPal.API.csproj" -c Release -o /output

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /webapp
COPY --from=build /output .

COPY --from=build /build/PayPal/PayPal.API/Migrations ./Migrations/

ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "PayPal.API.dll"]