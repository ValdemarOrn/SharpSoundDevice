using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	public class GuiEvent
	{
		public GuiEventType Type { get; set; }
		public int Key { get; set; }
		public int Virtual { get; set; }
		public int Modifier { get; set; }
		public float Scroll { get; set; }
	}
}
