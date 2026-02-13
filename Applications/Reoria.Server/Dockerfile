# =========================
# Build Stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Set the build configuration
ARG BUILD_CONFIGURATION=Release

# Set the working directory
WORKDIR /src

# Copy only project file first (better caching)
COPY Applications/Reoria.Server/Reoria.Server.csproj Applications/Reoria.Server/

# Restore dependencies
RUN dotnet restore Applications/Reoria.Server/Reoria.Server.csproj

# Copy everything else
COPY . .

# Change to the project directory
WORKDIR /src/Applications/Reoria.Server

# Publish directly (skip separate build stage)
RUN dotnet publish Reoria.Server.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:PublishTrimmed=true

# =========================
# Runtime Stage
# =========================
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final

# Set the working directory
WORKDIR /app

# Set the user
USER $APP_UID

# Copy the application
COPY --from=build /app/publish .

# Define the entry point
ENTRYPOINT ["dotnet", "Reoria.Server.dll"]
