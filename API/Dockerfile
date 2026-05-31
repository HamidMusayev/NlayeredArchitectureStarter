# Build the API image from the solution root:
#   docker build -t nlayered-starter-api -f API/Dockerfile .
# Or via docker compose (recommended):
#   docker compose -f API/docker-compose.yml up --build

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["global.json", "./"]
COPY ["API/API.csproj", "API/"]
COPY ["BLL/BLL.csproj", "BLL/"]
COPY ["CORE/CORE.csproj", "CORE/"]
COPY ["DTO/DTO.csproj", "DTO/"]
COPY ["ENTITIES/ENTITIES.csproj", "ENTITIES/"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["GRAPHQL/GRAPHQL.csproj", "GRAPHQL/"]
COPY ["MEDIATRS/MEDIATRS.csproj", "MEDIATRS/"]
COPY ["MESSAGEBUS/MESSAGEBUS.csproj", "MESSAGEBUS/"]
COPY ["NOTIFICATIONS/NOTIFICATIONS.csproj", "NOTIFICATIONS/"]
COPY ["OUTBOX/OUTBOX.csproj", "OUTBOX/"]
COPY ["REFITS/REFITS.csproj", "REFITS/"]
COPY ["STORAGE/STORAGE.csproj", "STORAGE/"]
RUN dotnet restore "API/API.csproj"
COPY . .
WORKDIR "/src/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "API.dll"]
