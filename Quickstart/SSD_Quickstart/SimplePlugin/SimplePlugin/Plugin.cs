using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpSoundDevice;
using System.Globalization;

namespace SimplePlugin
{
	public class Plugin : IAudioDevice
	{
		private DeviceInfo DevInfo;

		public int CurrentProgram { get; private set; }
		public DeviceInfo DeviceInfo { get { return DevInfo; } }
		public IHostInfo HostInfo { get; set; }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }

		public Plugin()
		{
			DevInfo = new DeviceInfo();
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.DeviceID = "Valdemar Erlingsson - SimplePlugin";
            DevInfo.EditorHeight = 154; // Add this for editor
            DevInfo.EditorWidth = 247; // Add this for editor
            DevInfo.HasEditor = true;   // Add this for editor
			DevInfo.Name = "SimplePlugin";
			DevInfo.ProgramCount = 1;
			DevInfo.Type = DeviceType.Effect;
			DevInfo.Version = 1000;
			DevInfo.VstId = DeviceUtilities.GenerateIntegerId(DevInfo.DeviceID);

			ParameterInfo = new Parameter[2]
			{
				new Parameter() { Display = "1.0", Index = 0, Name = "Gain L", Steps = 0, Value = 1.0 },
				new Parameter() { Display = "1.0", Index = 1, Name = "Gain R", Steps = 0, Value = 1.0 }
			};

			PortInfo = new Port[2]
			{
				new Port() { Direction = PortDirection.Input, Name = "Stereo Input", NumberOfChannels = 2 }, 
				new Port() { Direction = PortDirection.Output, Name = "Stereo Output", NumberOfChannels = 2 }
			};
		}

		public void InitializeDevice() { }
		public void DisposeDevice() { }
		public void Start() { }
		public void Stop() { }

        Editor Editor; // Add this for editor

		public void OpenEditor(IntPtr parentWindow) 
		{
            // Add this for editor
			Editor = new Editor(this);
			DeviceUtilities.DockWinFormsPanel(Editor, parentWindow);
		}

		public void CloseEditor() { }
		public void HostChanged() { }

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			var gainL = ParameterInfo[0].Value;
			var gainR = ParameterInfo[1].Value;

			for (int i = 0; i < bufferSize; i++)
			{
				output[0][i] = input[0][i] * gainL;
				output[1][i] = input[1][i] * gainR;
			}
		}

		public void SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter && ev.EventIndex < ParameterInfo.Length)
			{
				SetParam(ev.EventIndex, (double)ev.Data);
				if (Editor != null) // Add this for editor
					Editor.Reload();
			}
		}

		public void SetParam(int index, double value)
		{
			ParameterInfo[index].Value = value;
			ParameterInfo[index].Display = String.Format("{0:0.00}", value);
		}

		public Program GetProgramData(int index)
		{
			string programText = String.Format(CultureInfo.InvariantCulture, 
				"{0:0.00}, {1:0.00}", ParameterInfo[0].Value, ParameterInfo[1].Value);
			byte[] programData = Encoding.ASCII.GetBytes(programText);

			var program = new Program();
			program.Name = "Program 1";
			program.Data = programData;
			return program;
		}

		public void SetProgramData(Program program, int index)
		{
			string programText = Encoding.ASCII.GetString(program.Data);
			double[] values = programText.Split(',').Select(x => 
				Convert.ToDouble(x, CultureInfo.InvariantCulture)).ToArray();
			
			ParameterInfo[0].Value = values[0];
			ParameterInfo[1].Value = values[1];
			ParameterInfo[0].Display = String.Format("{0:0.00}", values[0]);
			ParameterInfo[1].Display = String.Format("{0:0.00}", values[1]);
		}	
	}
}
