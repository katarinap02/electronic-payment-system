FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /build

COPY . .

RUN dotnet tool install --global dotnet-ef --version 8.0.0

RUN dotnet restore "PSP/PSP.API/PSP.API.csproj"
RUN dotnet publish "PSP/PSP.API/PSP.API.csproj" -c Release -o /output

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /webapp
COPY --from=build /output .

ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "PSP.API.dll"]
