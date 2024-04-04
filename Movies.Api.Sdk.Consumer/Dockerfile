FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Movies.Api.Sdk.Consumer/Movies.Api.Sdk.Consumer.csproj", "Movies.Api.Sdk.Consumer/"]
RUN dotnet restore "Movies.Api.Sdk.Consumer/Movies.Api.Sdk.Consumer.csproj"
COPY . .
WORKDIR "/src/Movies.Api.Sdk.Consumer"
RUN dotnet build "Movies.Api.Sdk.Consumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Movies.Api.Sdk.Consumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Movies.Api.Sdk.Consumer.dll"]
