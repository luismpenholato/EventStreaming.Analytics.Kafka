FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Services/Query/Analytics.QueryApi/Analytics.QueryApi.csproj
RUN dotnet publish src/Services/Query/Analytics.QueryApi/Analytics.QueryApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Analytics.QueryApi.dll"]
