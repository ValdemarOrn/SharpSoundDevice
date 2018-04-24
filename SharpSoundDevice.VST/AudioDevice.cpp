
#include "string.h"
#include <Windows.h>

#include "AudioDevice.h"
#include "VstPluginBridge.h"
#include "AssemblyInitializer.h"
#include "HostInfoClass.h"

#using <mscorlib.dll>
#using <System.dll>

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace SharpSoundDevice::VST;

// ---------------------- Create a managed AudioDevice object ----------------------

// The patcher is used to replace this value
// Note: The patcher literally does a binary replacement on this string, do not mess with it!
const char* assemblyName = "::PLACEHOLDER::                                                                                                                                                                                                  ";

HINSTANCE GetMyModuleHandle()
{
	MEMORY_BASIC_INFORMATION mbi;
	VirtualQuery(GetMyModuleHandle, &mbi, sizeof(mbi));
	return (HINSTANCE)(mbi.AllocationBase);
}

int CreateDevice()
{
	try
	{
		char dllName[256];
		HINSTANCE thismodule = GetMyModuleHandle();
		GetModuleFileName(thismodule, dllName, 256);

		SharpSoundDevice::Logging::Log("");
		SharpSoundDevice::Logging::Log("===============================================================================================");
		SharpSoundDevice::Logging::Log("");
		SharpSoundDevice::Logging::Log("Creating new device, SharpSoundDevice.VST (Bridge) Version: " + System::Reflection::Assembly::GetExecutingAssembly()->FullName);
		int deviceID = SharpSoundDevice::Interop::CreateDevice(gcnew String(dllName), gcnew String(assemblyName));
		return deviceID;
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::Log("Failed to call CreateDevice");
		SharpSoundDevice::Logging::LogDeviceException(-1, e);
	}
}

// ------------------------ Constructor / Destructor ------------------------

AudioDevice::AudioDevice()
{
	// Note: DO NOT ADD CALLS TO SharpSoundDevice.dll IN HERE!!! THIS IS THE RESOLVER!!!

#ifdef _DEBUG
	AllocConsole();
#endif
	System::String^ assemblyNameStr = (gcnew String(assemblyName))->Trim();
	AssemblyLoader::AddAssemblyResolver(assemblyNameStr);
	this->AudioDeviceID = CreateDevice();

	this->InputChannelCount = -1;
	this->OutputChannelCount = -1;
	PortInfo* portInfo = GetPortInfo(); // refresh input and output port count
	delete portInfo;
}

AudioDevice::~AudioDevice()
{
	try
	{
		SharpSoundDevice::Interop::UnloadAudioBuffers(this->AudioDeviceID);
		SharpSoundDevice::Interop::DeleteDevice(this->AudioDeviceID);
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
	}
}


// ---------------------- Plugin implementation ----------------------

void AudioDevice::InitializeDevice()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->InitializeDevice();
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::DisposeDevice()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->DisposeDevice();
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::Start()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->Start();
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::Stop()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->Stop();
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
	}
}

DeviceInfo* AudioDevice::GetDeviceInfo()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		SharpSoundDevice::DeviceInfo^ info = dev->DeviceInfo;

		DeviceInfo* devInfo = new DeviceInfo();

		SharpSoundDevice::DeviceUtilities::CopyStringToBuffer(info->Developer, (IntPtr)devInfo->Developer, 256);
		SharpSoundDevice::DeviceUtilities::CopyStringToBuffer(info->DeviceID, (IntPtr)devInfo->DeviceID, 256);
		SharpSoundDevice::DeviceUtilities::CopyStringToBuffer(info->Name, (IntPtr)devInfo->Name, 256);

		devInfo->HasEditor = info->HasEditor;
		devInfo->ProgramCount = info->ProgramCount;
		devInfo->Type = (DeviceType)(int)info->Type;
		devInfo->Version = info->Version;
		devInfo->VstId = info->VstId;
		devInfo->EditorWidth = info->EditorWidth;
		devInfo->EditorHeight = info->EditorHeight;
		devInfo->UnsafeProcessing = info->UnsafeProcessing;

		return devInfo;
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
		return 0;
	}
}

//Todo: Make sure the parameter array is deleted properly at runtime
ParameterInfo* AudioDevice::GetParameterInfo()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		array<SharpSoundDevice::Parameter^>^ params = dev->ParameterInfo;

		ParameterInfo* paramInfo = new ParameterInfo();
		paramInfo->ParameterCount = params->Length;
		paramInfo->Parameters = new Parameter[params->Length];

		int count = (int)paramInfo->ParameterCount;

		for (int i = 0; i < count; i++)
		{
			SharpSoundDevice::DeviceUtilities::CopyStringToBuffer(params[i]->Display, (IntPtr)paramInfo->Parameters[i].Display, 256);
			SharpSoundDevice::DeviceUtilities::CopyStringToBuffer(params[i]->Name, (IntPtr)paramInfo->Parameters[i].Name, 256);

			paramInfo->Parameters[i].Index = params[i]->Index;
			//paramInfo->Parameters[i].Max = params[i].Max;
			//paramInfo->Parameters[i].Min = params[i].Min;
			paramInfo->Parameters[i].Steps = params[i]->Steps;
			paramInfo->Parameters[i].Value = params[i]->Value;
		}

		return paramInfo;
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
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

		for (int i = 0; i < ports->Length; i++)
		{
			SharpSoundDevice::DeviceUtilities::CopyStringToBuffer(ports[i].Name, (IntPtr)portInfo->Ports[i].Name, 256);
			portInfo->Ports[i].Direction = (PortDirection)(int)ports[i].Direction;
			portInfo->Ports[i].NumberOfChannels = ports[i].NumberOfChannels;

			if (portInfo->Ports[i].Direction == _OUTPUT)
				OutputChannelCount += portInfo->Ports[i].NumberOfChannels;
			else if (portInfo->Ports[i].Direction == _INPUT)
				InputChannelCount += portInfo->Ports[i].NumberOfChannels;
		}

		return portInfo;
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
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
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
		return 0;
	}
}


bool AudioDevice::SendEvent(Event* event)
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		SharpSoundDevice::Event^ ev = gcnew SharpSoundDevice::Event();

		ev->Type = (SharpSoundDevice::EventType)(int)event->Type;
		ev->EventIndex = event->EventIndex;

		if (event->Type == _PARAMETER)
		{
			ev->Data = (System::Double)*((double*)event->Data);
		}
		else if (event->Type == _MIDI)
		{
			array<Byte>^ val = gcnew array<Byte>(event->DataLength);
			for (int i = 0; i < val->Length; i++)
				val[i] = (Byte)((unsigned char*)event->Data)[i];

			ev->Data = val;
		}
		else if (event->Type == _PROGRAM)
		{
			ev->Data = nullptr;
		}
		else if (event->Type == _GUIEVENT)
		{
			GuiEvent* evData = (GuiEvent*)event->Data;
			SharpSoundDevice::GuiEvent^ guiEv = gcnew SharpSoundDevice::GuiEvent();
			guiEv->Key = evData->Key;
			guiEv->Modifier = evData->Modifier;
			guiEv->Virtual = evData->Virtual;
			guiEv->Scroll = evData->Scroll;
			guiEv->Type = (SharpSoundDevice::GuiEventType)evData->Type;

			ev->Data = guiEv;
		}

		bool output = dev->SendEvent(*ev);
		return output;
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
		return false;
	}
}

void AudioDevice::ProcessSample(double** input, double** output, unsigned int bufferSize)
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);

		if (dev->DeviceInfo.UnsafeProcessing)
		{
			dev->ProcessSample((IntPtr)input, (IntPtr)output, this->InputChannelCount, this->OutputChannelCount, bufferSize);
		}
		else
		{
			array<array<double>^>^ inp = SharpSoundDevice::DeviceUtilities::GetManagedSamplesForDevice(this->AudioDeviceID, (IntPtr)input, InputChannelCount, bufferSize);
			array<array<double>^>^ outp = SharpSoundDevice::DeviceUtilities::GetEmptyArrays(OutputChannelCount, bufferSize);
			dev->ProcessSample(inp, outp, bufferSize);
			SharpSoundDevice::DeviceUtilities::CopyToUnmanaged(outp, (IntPtr)output, OutputChannelCount, bufferSize);
		}
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::OpenEditor(void* parentWindow)
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->OpenEditor((IntPtr)parentWindow);
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::CloseEditor()
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		dev->CloseEditor();
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
	}
}

void AudioDevice::SetProgramData(Program* program, int index)
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		SharpSoundDevice::Program newprog = *(gcnew SharpSoundDevice::Program());

		// only load data if not null
		if (program->Data != 0 && program->DataSize > 0)
		{
			// allocate byte array for data
			array<Byte>^ val = gcnew array<Byte>(program->DataSize);
			Marshal::Copy((IntPtr)program->Data, val, 0, program->DataSize);

			newprog.Data = val;
		}

		if (program->Name != 0)
			newprog.Name = gcnew String(program->Name);
		else
			newprog.Name = gcnew String("");

		dev->SetProgramData(newprog, index);
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
	}
}

Program* AudioDevice::GetProgramData(int index)
{
	try
	{
		SharpSoundDevice::IAudioDevice^ dev = SharpSoundDevice::Interop::GetDevice(AudioDeviceID);
		SharpSoundDevice::Program^ progManaged = dev->GetProgramData(index);

		Program* program = new Program();

		if (progManaged->Data == nullptr)
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

		if (progManaged->Name == nullptr)
			strncpy(program->Name, "", 256);
		else
			SharpSoundDevice::DeviceUtilities::CopyStringToBuffer(progManaged->Name, (IntPtr)program->Name, 256);

		return program;
	}
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
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
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
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
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(AudioDeviceID, e);
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
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(device->AudioDeviceID, e);
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
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(device->AudioDeviceID, e);
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
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(device->AudioDeviceID, e);
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
	catch (Exception^ e)
	{
		SharpSoundDevice::Logging::LogDeviceException(device->AudioDeviceID, e);
	}
}
