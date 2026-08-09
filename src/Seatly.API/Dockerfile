# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project files and restore
COPY ["src/Seatly.Domain/Seatly.Domain.csproj", "src/Seatly.Domain/"]
COPY ["src/Seatly.Application/Seatly.Application.csproj", "src/Seatly.Application/"]
COPY ["src/Seatly.Infrastructure/Seatly.Infrastructure.csproj", "src/Seatly.Infrastructure/"]
COPY ["src/Seatly.API/Seatly.API.csproj", "src/Seatly.API/"]
RUN dotnet restore "src/Seatly.API/Seatly.API.csproj"

# Copy source and publish
COPY src/ src/
WORKDIR "/app/src/Seatly.API"
RUN dotnet publish "Seatly.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Seatly.API.dll"]
