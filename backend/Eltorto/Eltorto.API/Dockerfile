FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
#EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Eltorto.API/Eltorto.API.csproj", "Eltorto.API/"]
COPY ["Eltorto.Domain/Eltorto.Domain.csproj", "Eltorto.Domain/"]
COPY ["Eltorto.Infrastructure/Eltorto.Infrastructure.csproj", "Eltorto.Infrastructure/"]
COPY ["Eltorto.Application/Eltorto.Application.csproj", "Eltorto.Application/"]
RUN dotnet restore "Eltorto.API/Eltorto.API.csproj"

COPY . .

WORKDIR "/src/Eltorto.API"
RUN dotnet build "./Eltorto.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Eltorto.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Eltorto.API.dll"]