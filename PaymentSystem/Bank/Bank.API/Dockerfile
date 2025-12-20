FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /build

COPY . .

RUN dotnet restore "Bank/Bank.API/Bank.API.csproj"
RUN dotnet publish "Bank/Bank.API/Bank.API.csproj" -c Release -o /output

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /webapp
COPY --from=build /output .
ENTRYPOINT ["dotnet", "Bank.API.dll"]