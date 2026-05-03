# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Stage 2: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SSProjectSolution.csproj", "./"]
RUN dotnet restore "SSProjectSolution.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "SSProjectSolution.csproj" -c Release -o /app/build

# Stage 3: Publish stage
FROM build AS publish
RUN dotnet publish "SSProjectSolution.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create the directory for PDF generation and set permissions
RUN mkdir -p /app/outputs/dc && chmod 777 /app/outputs/dc

# Cloud Run uses the PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT:-80}

ENTRYPOINT ["dotnet", "SSProjectSolution.dll"]
