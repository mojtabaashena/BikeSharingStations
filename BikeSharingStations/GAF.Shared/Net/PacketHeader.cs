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
	/// Packet header.
	/// </summary>
	[Obsolete ("This class is deprecated. The similarly named class within the GAF.Network package, should be used instead.")]
	public class PacketHeader
	{
		// The Header consists of; 
		// SOH (16 byte GUID)
		// Packet ID (byte)
		// Object ID (16 byte GUID)
		// Length of data (4 byte int32)

		private const int _headerSize = 37;
		private const string _soh = "20f45c2b-2e16-4a46-87da-b257cb205dd9"; 

		private const int _indexOfSoh = 0;
		private const int _indexofDataLengthElement = 16;
		private const int _indexOfPid = 20;
		private const int _indexOfOid = 21;

		private byte[] _headerBytes;

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Net.PacketHeader"/> class.
		/// </summary>
		/// <param name="packetId">Packet identifier.</param>
		/// <param name="objectId">Object identifier.</param>
		/// <param name="dataLength">Data length.</param>
		public PacketHeader (PacketId packetId, Guid objectId, int dataLength)
		{

			// get the byte representation of the header elements
			PacketId = packetId;
			ObjectId = objectId;
			DataLength = dataLength;

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Net.PacketHeader"/> class.
		/// </summary>
		/// <param name="headerBytes">Header bytes.</param>
		public PacketHeader (byte[] headerBytes)
		{
			_headerBytes = headerBytes;

			if (_headerBytes.Length != HeaderLength) {
				throw new ArgumentOutOfRangeException ("headerBytes", string.Format("The header must be a byte array of length {0}", HeaderLength));
			}

			// header consists of 
			//soh (Guid) 16 bytes
			//length (int) 4 bytes
			//pid (byte) 1 byte
			//oid (Guid) 16 bytes

			var headerSoh = new byte[16]; //guid
			var headerOid = new byte[16]; //guid

			//get the header elements
			Array.Copy (headerBytes, _indexOfSoh, headerSoh, 0, headerSoh.Length);
			if (new Guid (headerSoh).ToString () != SOH.ToString ()) {
				throw new ArgumentException ("SOH is invalid.", "headerBytes");
			}

			// pid
			PacketId = (PacketId)headerBytes [_indexOfPid];

			// objectId set by consumer
			Array.Copy (headerBytes, _indexOfOid, headerOid, 0, headerOid.Length);
			ObjectId = new Guid(headerOid);

			DataLength = BitConverter.ToInt32 (headerBytes, _indexofDataLengthElement);

		}

		/// <summary>
		/// Gets the SOH Guid.
		/// </summary>
		/// <value>The SOH.</value>
		public static Guid SOH { 
			get { 
				return new Guid (_soh);
			}
		}

		/// <summary>
		/// Gets the size of the data.
		/// </summary>
		/// <value>The size of the data.</value>
		public int DataLength { get; private set; }

		/// <summary>
		/// Gets the size of the data.
		/// </summary>
		/// <value>The size of the data.</value>
		public PacketId PacketId { get; private set; }

		/// <summary>
		/// Gets or sets the object identifier associated with each packet. This is an optional
		/// value and not required by the TCP connection.
		/// </summary>
		/// <value>The object identifier.</value>
		public Guid ObjectId { private set; get; }

		/// <summary>
		/// Gets the size of the header.
		/// </summary>
		/// <value>The size of the header.</value>
		public static int HeaderLength { 
			get { 
				//this is a fixed constantso that a listener can determine
				//what size a header is before reading all of the data.
				return _headerSize;
			}
		}

		/// <summary>
		/// Returns the Packet as a byte array.
		/// </summary>
		/// <returns>A byte array.</returns>
		public byte[] ToByteArray()
		{

			var headerBytes = new byte[HeaderLength];
			var soh = SOH.ToByteArray();
			var pid = new byte[1] { (byte)PacketId };
			var headerObjectId = ObjectId.ToByteArray();
			var headerDataLength = BitConverter.GetBytes (DataLength);

			//copy the header elements to the header property
			Array.Copy (soh,0,headerBytes,_indexOfSoh,soh.Length); // SOH
			Array.Copy (headerDataLength, 0, headerBytes, _indexofDataLengthElement, headerDataLength.Length); //Data Length
			Array.Copy (pid,0,headerBytes, _indexOfPid, pid.Length);
			Array.Copy (headerObjectId, 0, headerBytes, _indexOfOid, headerObjectId.Length); // Object Id

			return headerBytes;
		}
	}
}

#endif