using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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
			var deviceIp = device.Address;

			var devicePort =
				(from addr in device.XAdresses
				 let x = Uri.TryCreate (addr, UriKind.Absolute, out var uri) ? (isUri: true, uri) : (false, null)
				 where x.isUri && x.uri.Host == deviceIp
				 select x.uri.Port).DefaultIfEmpty (80).First ();

			var deviceMac =
				(from scope in device.Scopes
				 let x = Uri.TryCreate (scope, UriKind.Absolute, out var uri) ? (isUri: true, uri) : (false, null)
				 where x.isUri && x.uri.AbsolutePath.StartsWith("/mac", StringComparison.InvariantCultureIgnoreCase)
				 let macSegment = x.uri.Segments.Last()
				 let mac = Regex.Replace (macSegment, "-|:", "").ToUpperInvariant ()
				 select mac).FirstOrDefault ();

			var onvifFoundDevice = new OnvifFoundDevice (deviceIp, devicePort, device.Model, device.Mfr, deviceMac);

			Log.Information ("Device discovered: {device}", onvifFoundDevice);
		}
	}

	public record OnvifFoundDevice (string Host, int HttpPort, string Model, string Manufacturer, string MacAddress);
}
