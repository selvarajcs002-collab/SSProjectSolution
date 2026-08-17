# Use the official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj and restore any dependencies (via NuGet)
COPY ["SSProjectSolution.csproj", "./"]
RUN dotnet restore "SSProjectSolution.csproj"

# Copy the remaining files and build the app
COPY . .
WORKDIR "/src/."
RUN dotnet build "SSProjectSolution.csproj" -c Release -o /app/build

# Publish the app to a folder
FROM build AS publish
RUN dotnet publish "SSProjectSolution.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy the published output from the previous stage
COPY --from=publish /app/publish .

# Define the entry point for the container
ENTRYPOINT ["dotnet", "SSProjectSolution.dll"]
