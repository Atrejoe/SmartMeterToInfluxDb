 docker build -t smartmetertoinfluxdb .
 docker rm smartmetertoinfluxdb
 docker run --name smartmetertoinfluxdb --device=/dev/ttyUSB0 smartmetertoinfluxdb