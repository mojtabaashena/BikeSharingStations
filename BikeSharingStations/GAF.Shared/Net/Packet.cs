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

namespace GAF.Net
{
	/// <summary>
	/// Transmission Packet.
	/// </summary>
	[Obsolete ("This class is deprecated. The similarly named class within the GAF.Network package, should be used instead.")]
	public class Packet
	{


		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Net.Packet"/> class.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name = "packetId"></param>
		/// <param name="objectId">Object identifier.</param>
		public Packet (byte[] data, PacketId packetId, Guid objectId)
		{
			if (data != null) {

				var dataLength = data.GetLength (0);

				Header = new PacketHeader (packetId, objectId, dataLength);
				Data = data;

			} else {
				//create an empty data array (header already empty)
				Data = new byte[0];
			}
		}

//		public Packet (byte[] data, PacketHeader header)
//		{
//			if (data != null) {
//				Header = header;
//				Data = data;
//			} else {
//				//create an empty data array (header already empty)
//				Data = new byte[0];
//			}
//
//		}
		/// <summary>
		/// Factopry method to create a new packet based on raw data. The argument rawBytes should contain 
		/// both header and data. The number of data bytes should be at least as long as that specified 
		/// in the header.
		/// </summary>
		/// <returns>A new instance of the Packet type.</returns>
		/// <param name="rawBytes">Raw bytes.</param>
		internal static Packet CreatePacket(byte[] rawBytes)
		{
			// header consists of 
			//soh (Guid) 16 bytes
			//pid (byte) 1 byte
			//oid (Guid) 16 bytes
			//length (int) 4 bytes
			if (rawBytes.Length < PacketHeader.HeaderLength) {
			
				throw new ArgumentOutOfRangeException ("rawBytes", "The length of data is too small to form a valid packet.");
			}
				
			var headerBytes = new byte[PacketHeader.HeaderLength];
			Array.Copy (rawBytes, 0, headerBytes, 0, PacketHeader.HeaderLength);

			var header = new PacketHeader (headerBytes);

			//determine the data length of the incomming data and compare to that specified in the header
			var actualDataLength = rawBytes.GetLength (0)-PacketHeader.HeaderLength;
			if (actualDataLength < header.DataLength) {
				throw new ArgumentException ("Length of the data is less than that specified in the header.", "rawBytes");
			}

			var data= new byte[header.DataLength];
			Array.Copy (rawBytes, PacketHeader.HeaderLength, data, 0, header.DataLength);

			return new Packet (data, (PacketId)header.PacketId, header.ObjectId);

		}

		/// <summary>
		/// Gets or sets the header.
		/// </summary>
		/// <value>The header.</value>
		public PacketHeader Header { private set; get; }

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		/// <value>The data.</value>
		public byte[] Data { get; private set; }


		/// <summary>
		/// Gets the bytes.
		/// </summary>
		/// <returns>The bytes.</returns>
		public byte[] ToByteArray ()
		{
			//create an array for the data and the header
			var byteData = new byte[PacketHeader.HeaderLength + Header.DataLength];

			//concatenate the header and data
			Array.Copy (Header.ToByteArray(), 0, byteData, 0, PacketHeader.HeaderLength);
			Array.Copy (Data, 0, byteData, PacketHeader.HeaderLength, Header.DataLength);

			return byteData;
		}
	}
}

#endif