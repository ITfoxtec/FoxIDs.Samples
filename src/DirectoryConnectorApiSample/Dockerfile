FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DirectoryConnectorApiSample/DirectoryConnectorApiSample.csproj", "DirectoryConnectorApiSample/"]
COPY ["src/FoxIDs.SampleHelperLibrary/FoxIDs.SampleHelperLibrary.csproj", "FoxIDs.SampleHelperLibrary/"]
RUN dotnet restore "DirectoryConnectorApiSample/DirectoryConnectorApiSample.csproj"
COPY . .
WORKDIR "/src/src/DirectoryConnectorApiSample"
RUN dotnet build "DirectoryConnectorApiSample.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DirectoryConnectorApiSample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectoryConnectorApiSample.dll"]
