
/*
	Genetic Algorithm Framework for .Net
	Copyright (C) 2016  John Newcombe

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

		You should have received a copy of the GNU Lesser General Public License
		along with this program.  If not, see <http://www.gnu.org/licenses/>.

	http://johnnewcombe.net
*/

#if !PCL

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GAF.Net
{
	/// <summary>
	/// Synchronous socket client.
	/// </summary>
	[Obsolete ("This class is deprecated. The similarly named class within the GAF.Network package, should be used instead.")]
	public static class SocketClient
	{
		private const int headerSize = 4;
		private const int maxRetries = 10;
		private const int pidEtx = 15;

		/// <summary>
		/// Connect to the specified ipAddress and port.
		/// </summary>
		/// <param name="ipAddress">Ip address.</param>
		/// <param name="port">Port.</param>
		public static Socket Connect (IPAddress ipAddress, int port)
		{
			try {
				if (ipAddress == null)
					throw new ArgumentNullException ("nameof (ipAddress)");
				if (port < 1024)
					throw new ArgumentOutOfRangeException ("nameof (port)");
				
				// Establish the remote endpoint
				IPEndPoint remoteEndPoint = new IPEndPoint (ipAddress, port);
				return Connect (remoteEndPoint);
			
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Connect to the specified remoteEndPoint.
		/// </summary>
		/// <param name="remoteEndPoint">Remote end point.</param>
		public static Socket Connect (IPEndPoint remoteEndPoint)
		{
			Socket client = null;

			try {
				var retries = 0;

				while ((client == null || !client.Connected) && retries < maxRetries) {

					if (remoteEndPoint == null)
						throw new ArgumentNullException ("nameof (remoteEndPoint)");

					// Create a TCP/IP  socket.
					client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

					// Connect the socket to the remote endpoint. Catch any errors.
					client.Connect (remoteEndPoint);

					retries++;
				}

				if (!client.Connected) {
					throw new GAF.Exceptions.SocketException (string.Format ("Unable to connect to endpoint after {0} attempts.", maxRetries));
				}
					
			} catch (Exception ex) {
				throw ex;
			}

			return client;

		}

		/// <summary>
		/// Transmits the data.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="client">Client.</param>
		/// <param name="packet">Packet.</param>
		public static Packet TransmitData (Socket client, Packet packet)
		{
			try {
				
				client.Send (packet.ToByteArray ());
				byte[] bytes = new byte[1024];

				// Receive the response from the remote device.
				var bytesReceived = client.Receive (bytes);

				if (bytesReceived > PacketHeader.HeaderLength) {
					
					var tmp = new byte[bytesReceived];
					Array.Copy (bytes, 0, tmp, 0, bytesReceived);
					return Packet.CreatePacket (bytes);				
				}

				return null;

			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// Transmits an ETX Packet.
		/// </summary>
		/// <param name="client">Client.</param>
		public static void TransmitETX (Socket client)
		{
			TransmitData (client, new Packet (new byte[0], PacketId.Etx, new Guid ()));
		}

		/// <summary>
		/// Close the specified client.
		/// </summary>
		/// <param name="client">Client.</param>
		public static bool Close (Socket client)
		{
			try {

				client.LingerState = new LingerOption (true, 1);
				//client.Shutdown (SocketShutdown.Both);
				client.Close ();


			} catch (Exception ex) {
				throw ex;
			}

			return !client.Connected;
		}
	}
}

#endif
