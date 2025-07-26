# Étape 1 : Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers sln et csproj
COPY *.sln ./
COPY src/Core/HotelManagement.Application/*.csproj src/Core/HotelManagement.Application/
COPY src/Core/HotelManagement.Domain/*.csproj src/Core/HotelManagement.Domain/
COPY src/Infrastructure/HotelManagement.Identity/*.csproj src/Infrastructure/HotelManagement.Identity/
COPY src/Infrastructure/HotelManagement.Infrastructure/*.csproj src/Infrastructure/HotelManagement.Infrastructure/
COPY src/Presentation/HotelManagement.API/*.csproj src/Presentation/HotelManagement.API/

COPY tests/HotelManagement.Domain.Tests/*.csproj tests/HotelManagement.Domain.Tests/
COPY tests/HotelManagement.Application.Tests/*.csproj tests/HotelManagement.Application.Tests/
COPY tests/HotelManagement.Infrastructure.Tests/*.csproj tests/HotelManagement.Infrastructure.Tests/
COPY tests/HotelManagement.API.Tests/*.csproj tests/HotelManagement.API.Tests/

# Restauration des dépendances
RUN dotnet restore

# Copier le reste des fichiers
COPY . .

# Compiler et publier
WORKDIR /src/src/Presentation/HotelManagement.API
RUN dotnet publish -c Release -o /app/publish

# Étape 2 : Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "HotelManagement.API.dll"]
