//
// Authors:
//   Ben Motmans <ben.motmans@gmail.com>
//
// Copyright (C) 2007 Ben Motmans
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Mono.Nat
{
	public static class NatUtility
	{
		public static event EventHandler<DeviceEventArgs> DeviceFound;
		public static event EventHandler<DeviceEventArgs> DeviceLost;
		
		private static List<INatController> controllers;

		static NatUtility ()
		{
			controllers = new List<INatController> ();
		}
		
		public static IEnumerable<INatController> Controllers
		{
			get { return controllers; }
		}

		public static void AddController (INatController controller)
		{
			if (controller == null)
				throw new ArgumentNullException ("controller");

			controller.DeviceFound += delegate (object sender, DeviceEventArgs args) {
				if (DeviceFound != null)
					DeviceFound (sender, args);
			};
			controller.DeviceLost += delegate (object sender, DeviceEventArgs args) {
				if (DeviceLost != null)
					DeviceLost (sender, args);
			};
				
			controllers.Add (controller);
		}
		
		public static void StartDiscovery ()
		{
			foreach (INatController controller in controllers)
				controller.StartDiscovery ();
		}

		public static void StopDiscovery ()
		{
			foreach (INatController controller in controllers)
				controller.StopDiscovery ();
		}
		
		public static IPAddress[] GetLocalAddresses (bool includeIPv6)
		{
			List<IPAddress> addresses = new List<IPAddress> ();

			IPHostEntry hostInfo = Dns.GetHostEntry (Dns.GetHostName ());
			foreach (IPAddress address in hostInfo.AddressList) {
				if (address.AddressFamily == AddressFamily.InterNetwork ||
					(includeIPv6 && address.AddressFamily == AddressFamily.InterNetworkV6)) {
					addresses.Add (address);
				}
			}
			
			return addresses.ToArray ();
		}
		
		//checks if an IP address is a private address space as defined by RFC 1918
		public static bool IsPrivateAddressSpace (IPAddress address)
		{
			byte[] ba = address.GetAddressBytes ();

			switch ((int)ba[0]) {
			case 10:
				return true; //10.x.x.x
			case 172:
				return ((int)ba[1] & 16) != 0; //172.16-31.x.x
			case 192:
				return (int)ba[1] == 168; //192.168.x.x
			default:
				return false;
			}
		}
	}
}