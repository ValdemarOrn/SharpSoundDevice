using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	/// <summary>
	/// A structure that carrying event data from device-to-host, or host-to-device
	/// </summary>
	public struct Event
	{
		/// <summary>
		/// Specifies the event type. See EventType enum.
		/// </summary>
		public EventType Type { get; set; }

		/// <summary>
		/// Specifies the event index. (Needs more documentation)
		/// </summary>
		public int EventIndex { get; set; }

		/// <summary>
		/// Contains the event data
		/// </summary>
		public object Data { get; set; }
	}
}
