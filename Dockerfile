# Use the appropriate ASP.NET runtime base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the appropriate ASP.NET SDK base image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project files first to leverage Docker layer caching
COPY ./*.csproj BlogSharp/
RUN dotnet restore "BlogSharp/BlogSharp.csproj"

# Copy the rest of the application source code
COPY ./ BlogSharp/
WORKDIR "/src/BlogSharp"
RUN dotnet build "BlogSharp.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "BlogSharp.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlogSharp.dll"]
