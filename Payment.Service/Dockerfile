FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Payment.Service/Payment.Service.csproj", "Payment.Service/"]
COPY ["ECommerce.Contracts/ECommerce.Contracts.csproj", "ECommerce.Contracts/"]
RUN dotnet restore "Payment.Service/Payment.Service.csproj"
COPY . .
WORKDIR "/src/Payment.Service"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Payment.Service.dll"]