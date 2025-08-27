FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AspNetCoreApi2Sample/AspNetCoreApi2Sample.csproj", "AspNetCoreApi2Sample/"]
RUN dotnet restore "AspNetCoreApi2Sample/AspNetCoreApi2Sample.csproj"
COPY . .
WORKDIR "/src/src/AspNetCoreApi2Sample"
RUN dotnet build "AspNetCoreApi2Sample.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AspNetCoreApi2Sample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspNetCoreApi2Sample.dll"]