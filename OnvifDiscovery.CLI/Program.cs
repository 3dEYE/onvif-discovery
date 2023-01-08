using System;
using System.Threading;
using System.Threading.Tasks;
using OnvifDiscovery.Models;

using Serilog;

namespace OnvifDiscovery.CLI
{
	class Program
	{
		static async Task Main ()
		{
			Log.Logger = new LoggerConfiguration ()
				.WriteTo.Console ()
				.CreateLogger();

			Log.Information ("Starting Discover ONVIF cameras for 10 seconds, press Ctrl+C to abort\n");

			var cts = new CancellationTokenSource ();
			Console.CancelKeyPress += (s, e) => {
				e.Cancel = true;
				cts.Cancel ();
			};

			var discovery = new Discovery ();
			await discovery.Discover (10, OnNewDevice, cts.Token);

			Log.Information ("ONVIF Discovery finished");

			Log.CloseAndFlush ();
		}

		private static void OnNewDevice (DiscoveryDevice device)
		{
			Log.Information ("Device discovered: {@device}", device);
		}
	}
}
