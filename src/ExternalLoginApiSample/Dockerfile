FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ExternalLoginApiSample/ExternalLoginApiSample.csproj", "ExternalLoginApiSample/"]
RUN dotnet restore "ExternalLoginApiSample/ExternalLoginApiSample.csproj"
COPY . .
WORKDIR "/src/src/ExternalLoginApiSample"
RUN dotnet build "ExternalLoginApiSample.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ExternalLoginApiSample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExternalLoginApiSample.dll"]