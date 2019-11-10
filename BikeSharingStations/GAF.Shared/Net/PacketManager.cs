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
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

#if !PCL

using System;

namespace GAF.Net
{
	/// <summary>
	/// Packet manager class to manage a cyclic byte buffer.
	/// </summary>
	[Obsolete ("This class is deprecated. The similarly named class within the GAF.Network package, should be used instead.")]
	public class PacketManager
	{
		private readonly IQueue _queue;
		private State _state = State.LookingForSOH;
		private Packet _currentPacket = null;
		private PacketHeader _currentHeader = null;

		private enum State
		{
			LookingForSOH,
			LookingForHeader,
			LookingForData
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Net.PacketManager"/> class
		/// with a default buffer size of 1Mb.
		/// </summary>
		public PacketManager (IQueue queue)
		{
			// initialise
			_queue = queue;
		}

		/// <summary>
		/// Add the specified data.
		/// </summary>
		/// <param name="data">Data.</param>
		public void Add (byte[] data)
		{
			_queue.Enqueue (data);
		}

		/// <summary>
		/// Add the specified data.
		/// </summary>
		/// <param name="data">Data.</param>
		public void Add (byte data)
		{
			_queue.Enqueue (data);
		}

		/// <summary>
		/// Gets a value indicating whether this instance is processing incomming data.
		/// </summary>
		/// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
		public bool IsRunning { get; private set; }

		/// <summary>
		/// Gets the last pid received.
		/// </summary>
		/// <value>The last pid received.</value>
		public PacketId LastPidReceived { get; private set; }


		/// <summary>
		/// Gets the next available packet.
		/// </summary>
		/// <returns>The packet.</returns>
		public Packet GetPacket ()
		{
			var timestamp = DateTime.Now.Ticks;

			var soh = PacketHeader.SOH.ToByteArray ();
			var exit = false;

			while (_queue.Count > 0 && !exit) {

				switch (_state) {

				case State.LookingForSOH:
					{
						var orphanedBytes = FindBytes (soh);

						if (orphanedBytes >= 0) {

							//SOH exists so move to that point in the queue
							//A value greater than zero probably indicates some sort of transmission error.

							_queue.Dequeue (orphanedBytes);
							_state = State.LookingForHeader;

						} else {
							//no soh so exit this loop to allow more data to be added to the buffer
							exit = true;
						}
						break;
					}

				case State.LookingForHeader:
					{
						
						//try and get the header
						if (_queue.Count >= PacketHeader.HeaderLength) {

							var headerBytes = _queue.Dequeue (PacketHeader.HeaderLength);
							_currentHeader = new PacketHeader (headerBytes);

							if (_currentHeader != null) {

								if (_currentHeader.DataLength > 0) {
									_state = State.LookingForData;
								} else {
								
									//no data for this packet so all done											
									_currentPacket = new Packet (new byte[0], _currentHeader.PacketId, _currentHeader.ObjectId);
									_state = State.LookingForSOH;

									LastPidReceived = _currentPacket.Header.PacketId;

									return _currentPacket;

//										if (OnPacketReceived != null) {
//
//											var currentPacket = new Packet (new byte[0], currentHeader.PacketId, currentHeader.ObjectId);
//											var args = new PacketReceivedEventArgs (currentPacket, packetCount, orphanedBytes);
//											this.OnPacketReceived (this, args);
//										}

//									
								}
							}
						} else {
							//no soh so exit this loop to allow more data to be added to the buffer
							exit = true;
						}

						break;
					}

				case State.LookingForData:
					{
						//try and get the data
						if (_queue.Count >= _currentHeader.DataLength) {

							var dataBytes = _queue.Dequeue (_currentHeader.DataLength);

							if (dataBytes != null) {

								_state = State.LookingForSOH;

								_currentPacket = new Packet (dataBytes, _currentHeader.PacketId, _currentHeader.ObjectId);

								LastPidReceived = _currentPacket.Header.PacketId;

								return _currentPacket;

							}
						} else {
							//no soh so exit this loop to allow more data to be added to the buffer
							exit = true;
						}

						break;
					}

				}
			}

			return null;
		}

		#region private methods

		/// <summary>
		/// Finds a byte pattern by peeking at the queue. The method 
		/// returns the index of the first byte of the pattern in the queue.
		/// A value of -1 indicates that no pattern was found.
		/// </summary>
		/// <returns>The index within the queue where the pattern is located.</returns>
		private int FindBytes (byte[] bytesToFind)
		{
			var index = 0;
			var length = bytesToFind.Length;

			//make sure we have enough for a header
			while (_queue.Count >= length + index) {


				var potentialSoh = _queue.Peek (index, length);
				if (ArraysEqual (potentialSoh, bytesToFind, length)) {

					//header found
					return index;

				} else {
					//move along one byte
					index++;
				}
			}

			return -1;

		}

		/// <summary>
		/// Checks two arrays for equality
		/// </summary>
		/// <returns><c>true</c>, if arrays are equal, <c>false</c> otherwise.</returns>
		/// <param name="array1">Array1.</param>
		/// <param name="array2">Array2.</param>
		/// <param name="bytesToCompare">Bytes to compare.</param>
		/// <remarks>Code supplied by Zar Shardan see..http://stackoverflow.com/questions/43289/comparing-two-byte-arrays-in-net</remarks>
		private static bool ArraysEqual (byte[] array1, byte[] array2, int bytesToCompare = 0)
		{
			if (array1.Length != array2.Length)
				return false;

			var length = (bytesToCompare == 0) ? array1.Length : bytesToCompare;
			var tailIdx = length - length % sizeof(Int64);

			// check in 8 byte chunks
			for (var i = 0; i < tailIdx; i += sizeof(Int64)) {
				if (BitConverter.ToInt64 (array1, i) != BitConverter.ToInt64 (array2, i))
					return false;
			}

			// check the remainder of the array, always shorter than 8 bytes
			for (var i = tailIdx; i < length; i++) {
				if (array1 [i] != array2 [i])
					return false;
			}

			return true;
		}


		#endregion
	}
}

#endif