FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AspNetCoreSamlIdPSample/AspNetCoreSamlIdPSample.csproj", "AspNetCoreSamlIdPSample/"]
RUN dotnet restore "AspNetCoreSamlIdPSample/AspNetCoreSamlIdPSample.csproj"
COPY . .
WORKDIR "/src/src/AspNetCoreSamlIdPSample"
RUN dotnet build "AspNetCoreSamlIdPSample.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AspNetCoreSamlIdPSample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY "src/AspNetCoreSamlIdPSample/Certificates/." "Certificates/"
ENTRYPOINT ["dotnet", "AspNetCoreSamlIdPSample.dll"]