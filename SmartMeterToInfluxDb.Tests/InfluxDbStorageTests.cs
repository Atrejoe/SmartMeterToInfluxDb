using DsmrParser.Models;
using System;
using Xunit;

namespace SmartMeterToInfluxDb.Tests
{
    public class InfluxDbStorageTests
    {
        [Fact(Skip = "This is an integration test, I need to handle secrets")]
        public void StoreTelegramTest()
        {
            //arrange
            Environment.SetEnvironmentVariable(nameof(Config.InfluxDbServerAddress), new Uri("http://192.168.1.110:8086").ToString());
            Environment.SetEnvironmentVariable(nameof(Config.InfluxDbUserName), "InfluxDbUserName");
            Environment.SetEnvironmentVariable(nameof(Config.InfluxDbPassword), "InfluxDbPassword");
            Environment.SetEnvironmentVariable(nameof(Config.InfluxDbDatabaseName), "SmartMeterToInfluxDb");

            var telegram = new Telegram()
            {
                InstantaneousElectricityUsage = 1.2345m,
                Timestamp = DateTime.UtcNow,
                GasUsage = 42,
                GasTimestamp = DateTime.UtcNow
            };

            //act
            InfluxDbStorage.StoreTelegram(telegram, default);

            //assert
        }
    }
}
