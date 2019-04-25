docker build -t smartmetertoinfluxdb .
docker rm smartmetertoinfluxdb
docker run --name smartmetertoinfluxdb smartmetertoinfluxdb
docker stats