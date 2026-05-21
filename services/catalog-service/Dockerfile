# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["catalog-service.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish "catalog-service.csproj" -c Release -o /app/publish

# Stage 2: Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
#non root user for security
USER app 
ENTRYPOINT ["dotnet", "catalog-service.dll"]
