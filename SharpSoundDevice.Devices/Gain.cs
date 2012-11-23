using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Interop;

namespace SharpSoundDevice.Devices
{
	public class Gain : IAudioDevice
	{
		int _currentProgram;

		public int CurrentProgram
		{
			get
			{
				return _currentProgram;
			}
			set
			{
				if (_currentProgram != value)
				{
					_currentProgram = value;
					HostInfo.SendEvent(this, new Event() { Data = null, EventIndex = CurrentProgram, Type = EventType.ProgramChange });
				}
			}
		}

		public string[] Names;
		public double[] Gains;
		GainEditor Editor;

		public Gain()
		{
			Names = new string[DeviceInfo.ProgramCount];
			Gains = new double[DeviceInfo.ProgramCount];

			for (int i = 0; i < Names.Length; i++)
				Names[i] = "Program " + i;
			
			CurrentProgram = 0;
			Editor = new GainEditor(this);
		}

		public void InitializeDevice() { }
		public void DisposeDevice() { }

		public void Start() { }
		public void Stop() { }

		public DeviceInfo DeviceInfo
		{
			get
			{
				var info = new DeviceInfo();
				info.Developer = "Valdemar";
				info.DeviceID = "Gain Example";
				info.EditorWidth = (Editor != null) ? Editor.Width : 0;
				info.EditorHeight = (Editor != null) ? Editor.Height : 0;
				info.Type = DeviceType.Effect;
				info.HasEditor = true;
				info.Name = "Gain Effect";
				info.Version = 100;
				info.ProgramCount = 5;

				return info;
			}
		}

		public Parameter[] ParameterInfo
		{
			get
			{
				var info = new Parameter();
				info.Name = "Gain";
				info.Index = 0;
				//info.Min = 0.0;
				//info.Max = 5.0;
				info.Steps = 5;
				info.Value = Gains[CurrentProgram];
				info.Display = String.Format("{0:0.000}", Gains[CurrentProgram]);

				return new Parameter[] { info };
			}
		}

		public Port[] PortInfo
		{
			get
			{
				var infoIn = new Port();
				infoIn.Direction = PortDirection.Input;
				infoIn.Name = "Mono Input";
				infoIn.NumberOfChannels = 1;

				var infoOut = new Port();
				infoOut.Direction = PortDirection.Output;
				infoOut.Name = "Mono Output";
				infoOut.NumberOfChannels = 1;

				return new Port[] { infoIn, infoOut };
			}
		}

		public void SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter && ev.EventIndex == 0)
			{
				Gains[CurrentProgram] = (double)ev.Data;
				Editor.UpdateParam((int)ev.EventIndex, (double)ev.Data);

				if(Gains[CurrentProgram] < 0.5)
					HostInfo.SendEvent(this, new Event() { Data = new byte[] { 0xB0, 5, 9 }, EventIndex = 5, Type = EventType.Midi });
				else
					HostInfo.SendEvent(this, new Event() { Data = new byte[] { 0xF0, 42, 0xF7 }, EventIndex = 9, Type = EventType.Midi });
			}

			if (ev.Type == EventType.ProgramChange)
			{
				CurrentProgram = (int)ev.EventIndex;
				Editor.UpdateProgram(CurrentProgram);
			}

			if (ev.Type == EventType.Midi)
			{
				byte[] data = (byte[])ev.Data;
				if (data[0] == 0xB0 && data[1] == 1)
				{
					Gains[CurrentProgram] = data[2] / 127.0;
					HostInfo.SendEvent(this, new Event() { Data = Gains[CurrentProgram], EventIndex = 0, Type = EventType.Parameter });
				}
			}
		}

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			double gain = Gains[CurrentProgram];

			for (int i = 0; i < input.Length; i++)
			{
				for (int j = 0; j < input[i].Length; j++)
					output[i][j] = input[i][j] * gain;
			}

		}

		//Panel ff;
		//WpfUI.MainWindow w;

		public void OpenEditor(IntPtr parentWindow)
		{
			Editor = new GainEditor(this);
			Interop.DockWinFormsPanel(Editor, parentWindow);

			/*
			w = new WpfUI.MainWindow();
			w.Width = 300;
			w.Height = 200;
			Interop.DockWpfWindow(w, parentWindow);
			*/

			/*
			ff = new Panel();
			ff.Width = 300;
			ff.Height = 200;
			ff.Controls.Add(new Button());
			Interop.DockWinFormsWindow(ff, parentWindow);
			*/
		}

		public void CloseEditor()
		{
			
		}

		public Program GetProgramData(int index)
		{
			var data = Encoding.UTF8.GetBytes(Gains[index].ToString());
			return new Program() { Name = Names[index], Data = data };
		}

		public void SetProgramData(Program program, int index)
		{
			Names[index] = program.Name;
			Gains[index] = Convert.ToDouble(Encoding.UTF8.GetString(program.Data));
		}

		public void HostChanged()
		{
			var blocksize = HostInfo.BlockSize;
			var bpm = HostInfo.BPM;
			var name = HostInfo.HostName;
			var vendor = HostInfo.HostVendor;
			var ver = HostInfo.HostVersion;
			var samplepos = HostInfo.SamplePosition;
			var samplerate = HostInfo.SampleRate;
			var den = HostInfo.TimeSignatureDen;
			var num = HostInfo.TimeSignatureNum;
		}

		public HostInfo HostInfo { get; set; }
	}
}
