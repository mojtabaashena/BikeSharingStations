
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
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace GAF.Net
{
	/// <summary>
	/// State object for receiving data from remote device.
	/// </summary>
	internal class StateObject {

		/// <summary>
		/// Client socket.
		/// </summary>
		public Socket WorkSocket = null;

		/// <summary>
		/// The size of the data buffer.
		/// </summary>
		public const int BufferSize = 1024;

		/// <summary>
		/// Data buffer.
		/// </summary>
		public byte[] Buffer = new byte[BufferSize];

		/// <summary>
		/// Packet Manager.
		/// </summary>
		public PacketManager PacMan = new PacketManager (new SynchronisedQueue (8192));


	}
}
#endif

