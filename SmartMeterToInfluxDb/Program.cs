using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace SmartMeterToInfluxDb
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Smart meter to InfluxDb started at: {DateTime.Now}!");

            Config config = GetConfig();

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (_, e) =>
                cts.Cancel();

            try
            {
                //todo: make port name configurable
                var portNames = SerialDevice.GetPortNames();
                string portName = string.IsNullOrWhiteSpace(config.SerialPortName) ? portNames.FirstOrDefault() : config.SerialPortName;

                Console.WriteLine($"Listening to {portName}");

                using (var _serialPort = new SerialDevice(portName, config.BaudRate))
                {
                    var sb = new StringBuilder();
                    using (var s = new MemoryStream())
                    using (var bs = new BufferedStream(s))
                    {

                        var p = new DsmrParser.Dsmr.Parser();

                        //Subscribe to data
                        //Unfortunately the port does not provide a real stream
                        _serialPort.DataReceived += (sender, data) =>
                        {
                            Console.Write('.');
                            if (data.Length > 0)
                            {
                                var text = Encoding.ASCII.GetString(data);

                                if (text.Contains('/'))
                                {
                                    sb.Clear();
                                    sb.AppendLine();
                                    sb.Append(text.Substring(text.IndexOf('/')));
                                }
                                else
                                    sb.Append(text);

                                if (text.Contains('!'))
                                {
                                    Console.WriteLine($"Parsing telegrams");
                                    var telegrams = p.Parse(sb.ToString()).Result;
                                    //var output = string.Join('\n', telegrams.Select(x => JsonConvert.SerializeObject(x, Formatting.Indented)));
                                    //if (string.IsNullOrWhiteSpace(output))
                                    //    output = sb.ToString();

                                    if (telegrams.Any())
                                    {
                                        foreach (var telegram in telegrams)
                                            InfluxDbStorage.StoreTelegram(telegram, cts.Token);

                                        Console.WriteLine($"Last telegram timestamp stored : {telegrams.Last().Timestamp}");
                                        bs.SetLength(0);
                                    } else
                                        Console.WriteLine($"End-of-message detected, but no telegrams parsed");
                                }

                                bs.Write(data);
                            }
                            else
                            {
                                bs.Position = 0;
                                p.ParseFromStream(bs, (ssender, telegram) =>
                                {
                                    InfluxDbStorage.StoreTelegram(telegram, cts.Token);

                                    Console.WriteLine($"Last telegram timestamp stored : {telegram.Timestamp}");
                                });
                                bs.Position = bs.Length;
                                bs.SetLength(0);
                            }
                        };

                        _serialPort.Open();

                        while (!cts.Token.IsCancellationRequested)
                            cts.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(20));
                    }
                }
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
                Console.WriteLine($"Exiting in {5} seconds");
                Thread.Sleep(5 * 1000);
            }

        }

        internal static Config GetConfig()
        {
            var builder = new ConfigurationBuilder()
                                     .AddEnvironmentVariables();

            var c = builder.Build();

            var config = new Config();

            //foreach (var setting in c.AsEnumerable())
            //    Console.WriteLine($"{setting.Key}={setting.Value}");

            c.Bind(config);
            return config;
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            throw new OperationCanceledException();
        }

        //see also

        ////https://github.com/Ellerbach/serialapp
        //public static string[] GetPortNames()
        //{
        //	int p = (int)Environment.OSVersion.Platform;
        //	var serial_ports = new List<string>();

        //	// Are we on Unix?
        //	if (p == 4 || p == 128 || p == 6)
        //	{
        //		string[] ttys = System.IO.Directory.GetFiles(@"/dev/", @"tty\*");
        //		foreach (string dev in ttys)
        //		{
        //			if (dev.StartsWith("/dev/ttyS")
        //				|| dev.StartsWith("/dev/ttyUSB")
        //				|| dev.StartsWith("/dev/ttyACM")
        //				|| dev.StartsWith("/dev/ttyAMA"))
        //			{
        //				serial_ports.Add(dev);
        //				Console.WriteLine($"Serial list: {dev}");
        //			}
        //			else
        //				Console.WriteLine($"Rejected serial device: {dev}");
        //		}
        //	}
        //	return serial_ports.ToArray();
        //}
    }
}
