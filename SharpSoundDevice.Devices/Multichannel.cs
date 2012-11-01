using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice.Devices
{

	public class Multichannel : IAudioDevice
	{
		public int CurrentProgram { get; set; }
		public string Name;
		public double[] Gain = new double[8];

		public Multichannel()
		{
			Name = "Program";
			CurrentProgram = 0;
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
				info.DeviceID = "Multichannel Example";
				info.Type = DeviceType.Effect;
				info.HasEditor = false;
				info.Name = "Multichannel Effect";
				info.Version = 100;
				info.ProgramCount = 1;

				return info;
			}
		}

		public Parameter[] ParameterInfo
		{
			get
			{
				var parameters = new List<Parameter>();

				var info = new Parameter();
				info.Name = "Gain 0->0";
				info.Index = 0;
				//info.Min = 0.0;
				//info.Max = 1.0;
				info.Value = Gain[0];
				info.Display = String.Format("{0:0.000}", Gain[0]);
				parameters.Add(info);

				info = new Parameter();
				info.Name = "Gain 0->1";
				info.Index = 0;
				//info.Min = 0.0;
				//info.Max = 1.0;
				info.Value = Gain[1];
				info.Display = String.Format("{0:0.000}", Gain[1]);
				parameters.Add(info);

				info = new Parameter();
				info.Name = "Gain 0->2";
				info.Index = 0;
				//info.Min = 0.0;
				//info.Max = 1.0;
				info.Value = Gain[2];
				info.Display = String.Format("{0:0.000}", Gain[2]);
				parameters.Add(info);

				info = new Parameter();
				info.Name = "Gain 0->3";
				info.Index = 0;
				//info.Min = 0.0;
				//info.Max = 1.0;
				info.Value = Gain[3];
				info.Display = String.Format("{0:0.000}", Gain[3]);
				parameters.Add(info);

				info = new Parameter();
				info.Name = "Gain 1->0";
				info.Index = 0;
				//info.Min = 0.0;
				//info.Max = 1.0;
				info.Value = Gain[4];
				info.Display = String.Format("{0:0.000}", Gain[4]);
				parameters.Add(info);

				info = new Parameter();
				info.Name = "Gain 1->1";
				info.Index = 0;
				//info.Min = 0.0;
				//info.Max = 1.0;
				info.Value = Gain[5];
				info.Display = String.Format("{0:0.000}", Gain[5]);
				parameters.Add(info);

				info = new Parameter();
				info.Name = "Gain 1->2";
				info.Index = 0;
				//info.Min = 0.0;
				//info.Max = 1.0;
				info.Value = Gain[6];
				info.Display = String.Format("{0:0.000}", Gain[6]);
				parameters.Add(info);

				info = new Parameter();
				info.Name = "Gain 1->3";
				info.Index = 0;
				//info.Min = 0.0;
				//info.Max = 1.0;
				info.Value = Gain[7];
				info.Display = String.Format("{0:0.000}", Gain[7]);
				parameters.Add(info);

				return parameters.ToArray();
			}
		}

		public Port[] PortInfo
		{
			get
			{
				var infoIn = new Port();
				infoIn.Direction = PortDirection.Input;
				infoIn.Name = "Stereo Input";
				infoIn.NumberOfChannels = 2;

				var infoOut = new Port();
				infoOut.Direction = PortDirection.Output;
				infoOut.Name = "Stereo Output 1";
				infoOut.NumberOfChannels = 2;

				var infoOut2 = new Port();
				infoOut2.Direction = PortDirection.Output;
				infoOut2.Name = "Stereo Output 2";
				infoOut2.NumberOfChannels = 2;

				return new Port[] { infoIn, infoOut, infoOut2 };
			}
		}

		public void SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter)
				Gain[ev.EventIndex] = (double)ev.Data;
				
		}

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			for (int i = 0; i < bufferSize; i++)
			{
				output[0][i] = input[0][i] * Gain[0];
				output[1][i] = input[0][i] * Gain[1];
				output[2][i] = input[0][i] * Gain[2];
				output[3][i] = input[0][i] * Gain[3];

				output[0][i] += input[1][i] * Gain[4];
				output[1][i] += input[1][i] * Gain[5];
				output[2][i] += input[1][i] * Gain[6];
				output[3][i] += input[1][i] * Gain[7];
			}
		}

		public void OpenEditor(IntPtr parentWindow) { }
		public void CloseEditor() { }

		public Program GetProgramData(int index)
		{
			string str = "";
			foreach (double g in Gain)
				str += g.ToString() + ",";

			var data = Encoding.UTF8.GetBytes(str);
			return new Program() { Name = Name, Data = data };
		}

		public void SetProgramData(Program program, int index)
		{
			Name = program.Name;

			string[] parts = Encoding.UTF8.GetString(program.Data).Split(',');

			for(int i=0; i<8; i++)
				Gain[i] = Convert.ToDouble(parts[i]);
		}

		public void HostChanged() { }
		public HostInfo HostInfo { get; set; }
	}
}
