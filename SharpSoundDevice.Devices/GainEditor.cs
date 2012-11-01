using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpSoundDevice.Devices
{
	public partial class GainEditor : Panel
	{
		Gain G;
		public GainEditor(Gain g)
		{
			G = g;
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if(G.CurrentProgram > 0)
				G.CurrentProgram--;

			labelProgram.Text = G.CurrentProgram.ToString();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (G.CurrentProgram < 4)
				G.CurrentProgram++;

			labelProgram.Text = G.CurrentProgram.ToString();
		}

		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			var val = trackBar1.Value / 100.0;
			G.Gains[G.CurrentProgram] = val;

			labelValue.Text = G.Gains[G.CurrentProgram].ToString();
			G.HostInfo.SendEvent(G, new Event() { Data = G.Gains[G.CurrentProgram], EventIndex = 0, Type = EventType.Parameter });
		}

		public void UpdateParam(int param, double val)
		{
			trackBar1.Value = (int)(val * 100);
			labelValue.Text = val.ToString();
		}

		public void UpdateProgram(int program)
		{
			labelProgram.Text = G.CurrentProgram.ToString();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			this.Width = 600;
			this.Height = 600;

			G.HostInfo.SendEvent(G, new Event() { Data = G.Gains[G.CurrentProgram], EventIndex = 0, Type = EventType.WindowSize });
		}
	}
}
