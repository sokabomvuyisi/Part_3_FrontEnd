# Multi-stage Dockerfile for RaceDay.Mvc (ASP.NET Core 8)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["RaceDay.Mvc/RaceDay.Mvc.csproj", "RaceDay.Mvc/"]
COPY ["RaceDay.Contracts/RaceDay.Contracts.csproj", "RaceDay.Contracts/"]
RUN dotnet restore "RaceDay.Mvc/RaceDay.Mvc.csproj"

# Copy everything and publish
COPY . .
WORKDIR /src/RaceDay.Mvc
RUN dotnet publish "RaceDay.Mvc.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]
