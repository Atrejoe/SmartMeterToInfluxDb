using System;
using System.IO.Ports;

namespace SmartMeterToInfluxDb
{
    public class Config
    {
        public string SerialPortName { get; set; }
        public BaudRate BaudRate { get; set; } = BaudRate.B115200;

        public string InfluxDbUserName { get; set; }
        public string InfluxDbPassword { get; set; }
        public Uri InfluxDbServerAddress { get; set; } = new Uri("http://127.0.0.1:8086");
        public string InfluxDbDatabaseName { get; set; }
    }
}
