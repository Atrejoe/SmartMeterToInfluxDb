FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY *.sln ./
COPY *.dcproj ./
COPY SmartMeterToInfluxDb/SmartMeterToInfluxDb.csproj SmartMeterToInfluxDb/
RUN dotnet restore
COPY . .
WORKDIR /src/SmartMeterToInfluxDb
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

ENTRYPOINT ["dotnet", "SmartMeterToInfluxDb.dll"]
