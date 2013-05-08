using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpSoundDevice;

namespace SimplePlugin
{
	public partial class Editor : UserControl
	{
		public Plugin Plugin;

		public Editor(Plugin plugin)
		{
			Plugin = plugin;
			InitializeComponent();
			Reload();
		}

		public void Reload()
		{
			var gainL = (int)(Plugin.ParameterInfo[0].Value * 100);
			var gainR = (int)(Plugin.ParameterInfo[1].Value * 100);

			if(gainL != trackBar1.Value)
				trackBar1.Value = gainL;

			if (gainR != trackBar2.Value)
				trackBar2.Value = gainR;
		}

		private void Update(object sender, EventArgs e)
		{
			var gainL = trackBar1.Value / 100.0;
			var gainR = trackBar2.Value / 100.0;
			var evL = new Event() { Data = gainL, EventIndex = 0, Type = EventType.Parameter };
			var evR = new Event() { Data = gainR, EventIndex = 1, Type = EventType.Parameter };

			Plugin.SetParam(0, gainL);
			Plugin.SetParam(1, gainR);
			Plugin.HostInfo.SendEvent(Plugin, evL);
			Plugin.HostInfo.SendEvent(Plugin, evR);
		}
	}
}
