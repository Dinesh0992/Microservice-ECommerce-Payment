# Stage 1: Build the application using the .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
# This includes the shared contracts project
COPY ["Order.Service/Order.Service.csproj", "Order.Service/"]
COPY ["ECommerce.Contracts/ECommerce.Contracts.csproj", "ECommerce.Contracts/"]
RUN dotnet restore "Order.Service/Order.Service.csproj"

# Copy the rest of the code and publish
COPY . .
WORKDIR "/src/Order.Service"
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port your Order Service uses
EXPOSE 5170
ENV ASPNETCORE_URLS=http://+:5170

ENTRYPOINT ["dotnet", "Order.Service.dll"]