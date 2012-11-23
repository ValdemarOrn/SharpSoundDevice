using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	public struct Event
	{
		public EventType Type { get; set; }
		public int EventIndex { get; set; }
		public object Data { get; set; }
	}
}
