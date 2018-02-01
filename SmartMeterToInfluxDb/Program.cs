using System;
using System.Collections.Generic;
//using System.IO.Ports;
using DsmrParser.Models;

namespace SmartMeterToInfluxDb
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			var portNames = GetPortNames();
			Console.WriteLine($"Ports: {(string.Join(",", portNames))}");

			//correct to serialport
			try
			{
				var _serialPort = new RJCP.IO.Ports.SerialPortStream();

				var p = new DsmrParser.Dsmr.Parser();
				p.ParseFromStream(_serialPort, onParsed);
			}
			catch (Exception ex)
			{
				var old = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"{ex}");
				Console.ForegroundColor = old;
			}

			if (Environment.UserInteractive
				&& !Console.IsOutputRedirected)
			{
				Console.WriteLine("Press any key to exit");
				Console.ReadKey();
			}
			else
			{
				Console.WriteLine($"Exiting {100} seconds");
				System.Threading.Thread.Sleep(10 * 10000);
			}

		}

		private static void onParsed(object sender, Telegram telegram)
		{
			//Whatever, write to Influxdb here
			var i = new InfluxDB.LineProtocol.LineProtocolWriter();

			var c = new InfluxDB.LineProtocol.Client.LineProtocolClient(new Uri(""), "");
			var payload = new InfluxDB.LineProtocol.Payload.LineProtocolPayload();
			//payload.Add
			//c.WriteAsync();

			//i.Measurement("Tariff").v.Timestamp(telegram.Timestamp);
		}

		//see also

		//https://github.com/Ellerbach/serialapp
		public static string[] GetPortNames()
		{
			int p = (int)Environment.OSVersion.Platform;
			var serial_ports = new List<string>();

			// Are we on Unix?
			if (p == 4 || p == 128 || p == 6)
			{
				string[] ttys = System.IO.Directory.GetFiles(@"/dev/", @"tty\*");
				foreach (string dev in ttys)
				{
					if (dev.StartsWith("/dev/ttyS")
						|| dev.StartsWith("/dev/ttyUSB")
						|| dev.StartsWith("/dev/ttyACM")
						|| dev.StartsWith("/dev/ttyAMA"))
					{
						serial_ports.Add(dev);
						Console.WriteLine($"Serial list: {dev}");
					}
					else
						Console.WriteLine($"Rejected serial device: {dev}");
				}
			}
			return serial_ports.ToArray();
		}
	}
}
