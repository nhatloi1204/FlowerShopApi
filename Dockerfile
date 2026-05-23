# .NET 8.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY FlowerShop.API/FlowerShop.API.csproj ./FlowerShop.API/

RUN dotnet restore FlowerShop.API/FlowerShop.API.csproj

COPY . ./

RUN dotnet publish FlowerShop.API/FlowerShop.API.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /out .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FlowerShop.API.dll"]