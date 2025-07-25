# Étape 1 : Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers csproj nécessaires
COPY ../../../HotelManagement.sln ./
COPY ../../Core/HotelManagement.Application/*.csproj Core/HotelManagement.Application/
COPY ../../Core/HotelManagement.Domain/*.csproj Core/HotelManagement.Domain/
COPY ../../Infrastructure/HotelManagement.Identity/*.csproj Infrastructure/HotelManagement.Identity/
COPY ../../Infrastructure/HotelManagement.Infrastructure/*.csproj Infrastructure/HotelManagement.Infrastructure/
COPY ./*.csproj .

# Restauration
RUN dotnet restore

# Copier tout le code source
COPY ../../ .

# Build de l’API
WORKDIR /src/Presentation/HotelManagement.API
RUN dotnet publish -c Release -o /app/publish

# Étape 2 : Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "HotelManagement.API.dll"]
