# SmartMeterToInfluxDb

An attempt to read Dutch smart meter information and write it to an Influxdb instance, using .Net Core

You can build it yourself using `dockerize.bat/sh`, but it's also available also available as a Linux Docker image: https://hub.docker.com/r/atreyu/smartmetertoinfluxdb/

>NOTE This is very much in alpha, I'm having connection issues to InfluxDb, but the serial port reading works (!)

## Requirements

- A Dutch Smart meter with a P1 port
- A Device running [.Net Core runtime 2.1](<https://dotnet.microsoft.com/download/dotnet-core/2.1>)+ **OR** Docker
- A Dutch Smart meter cable, like [here](<https://www.slimmemeterkabel.nl/>), [here](<https://www.sossolutions.nl/slimme-meter-kabel>) or [Google it yourself](https://www.google.com/search?q=dutch+smart+meter+cable)

## Configuration

Configuration is done for now using the following environment variables:

```c#
SerialPortName*       //on Linux usually /dev/tty0
BaudRate              //default: 115200 ()

InfluxDbUserName*
InfluxDbPassword*
InfluxDbServerAddress //default: http://127.0.0.1:8086
InfluxDbDatabaseName*
```

## Dependencies

Beside the usual, the library depends on:

- [NetCoreSerial](<https://github.com/Ellerbach/serialapp>)
- [DsmrParser](<https://github.com/peckham/DsmrParser>)
- [InfluxDB .NET Collector](<https://github.com/influxdata/influxdb-csharp>)

## Usage

### .Net Core

Untested, something like this?
```
dotnet run SmartMeterToInfluxDb
```


### Docker

Docker image is available at:

https://hub.docker.com/r/atreyu/smartmetertoinfluxdb/

1. Create `docker-compose.yml` example:
	```yaml
	services:
	  smartmetertoinfluxdb:
	    restart: unless-stopped
	    image: atreyu/smartmetertoinfluxdb:dev
	    container_name: smartmetertoinfluxdb
	    devices:
	      - /dev/ttyUSB0:/dev/ttyUSB0
	    environment:
	      - SerialPortName=/dev/ttyUSB0
	      - BaudRate=115200
	      - InfluxDbUserName=MyUsername
	      - InfluxDbPassword=MyPa$$w0rd
	      - InfluxDbServerAddress=http://127.0.0.1:8086
	      - InfluxDbDatabaseName=SmartMeter
	```
2.Run `docker-compose up` or `docker-compose up -d` to run as a deamon.




