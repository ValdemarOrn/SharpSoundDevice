using namespace VSTAudioDevice;

ref class HostInfoClass// : VSTAudioDevice::HostInfo
{
public:
	VstHost* vstHost;

public:

	void SendEvent(VSTAudioDevice::IAudioDevice^ sender, Event ev)
	{
	
	}

	property Double BPM
	{
		Double get()
		{
			return this->vstHost->getTimeInfo()->tempo;
		}
	}

	property Double SamplePosition
	{
		Double get()
		{
			return this->vstHost->getTimeInfo()->samplePos;
		}
	}

	property Double SampleRate
	{
		Double get()
		{
			return 56;
		}
	}
};
