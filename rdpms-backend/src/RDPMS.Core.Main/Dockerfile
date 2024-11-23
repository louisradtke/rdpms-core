FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RDPMS.Core.Main/RDPMS.Core.Main.csproj", "RDPMS.Core.Main/"]
RUN dotnet restore "RDPMS.Core.Main/RDPMS.Core.Main.csproj"
COPY . .
WORKDIR "/src/RDPMS.Core.Main"
RUN dotnet build "RDPMS.Core.Main.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RDPMS.Core.Main.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RDPMS.Core.Main.dll"]
