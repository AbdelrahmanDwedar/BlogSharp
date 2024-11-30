# Use the appropriate ASP.NET runtime base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Use the appropriate ASP.NET SDK base image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BlogSharp.csproj", "BlogSharp/"]
RUN dotnet restore "BlogSharp/BlogSharp.csproj"
COPY . .
WORKDIR "/src/BlogSharp"
RUN dotnet build "BlogSharp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlogSharp.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlogSharp.dll"]
