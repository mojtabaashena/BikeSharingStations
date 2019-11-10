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
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace GAF.Net
{

	/// <summary>
	/// Asynchronous socket listener.
	/// </summary>
	[Obsolete ("This class is deprecated. The similarly named class within the GAF.Network package, should be used instead.")]
	public static class SocketListener
	{
		/// <summary>
		/// Delegate definition for the GenerationComplete event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void PacketReceivedHandler (object sender, PacketEventArgs e);

		/// <summary>
		/// Event definition for the GenerationComplete event handler.
		/// </summary>
		public static event PacketReceivedHandler OnPacketReceived;

		/// <summary>
		/// Thread Signal
		/// </summary>
		private static ManualResetEvent _allDone = new ManualResetEvent (false);

		private static int _packetCount = 0;
		private const int pidEtx = 15;

		/// <summary>
		/// Starts the listening.
		/// </summary>
		/// <param name = "ipAddress"></param>
		/// <param name="port">Port.</param>
		public static void StartListening (IPAddress ipAddress, int port)
		{
			IPEndPoint localEndPoint = new IPEndPoint (ipAddress, port);

			// Create a TCP/IP socket.
			Socket listener = new Socket (AddressFamily.InterNetwork,
				                  SocketType.Stream, ProtocolType.Tcp);


			// Bind the socket to the local endpoint and listen for incoming connections.

				listener.Bind (localEndPoint);
				listener.Listen (100);

				while (true) {
					// Set the event to nonsignaled state.
					_allDone.Reset ();

					// Start an asynchronous socket to listen for connections.
					listener.BeginAccept (
						new AsyncCallback (AcceptCallback),
						listener);

					// Wait until a connection is made before continuing.
					_allDone.WaitOne ();

				}


		}

		private static void AcceptCallback (IAsyncResult ar)
		{
			// Signal the main thread to continue.
			_allDone.Set ();

			// Get the socket that handles the client request.
			Socket listener = (Socket)ar.AsyncState;
			Socket handler = listener.EndAccept (ar);

			// Create the state object.
			StateObject state = new StateObject ();
			state.WorkSocket = handler;

			handler.BeginReceive (state.Buffer, 0, StateObject.BufferSize, 0,
				new AsyncCallback (ReadCallback), state);

		}

		private static void ReadCallback (IAsyncResult ar)
		{

			// Retrieve the state object and the handler socket
			// from the asynchronous state object.
			StateObject state = (StateObject)ar.AsyncState;
			Socket handler = state.WorkSocket;
			bool etxReceived = false;

				// Read data from the client socket. 
				int bytesRead = handler.EndReceive (ar);

				if (bytesRead > 0) {
			
					// At this point all of the read bytes are in the state.Buffer. The
					// state.Buffer is 1024 bytes in size, so bytes will need to be transferred to 
					// the PacketManager in 1024 byte chunks (or less if its the last chunk).
					//Console.WriteLine ("Read {0} bytes from socket.", bytesRead);

					state.PacMan.Add (state.Buffer);

					//see if we have any packets
					var packet = state.PacMan.GetPacket ();

					if (packet != null) {

						double fitness = 0.0;

						_packetCount++;

						if (OnPacketReceived != null) {

							var args = new PacketEventArgs (_packetCount, packet);
							OnPacketReceived (null, args);
							fitness = args.Result;

						}

						//have we received all for this transmission?
					etxReceived = packet.Header.PacketId == PacketId.Etx;

						//Packets other than 0 are control packets
						if (packet.Header.PacketId == 0) {
							Send (ar, new Packet (BitConverter.GetBytes (fitness), 0, packet.Header.ObjectId));
						} else {
							Send (ar, new Packet (new byte[0], 0, packet.Header.ObjectId));
						}
					}

					//if not end of transmission get more data
					if (!etxReceived) {
						state.WorkSocket.BeginReceive (state.Buffer, 0, StateObject.BufferSize, 0,
							new AsyncCallback (ReadCallback), state);
					}

			}
		}

		private static void Send (IAsyncResult ar, Packet data)
		{
			StateObject state = (StateObject)ar.AsyncState;
			Socket handler = state.WorkSocket;

			// Begin sending the data to the remote device.
			handler.BeginSend (data.ToByteArray (), 0, data.Header.DataLength + PacketHeader.HeaderLength, 0,
				new AsyncCallback (SendCallback), state);
			

		}

		private static void SendCallback (IAsyncResult ar)
		{

			StateObject state = (StateObject)ar.AsyncState;
			Socket handler = state.WorkSocket;

			try {
				
			// Complete sending the data to the remote device.
			handler.EndSend (ar);

				if (state.PacMan.LastPidReceived == PacketId.Etx) {
					handler.LingerState = new LingerOption (true, 1);
					handler.Close ();
				}

			} catch (Exception ex) {
				if (state.PacMan.LastPidReceived != PacketId.Etx) {
					throw ex;
				}
			}
		}

	}
}
#endif
