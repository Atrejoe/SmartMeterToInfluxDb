FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY *.sln ./
#COPY docker-compose.dcproj ./
COPY SmartMeterToInfluxDb/SmartMeterToInfluxDb.csproj SmartMeterToInfluxDb/
RUN dotnet restore SmartMeterToInfluxDb/SmartMeterToInfluxDb.csproj
COPY . .
#COPY *.dcproj ./
WORKDIR /src/SmartMeterToInfluxDb
#COPY *.dcproj ./
RUN dotnet build SmartMeterToInfluxDb.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish SmartMeterToInfluxDb.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SmartMeterToInfluxDb.dll"]
