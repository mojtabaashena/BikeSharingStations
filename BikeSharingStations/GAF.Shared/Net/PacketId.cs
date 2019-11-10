using System;
namespace GAF.Net
{
    /// <summary>
    /// Packet identifier.
    /// </summary>
	[Obsolete ("This enumerator is deprecated. The similarly named enumerator within the GAF.Network package, should be used instead.")]
	public enum PacketId
	{
        /// <summary>
        /// Data PID.
        /// </summary>
		Data,
		/// <summary>
        /// Init PID.
        /// </summary>
        Init,
		/// <summary>
        /// Etx PID.
        /// </summary>
        Etx
	}
}

