
#include "AudioDevice.h"
#include "string.h"
#include "VstPluginBridge.h"
#include <Windows.h>

#using <mscorlib.dll>
using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

// ---------------------- Callback Class for HostInfo ------------------------

public ref class HostInfoClass : SharpSoundDevice::HostInfo
{
private:
	char* HostVendorPtr;
	char* HostNamePtr;

public:
	VstPluginBridge* vstHost;
	AudioDevice* Device;
	
	HostInfoClass()
	{
		HostVendorPtr = new char[128];
		HostNamePtr = new char[128];
	}

	~HostInfoClass()
	{
		delete HostVendorPtr;
		delete HostNamePtr;
	}

	virtual void SendEvent(SharpSoundDevice::IAudioDevice^ sender, SharpSoundDevice::Event ev)
	{
		try
		{
			if(ev.Type == SharpSoundDevice::EventType::Parameter)
			{
				double val = ((System::Double)ev.Data);
				int index = ev.EventIndex;
				vstHost->setParameterAutomated(index, (float)val);
				vstHost->updateDisplay();
			}
			else if(ev.Type == SharpSoundDevice::EventType::Midi)
			{
				array<byte>^ evData = (array<byte>^)ev.Data;
				VstEvents events;
				events.numEvents = 1;

				//sysex
				if(evData[0] == 0xF0)
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
					if(evData->Length != 3)
						return;

					VstMidiEvent  midiEv;
					midiEv.byteSize = sizeof(VstMidiEvent);
					midiEv.deltaFrames = ev.EventIndex;
					midiEv.detune = 0;
					midiEv.flags = kVstMidiEventIsRealtime;
					midiEv.midiData[0] = evData[0];
					midiEv.midiData[1] = evData[1];
					midiEv.midiData[2] = evData[2];
					midiEv.midiData[3] = 0;
					midiEv.noteLength = 0;
					midiEv.noteOffset = 0;
					midiEv.noteOffVelocity = 0;
					midiEv.type = kVstMidiType;

					events.events[0] = (VstEvent*)&midiEv;
					vstHost->sendVstEventsToHost(&events);
				}
			}
			else if(ev.Type == SharpSoundDevice::EventType::ProgramChange)
			{
				vstHost->updateDisplay();
			}
			else if(ev.Type == SharpSoundDevice::EventType::WindowSize)
			{
				vstHost->sizeWindow(sender->DeviceInfo.EditorWidth, sender->DeviceInfo.EditorHeight);
			}
		}
		catch(Exception^ e)
		{
			SharpSoundDevice::Interop::LogDeviceException(sender, e);
		}
	}

	virtual property double BPM
	{
		double get()
		{
			return vstHost->getTimeInfo(kVstTempoValid)->tempo;
		}
	}

	virtual property double SamplePosition
	{
		double get()
		{
			return vstHost->getTimeInfo(0)->samplePos;
		}
	}

	virtual property double SampleRate
	{
		double get()
		{
			return vstHost->getTimeInfo(0)->sampleRate;
		}
	}

	virtual property int BlockSize
	{
		int get()
		{
			return vstHost->getBlockSize();
		}
	}

	virtual property int TimeSignatureNum
	{
		int get()
		{
			return vstHost->getTimeInfo(kVstTempoValid)->timeSigNumerator;
		}
	}

	virtual property int TimeSignatureDen
	{
		int get()
		{
			return vstHost->getTimeInfo(kVstTempoValid)->timeSigDenominator;
		}
	}

	virtual property String^ HostVendor
	{
		String^ get()
		{
			vstHost->getHostVendorString(HostVendorPtr);
			return gcnew String(HostVendorPtr);  
		}
	}

	virtual property String^ HostName
	{
		String^ get()
		{
			vstHost->getHostProductString(HostNamePtr);
			return gcnew String(HostNamePtr);  
		}
	}

	virtual property unsigned int HostVersion
	{
		unsigned int get()
		{
			unsigned int version = vstHost->getHostVendorVersion();
			return version; 
		}
	}
};

// ---------------------- Assembly Initializer ------------------------------------

public ref class AssemblyLoader
{
private:
	static Boolean Loaded;

public:
	
	static void AddAssemblyResolver()
	{
		if(!Loaded)
		{
			AppDomain^ domain = AppDomain::CurrentDomain;
			domain->AssemblyResolve += gcnew ResolveEventHandler(loadAsm);
			Loaded = true;
		}
	}

	// This is used to look for a missing assembly. If the assembly is not located in the same directory as
	// the host process (very unlikely) we instruct the .NET runtime to look in the same directory as this dll
	static Reflection::Assembly^ loadAsm(Object^ sender, ResolveEventArgs^ args)
	{
		String^ folderPath = System::IO::Path::GetDirectoryName(Reflection::Assembly::GetExecutingAssembly()->Location);
		String^ assemblyPath = System::IO::Path::Combine(folderPath, (gcnew Reflection::AssemblyName(args->Name))->Name + ".dll");
		if (System::IO::File::Exists(assemblyPath) == false)
			return nullptr;

		Reflection::Assembly^ assembly = Reflection::Assembly::LoadFrom(assemblyPath);
		return assembly;
	}
};

// ---------------------- Create a managed AudioDevice object ----------------------

HINSTANCE GetMyModuleHandle()
{
	MEMORY_BASIC_INFORMATION mbi;
	VirtualQuery(GetMyModuleHandle, &mbi, sizeof(mbi));
	return (HINSTANCE) (mbi.AllocationBase); 
}

int CreateDevice()
{
	char dllName[256];
	HINSTANCE thismodule = GetMyModuleHandle();
	GetModuleFileName(thismodule,dllName,256);

	int deviceID = SharpSoundDevice::Interop::CreateDevice(gcnew String(dllName));

	SharpSoundDevice::Interop::LogDeviceMessage(deviceID, "SharpSoundDevice.VST Version: " + System::Reflection::Assembly::GetExecutingAssembly()->ImageRuntimeVersion);
#ifdef _DEBUG
	SharpSoundDevice::Interop::LogDeviceMessage(deviceID, "SharpSoundDevice.VST Configuration: Debug");
#else
	SharpSoundDevice::Interop::LogDeviceMessage(deviceID, "SharpSoundDevice.VST Configuration: Release");
#endif

	return deviceID;
}

// ------------------------ Constructor / Destructor ------------------------

AudioDevice::AudioDevice()
{
	AssemblyLoader::AddAssemblyResolver();
	this->AudioDeviceID = CreateDevice();

	this->InputChannelCount = -1;
	this->OutputChannelCount = -1;	

	PortInfo* portInfo = GetPortInfo(); // refresh input and output port count
	delete portInfo;
}

AudioDevice::~AudioDevice()
{
	SharpSoundDevice::Interop::DeleteDevice(this->AudioDeviceID);
}


// ---------------------- Plugin implementation ----------------------

void AudioDevice::InitializeDevice()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->InitializeDevice();
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::DisposeDevice()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->DisposeDevice();
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::Start()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->Start();
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::Stop()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->Stop();
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
	}
}

DeviceInfo* AudioDevice::GetDeviceInfo()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		SharpSoundDevice::DeviceInfo^ info = dev->DeviceInfo;

		DeviceInfo* devInfo = new DeviceInfo();

		SharpSoundDevice::Interop::CopyStringToBuffer(info->Developer, (IntPtr)devInfo->Developer, 256);
		SharpSoundDevice::Interop::CopyStringToBuffer(info->DeviceID, (IntPtr)devInfo->DeviceID, 256);
		SharpSoundDevice::Interop::CopyStringToBuffer(info->Name, (IntPtr)devInfo->Name, 256);

		devInfo->HasEditor = info->HasEditor;
		devInfo->ProgramCount = info->ProgramCount;
		devInfo->Type = (DeviceType)(int)info->Type;
		devInfo->Version = info->Version;
		devInfo->VstId = info->VstId;
		devInfo->EditorWidth = info->EditorWidth;
		devInfo->EditorHeight = info->EditorHeight;

		return devInfo;
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
		return 0;
	}
}

//Todo: Make sure the parameter array is deleted properly at runtime
ParameterInfo* AudioDevice::GetParameterInfo()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		array<SharpSoundDevice::Parameter>^ params = dev->ParameterInfo;

		ParameterInfo* paramInfo = new ParameterInfo();
		paramInfo->ParameterCount = params->Length;
		paramInfo->Parameters = new Parameter[params->Length];

		int count = (int)paramInfo->ParameterCount;

		for(int i = 0; i < count; i++)
		{
			SharpSoundDevice::Interop::CopyStringToBuffer(params[i].Display, (IntPtr)paramInfo->Parameters[i].Display, 256);
			SharpSoundDevice::Interop::CopyStringToBuffer(params[i].Name, (IntPtr)paramInfo->Parameters[i].Name, 256);

			paramInfo->Parameters[i].Index = params[i].Index;
			//paramInfo->Parameters[i].Max = params[i].Max;
			//paramInfo->Parameters[i].Min = params[i].Min;
			paramInfo->Parameters[i].Steps = params[i].Steps;
			paramInfo->Parameters[i].Value = params[i].Value;
		}

		return paramInfo;
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
		return 0;
	}
}

PortInfo* AudioDevice::GetPortInfo()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		array<SharpSoundDevice::Port>^ ports = dev->PortInfo;

		PortInfo* portInfo = new PortInfo;
		portInfo->PortCount = ports->Length;

		this->InputChannelCount = 0;
		this->OutputChannelCount = 0;

		for(int i = 0; i < ports->Length; i++)
		{
			SharpSoundDevice::Interop::CopyStringToBuffer(ports[i].Name, (IntPtr)portInfo->Ports[i].Name, 256);
			portInfo->Ports[i].Direction = (PortDirection)(int)ports[i].Direction;
			portInfo->Ports[i].NumberOfChannels = ports[i].NumberOfChannels;

			if(portInfo->Ports[i].Direction == _OUTPUT)
				OutputChannelCount += portInfo->Ports[i].NumberOfChannels;
			else if(portInfo->Ports[i].Direction == _INPUT)
				InputChannelCount += portInfo->Ports[i].NumberOfChannels;
		}

		return portInfo;
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
		return 0;
	}
}

int AudioDevice::GetCurrentProgram()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		return dev->CurrentProgram;
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
		return 0;
	}
}


void AudioDevice::SendEvent(Event* event)
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		SharpSoundDevice::Event^ ev = gcnew SharpSoundDevice::Event();

		ev->Type = (SharpSoundDevice::EventType)(int)event->Type;
		ev->EventIndex = event->EventIndex;

		if(event->Type == _PARAMETER)
		{
			ev->Data = (System::Double)*((double*)event->Data);
		}
		else if(event->Type == _MIDI)
		{
			array<Byte>^ val = gcnew array<Byte>(event->DataLength);
			for(int i=0; i<val->Length; i++)
				val[i] = (Byte)((unsigned char*)event->Data)[i];

			ev->Data = val;
		}
		else if(event->Type == _PROGRAM)
		{
			ev->Data = nullptr;
		}

		dev->SendEvent(*ev);
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::ProcessSample(double** input, double** output, unsigned int bufferSize)
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		array<array<double>^>^ inp = SharpSoundDevice::Interop::GetManagedSamples((IntPtr)input, InputChannelCount, bufferSize);
		array<array<double>^>^ outp = SharpSoundDevice::Interop::GetEmptyArrays(OutputChannelCount, bufferSize);

		dev->ProcessSample(inp, outp, bufferSize);

		SharpSoundDevice::Interop::CopyToUnmanaged(outp, (IntPtr)output, OutputChannelCount, bufferSize);
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::OpenEditor(void* parentWindow)
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->OpenEditor((IntPtr)parentWindow);
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::CloseEditor()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->CloseEditor();
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::SetProgramData(Program* program, int index)
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		SharpSoundDevice::Program newprog = *(gcnew SharpSoundDevice::Program());

		// only load data if not null
		if(program->Data != 0 && program->DataSize > 0)
		{
			// allocate byte array for data
			array<Byte>^ val = gcnew array<Byte>(program->DataSize);
			Marshal::Copy((IntPtr)program->Data, val, 0, program->DataSize);
		
			newprog.Data = val;
		}

		if(program->Name != 0)
			newprog.Name = gcnew String(program->Name);
		else
			newprog.Name = gcnew String("");

		dev->SetProgramData(newprog, index);
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
	}
}

Program* AudioDevice::GetProgramData(int index)
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		SharpSoundDevice::Program^ progManaged = dev->GetProgramData(index);

		Program* program = new Program();

		if(progManaged->Data == nullptr)
		{
			program->DataSize = 0;
			program->Data = 0;
		}
		else
		{
			// set data size
			program->DataSize = progManaged->Data->Length;

			// set data
			program->Data = new unsigned char[progManaged->Data->Length];
			Marshal::Copy(progManaged->Data, 0, (IntPtr)program->Data, progManaged->Data->Length);
		}

		if(progManaged->Name == nullptr)
			strncpy(program->Name, "", 256);
		else
			SharpSoundDevice::Interop::CopyStringToBuffer(progManaged->Name, (IntPtr)program->Name, 256);

		return program;
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
		return 0;
	}
}

void AudioDevice::HostChanged()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->HostChanged();
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::SetVstHost(VstPluginBridge* host)
{
	try
	{
		HostInfoClass^ hostinfo = gcnew HostInfoClass();
		hostinfo->vstHost = host;
		hostinfo->Device = this;

		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->HostInfo = hostinfo;
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(AudioDeviceID, e);
	}
}

// -------------------- Serialize Helpers --------------------

unsigned char* SerializedData = 0;

int SerializeProgram(AudioDevice* device, int programIndex, unsigned char** dataPointer)
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(device->AudioDeviceID);
		array<byte>^ data = SharpSoundDevice::ProgramData::SerializeSingleProgram(dev->GetProgramData(programIndex));

		delete SerializedData;
		SerializedData = new unsigned char[data->Length];
		Marshal::Copy(data, 0, (IntPtr)SerializedData, data->Length);

		*dataPointer = SerializedData;
		return data->Length;
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(device->AudioDeviceID, e);
		return 0;
	}
}

int SerializeBank(AudioDevice* device, unsigned char** dataPointer)
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(device->AudioDeviceID);
		array<byte>^ data = SharpSoundDevice::ProgramData::SerializeBank(dev);

		delete SerializedData;
		SerializedData = new unsigned char[data->Length];
		Marshal::Copy(data, 0, (IntPtr)SerializedData, data->Length);

		*dataPointer = SerializedData;
		return data->Length;
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(device->AudioDeviceID, e);
		return 0;
	}
}

void DeserializeProgram(AudioDevice* device, unsigned char* data, int dataLength, int programIndex)
{
	try
	{
		array<byte>^ d = gcnew array<byte>(dataLength);
		Marshal::Copy((IntPtr)data, d, 0, dataLength);

		SharpSoundDevice::Program prog = SharpSoundDevice::ProgramData::DeserializeSingleProgram(d);

		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(device->AudioDeviceID);
		dev->SetProgramData(prog, programIndex);
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(device->AudioDeviceID, e);
	}
}

void DeserializeBank(AudioDevice* device, unsigned char* data, int dataLength)
{
	try
	{
		array<byte>^ d = gcnew array<byte>(dataLength);
		Marshal::Copy((IntPtr)data, d, 0, dataLength);

		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(device->AudioDeviceID);
		SharpSoundDevice::ProgramData::DeserializeBank(d, dev);
	}
	catch(Exception^ e)
	{
		SharpSoundDevice::Interop::LogDeviceException(device->AudioDeviceID, e);
	}
}

