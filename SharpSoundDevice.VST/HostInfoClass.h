#ifndef HOSTINFOCLASSS
#define HOSTINFOCLASSS

#include "VstPluginBridge.h"
#using <mscorlib.dll>
using namespace System;

namespace SharpSoundDevice
{
	namespace VST
	{
		// ---------------------- Assembly Initializer ------------------------------------

		public ref class HostInfoClass : MarshalByRefObject, SharpSoundDevice::IHostInfo
		{
		private:
			char* HostVendorPtr;
			char* HostNamePtr;
		public:
			VstPluginBridge* vstHost;
			AudioDevice* Device;

			HostInfoClass();

			~HostInfoClass();

			virtual void SendEvent(int pluginSenderId, SharpSoundDevice::Event ev);

			virtual property double BPM { double get(); }
			virtual property double SamplePosition { double get(); }

			virtual property double SampleRate { double get(); }
			virtual property int BlockSize { int get(); }
			virtual property int TimeSignatureNum { int get(); }
			virtual property int TimeSignatureDen { int get(); }
			virtual property String^ HostVendor { String^ get(); }
			virtual property String^ HostName { String^ get(); }
		
			virtual property unsigned int HostVersion { unsigned int get(); }
		};
	}
}

#endif
