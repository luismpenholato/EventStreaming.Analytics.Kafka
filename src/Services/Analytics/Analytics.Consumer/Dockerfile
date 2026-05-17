FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Services/Analytics/Analytics.Consumer/Analytics.Consumer.csproj
RUN dotnet publish src/Services/Analytics/Analytics.Consumer/Analytics.Consumer.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Analytics.Consumer.dll"]
