using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	public interface IAudioDevice
	{
		void InitializeDevice();
		void DisposeDevice();

		void Start();
		void Stop();

		DeviceInfo DeviceInfo { get; }
		Parameter[] ParameterInfo { get; }
		Port[] PortInfo { get; }
		int CurrentProgram { get; }

		void SendEvent(Event ev);
		void ProcessSample(double[][] input, double[][] output, uint bufferSize);

		void OpenEditor(IntPtr parentWindow);
		void CloseEditor();

		void SetProgramData(Program program, int index);
		Program GetProgramData(int index);

		void HostChanged();
		HostInfo HostInfo { get; set; }
	}
}
