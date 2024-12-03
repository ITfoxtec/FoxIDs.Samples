FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AspNetCoreApi1Sample/AspNetCoreApi1Sample.csproj", "AspNetCoreApi1Sample/"]
RUN dotnet restore "AspNetCoreApi1Sample/AspNetCoreApi1Sample.csproj"
COPY . .
WORKDIR "/src/src/AspNetCoreApi1Sample"
RUN dotnet build "AspNetCoreApi1Sample.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AspNetCoreApi1Sample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY "src/AspNetCoreApi1Sample/CN=AspNetCoreApi1Sample, O=test corp.pfx" .
ENTRYPOINT ["dotnet", "AspNetCoreApi1Sample.dll"]