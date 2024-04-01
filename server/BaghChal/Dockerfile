FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BaghChal/BaghChal.csproj", "BaghChal/"]
# COPY ["Migrations/Migrations.csproj", "Migrations/"]
RUN dotnet restore "BaghChal/BaghChal.csproj"
COPY . .
WORKDIR "/src/BaghChal"
RUN dotnet build "BaghChal.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BaghChal.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BaghChal.dll"]
