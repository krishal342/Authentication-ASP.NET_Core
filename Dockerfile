# Stage 1: Runtime base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: SDK image to compile and build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the csproj file and restore dependencies
COPY ["Authentication.csproj", "."]
RUN dotnet restore "Authentication.csproj"

# Copy the remaining source code and build
COPY . .

# Stage 3: Publish the compiled app
RUN dotnet publish "Authentication.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final container image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Run the application as a non-root 'app' user for security
USER app

ENTRYPOINT ["dotnet", "Authentication.dll"]