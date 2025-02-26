# Base image for runtime dependencies
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Expose necessary ports

# WebSocketHandler
EXPOSE 5000
# ROV connection
EXPOSE 6900
EXPOSE 5017
EXPOSE 5018

# Install SDL2 runtime dependencies
RUN apt-get update && apt-get install -y \
    libsdl2-2.0-0 \
    joystick \
    && rm -rf /var/lib/apt/lists/*

# Set ASP.NET Core to listen on all interfaces
#ENV ASPNETCORE_URLS=http://0.0.0.0:5000

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install SDL2 development libraries for building
RUN apt-get update && apt-get install -y \
    libsdl2-2.0-0 libsdl2-dev joystick \
    && rm -rf /var/lib/apt/lists/*

# Copy project files and restore dependencies
COPY Backend.csproj Program.cs ./
RUN dotnet restore "Backend.csproj"

# Copy the entire source code and build
COPY . .
RUN dotnet publish "Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM base AS final
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Set environment variables

# Allows SDL to run in headless mode if needed
ENV SDL_VIDEODRIVER=dummy

# Ensure 'input' group exists before adding users
RUN groupadd -r input && usermod -a -G input root

# Start the backend application
ENTRYPOINT ["dotnet", "Backend.dll"]

