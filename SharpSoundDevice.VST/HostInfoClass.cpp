#include "HostInfoClass.h"

#using <mscorlib.dll>
using namespace System;
using namespace System::Runtime::InteropServices;

// ---------------------- Callback Class for HostInfo ------------------------

namespace SharpSoundDevice
{
	namespace VST
	{
		HostInfoClass::HostInfoClass()
		{
			HostVendorPtr = new char[128];
			HostNamePtr = new char[128];
		}

		HostInfoClass::~HostInfoClass()
		{
			delete HostVendorPtr;
			delete HostNamePtr;
		}

		void HostInfoClass::SendEvent(int pluginSenderId, SharpSoundDevice::Event ev)
		{
			try
			{
				if (ev.Type == SharpSoundDevice::EventType::Parameter)
				{
					double val = ((System::Double)ev.Data);
					int index = ev.EventIndex;
					vstHost->setParameterAutomated(index, (float)val);
					vstHost->updateDisplay();
				}
				else if (ev.Type == SharpSoundDevice::EventType::Midi)
				{
					array<unsigned char>^ evData = (array<unsigned char>^)ev.Data;
					VstEvents events;
					events.numEvents = 1;

					//sysex
					if (evData[0] == 0xF0)
					{
						unsigned char* data = new unsigned char[evData->Length];
						Marshal::Copy(evData, 0, (IntPtr)data, evData->Length);

						VstMidiSysexEvent sysexEv;
						sysexEv.byteSize = sizeof(VstMidiSysexEvent);
						sysexEv.deltaFrames = ev.EventIndex;
						sysexEv.dumpBytes = evData->Length;
						sysexEv.flags = 0;
						sysexEv.sysexDump = (char*)data;
						sysexEv.type = kVstSysExType;

						events.events[0] = (VstEvent*)&sysexEv;
						vstHost->sendVstEventsToHost(&events);
						delete data;
					}
					else
					{
						if (evData->Length < 1 || evData->Length > 4)
							return;

						VstMidiEvent  midiEv;
						midiEv.byteSize = sizeof(VstMidiEvent);
						midiEv.deltaFrames = ev.EventIndex;
						midiEv.detune = 0;
						midiEv.flags = kVstMidiEventIsRealtime;
						midiEv.midiData[0] = evData[0];
						
						if (evData->Length >= 2)
							midiEv.midiData[1] = evData[1];
						else
							midiEv.midiData[1] = 0;

						if (evData->Length >= 3)
							midiEv.midiData[2] = evData[2];
						else
							midiEv.midiData[2] = 0;

						midiEv.midiData[3] = 0;
						midiEv.noteLength = 0;
						midiEv.noteOffset = 0;
						midiEv.noteOffVelocity = 0;
						midiEv.type = kVstMidiType;

						events.events[0] = (VstEvent*)&midiEv;
						vstHost->sendVstEventsToHost(&events);
					}
				}
				else if (ev.Type == SharpSoundDevice::EventType::ProgramChange)
				{
					vstHost->updateDisplay();
				}
				else if (ev.Type == SharpSoundDevice::EventType::WindowSize)
				{
					SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(pluginSenderId);
					vstHost->sizeWindow(dev->DeviceInfo.EditorWidth, dev->DeviceInfo.EditorHeight);
				}
			}
			catch (Exception^ e)
			{
				SharpSoundDevice::Logging::LogDeviceException(pluginSenderId, e);
			}
		}

		double HostInfoClass::BPM::get()
		{
			VstTimeInfo* info = vstHost->getTimeInfo(kVstTempoValid);
			return info->tempo;
		}

		double HostInfoClass::SamplePosition::get()
		{
			VstTimeInfo* info = vstHost->getTimeInfo(0);
			return info->samplePos;
		}

		double HostInfoClass::SampleRate::get()
		{
			double fs = vstHost->Samplerate;
			if (fs > 0)
			{
				return fs;
			}
			else
			{
				VstTimeInfo* info = vstHost->getTimeInfo(0);
				return info->sampleRate;
			}
		}

		int HostInfoClass::BlockSize::get()
		{
			int blockSize = vstHost->getBlockSize();
			return blockSize;
		}

		int HostInfoClass::TimeSignatureNum::get()
		{
			VstTimeInfo* info = vstHost->getTimeInfo(kVstTempoValid);
			return info->timeSigNumerator;
		}

		int HostInfoClass::TimeSignatureDen::get()
		{
			VstTimeInfo* info = vstHost->getTimeInfo(kVstTempoValid);
			return info->timeSigDenominator;
		}

		System::String^ HostInfoClass::HostVendor::get()
		{
			vstHost->getHostVendorString(HostVendorPtr);
			return gcnew System::String(HostVendorPtr);
		}

		System::String^ HostInfoClass::HostName::get()
		{
				vstHost->getHostProductString(HostNamePtr);
				return gcnew String(HostNamePtr);
		}

		unsigned int HostInfoClass::HostVersion::get()
		{
			unsigned int version = vstHost->getHostVendorVersion();
			return version;
		}
	}
}
