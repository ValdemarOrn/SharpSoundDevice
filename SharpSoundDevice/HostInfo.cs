using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	public interface HostInfo
	{
		void SendEvent(IAudioDevice sender, Event ev);

		double BPM { get; }
		double SamplePosition { get; }
		double SampleRate { get; }
		int BlockSize { get; }
		int TimeSignatureNum { get; }
		int TimeSignatureDen { get; }

		string HostVendor { get; }
		string HostName { get; }
		uint HostVersion { get; }
	}
}
