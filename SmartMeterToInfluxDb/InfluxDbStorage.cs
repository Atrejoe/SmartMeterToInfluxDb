using DsmrParser.Models;
using InfluxDB.Collector.Diagnostics;
using InfluxDB.LineProtocol;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SmartMeterToInfluxDb
{
    public static class InfluxDbStorage
    {

        public static void StoreTelegram(Telegram telegram, CancellationToken token)
        {
            Console.WriteLine($"Obtaining config");

            var c = Program.GetConfig();

            StoreTelegram(c, telegram, token);
        }

        public static void StoreTelegram(Config config, Telegram telegram, CancellationToken token)
        {
            Console.WriteLine($"Storing telegram {telegram}");

            Console.WriteLine($"Creating linked cancellationtoken source");

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                Console.WriteLine($"Setting cancellation to 30 secs");

                cts.CancelAfter(TimeSpan.FromSeconds(30));

                //var host = IPAddress.Parse("192.168.1.110");

                //Metrics.Collector = new CollectorConfiguration()
                //    .Tag.With("host", Environment.GetEnvironmentVariable("COMPUTERNAME"))
                //    .Batch.AtInterval(TimeSpan.FromSeconds(2))
                //    .WriteTo.InfluxDB(serverBaseAddress: $"http://192.168.1.110:8086", database: "home_assistant", influxdb_username, influxdb_password)
                //    .CreateCollector();

                Console.WriteLine($"Registering error handler");

                CollectorLog.RegisterErrorHandler((message, exception) =>
                {
                    //Console.WriteLine($"{message}: {exception}");
                    throw exception;
                });

                //Metrics.Measure("usage", telegram.InstantaneousElectricityUsage);

                Console.WriteLine($"Creating line protocol writer");

                var i = new LineProtocolWriter();

                Console.WriteLine($"Getting lineprotocal client, to connect to {config.InfluxDbServerAddress}");

                var c = new LineProtocolClient(
                    serverBaseAddress: config.InfluxDbServerAddress,
                    database: config.InfluxDbDatabaseName,
                    username: config.InfluxDbUserName,
                    password: config.InfluxDbPassword);

                Console.WriteLine("Gathering payload");

                var payload = new LineProtocolPayload();
                payload.Add(GetPoint("A", telegram.InstantaneousCurrent, "power_current", "Current", "mdi:flash", telegram.Timestamp));
                payload.Add(GetPoint("kW", telegram.InstantaneousElectricityUsage, "power_consumption", "Power Consumption", "mdi:flash", telegram.Timestamp));
                payload.Add(GetPoint("kW", telegram.InstantaneousElectricityDelivery, "power_production", "Power Production", "mdi:flash", telegram.Timestamp));
                payload.Add(GetPoint("kWh", telegram.PowerConsumptionTariff1, "power_consumption_low", "Power Consumption (low)", "mdi:flash", telegram.Timestamp));
                payload.Add(GetPoint("kWh", telegram.PowerConsumptionTariff2, "power_consumption_normal", "Power Production (normal)", "mdi:flash", telegram.Timestamp));
                payload.Add(GetPoint("kWh", telegram.PowerproductionTariff1, "power_production_low", "Power Consumption (low)", "mdi:flash", telegram.Timestamp));
                payload.Add(GetPoint("kWh", telegram.PowerproductionTariff2, "power_production_normal", "Power Production (normal)", "mdi:flash", telegram.Timestamp));

                payload.Add(GetPoint("m3", telegram.GasUsage, "gas_consumption", "Gas Consumption", "mdi:fire", telegram.GasTimestamp));

                Console.WriteLine("Writing payload");

                LineProtocolWriteResult result;
                try
                {
                    result = c.WriteAsync(payload, cts.Token).Result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write to {config.InfluxDbDatabaseName}@{config.InfluxDbServerAddress} : {ex.Message}");
                    return;
                }

                if (!result.Success)
                    throw new Exception(result.ErrorMessage);

                Console.WriteLine(result.ToString());
            }
        }

        private static LineProtocolPoint GetPoint<T>(
            string measurement,
            T value,
            string entity,
            string friendlyName,
            string icon,
            DateTime? timestamp = null)
        {
            return new LineProtocolPoint(
                measurement: measurement,
                fields: new Dictionary<string, object>
                {
                    { "value", value },
                    { "friendly_name_str", friendlyName },
                    { "hidden", 0.0 },
                    { "icon_str", icon },
                },
                tags: new Dictionary<string, string>
                {
                    { "entity_id", entity },
                    { "domain", "sensor"},
                    { "instance", "prod"},
                    { "source", "hass"},
                },
                utcTimestamp: timestamp);

            //todo: convert to utc
        }
    }
}
