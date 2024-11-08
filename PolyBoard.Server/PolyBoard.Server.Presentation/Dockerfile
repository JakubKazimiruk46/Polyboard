FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Development
WORKDIR /src
COPY ["PolyBoard.Server.Presentation/PolyBoard.Server.Presentation.csproj", "PolyBoard.Server.Presentation/"]
COPY ["PolyBoard.Server.Application/PolyBoard.Server.Application.csproj", "PolyBoard.Server.Application/"]
COPY ["PolyBoard.Server.Core/PolyBoard.Server.Core.csproj", "PolyBoard.Server.Core/"]
COPY ["PolyBoard.Server.Infrastructure/PolyBoard.Server.Infrastructure.csproj", "PolyBoard.Server.Infrastructure/"]
RUN dotnet restore "PolyBoard.Server.Presentation/PolyBoard.Server.Presentation.csproj"
COPY . .
WORKDIR "/src/PolyBoard.Server.Presentation"
RUN dotnet build "PolyBoard.Server.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Development
RUN dotnet publish "PolyBoard.Server.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PolyBoard.Server.Presentation.dll"]
