using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	/// <summary>
	/// A structure that carrying event data from device-to-host, or host-to-device
	/// </summary>
	[Serializable]
	public struct Event
	{
		/// <summary>
		/// Specifies the event type. 
		/// See EventType enum for more info.
		/// </summary>
		public EventType Type { get; set; }

		/// <summary>
		/// Specifies the event index.
		/// EventType = Parameter: Specifies the parameter index that is being updated.
		/// EventType = Midi: Specifies the timing of the Midi event as number of samples
		///		offset from the start of the current sample buffer.
		/// EventType = ProgramChange: The index of the newly selected program.
		/// EventType = WindowSize: Unused.
		/// EventType = GuiEvent: Unused.
		/// </summary>
		public int EventIndex { get; set; }

		/// <summary>
		/// Contains the event data.
		/// EventType = Parameter: The new parameter value, type System.Double.
		/// EventType = Midi: byte array containing raw midi data.
		///		For regular Midi data, the length of the array is 3 bytes.
		///		For sysex data, the length corresponds to the number of sysex bytes sent.
		/// EventType = ProgramChange: Unused.
		/// EventType = WindowSize: Unused.
		/// EventType = GuiEvent: KeyEvent.
		/// </summary>
		public object Data { get; set; }
	}
}
