#include "VstPluginBridge.h"
#include "AudioDevice.h"
#include "VstPluginBridgeEditor.h"

char* strncpySafe (char* dest, const char* src, int len)
{
	char* result = strncpy (dest, src, len);
	dest[len-1] = 0;
	return result;
}

//-------------------------------------------------------------------------------------------------------

AudioEffect* createEffectInstance (audioMasterCallback audioMaster)
{
	return new VstPluginBridge (audioMaster);
}

//-----------------------------------------------------------------------------------------
// VstPluginBridge
//-----------------------------------------------------------------------------------------
VstPluginBridge::VstPluginBridge (audioMasterCallback audioMaster) : AudioEffectX (audioMaster, 1, 1)
{
	this->Samplerate = 0;

	for(int i=0; i<24; i++)
	{
		inputBuffers[i] = new double[32768];
		outputBuffers[i] = new double[32768];
	}

	Device = new AudioDevice();
	Device->InitializeDevice();

	Device->SetVstHost(this);

	ParameterInfo* paramInfo = Device->GetParameterInfo();

	// set number of program, params, inputs and outputs
	this->numParams = paramInfo->ParameterCount;
	this->cEffect.numParams = paramInfo->ParameterCount;

	delete paramInfo->Parameters;
	delete paramInfo;

	DeviceInfo* devInfo = Device->GetDeviceInfo();

	this->numPrograms = devInfo->ProgramCount;
	this->cEffect.numPrograms = devInfo->ProgramCount;

	calculatePortCount();
	setNumInputs(InputChannelCount);
	setNumOutputs(OutputChannelCount);

	// set unique vst ID
	int id = devInfo->VstId;
	setUniqueID(id);

	canProcessReplacing(true);
	canDoubleReplacing(true);

	setProcessPrecision(kVstProcessPrecision64);

	int type = devInfo->Type;

	if(type == _GENERATOR)
		isSynth(true);
	else
		isSynth(false);

	// Set editor
	if(devInfo->HasEditor)
	{
		VstPluginBridgeEditor* ed = new VstPluginBridgeEditor (this);
		ed->Device = Device;
		this->setEditor(ed);
	}

	programsAreChunks(true);
	setProgram (0);

	delete devInfo;

	
}

//-----------------------------------------------------------------------------------------
VstPluginBridge::~VstPluginBridge ()
{
	Device->DisposeDevice();
	delete Device;
}

void VstPluginBridge::calculatePortCount()
{
	PortInfo* portInfo = Device->GetPortInfo();
	this->InputChannelCount = 0;
	this->OutputChannelCount = 0;

	int count = ((int)portInfo->PortCount);

	for(int i = 0; i < count; i++)
	{
		if(portInfo->Ports[i].Direction == _OUTPUT)
			OutputChannelCount += portInfo->Ports[i].NumberOfChannels;
		else if(portInfo->Ports[i].Direction == _INPUT)
			InputChannelCount += portInfo->Ports[i].NumberOfChannels;
	}

	delete portInfo;
}

//-----------------------------------------------------------------------------------------
int VstPluginBridge::getProgram ()
{
	return Device->GetCurrentProgram();
}

//-----------------------------------------------------------------------------------------
void VstPluginBridge::setProgram (VstInt32 program)
{
	if(program < 0 || program >= numPrograms)
		return;

	Event ev;
	ev.Data = 0;
	ev.DataLength = 0;
	ev.EventIndex = program;
	ev.Type = _PROGRAM;
	Device->SendEvent(&ev);
}

//-----------------------------------------------------------------------------------------
void VstPluginBridge::setProgramName (char* name)
{
	int currentProgram = Device->GetCurrentProgram();

	Program* program = Device->GetProgramData(currentProgram);

	Program p;
	p.Data = program->Data;
	p.DataSize = program->DataSize;
	strncpy(p.Name, name, 256);
	Device->SetProgramData(&p, currentProgram);

	delete program->Data;
	delete program;
}

//-----------------------------------------------------------------------------------------
void VstPluginBridge::getProgramName (char* name)
{
	int currentProgram = Device->GetCurrentProgram();

	Program* program = Device->GetProgramData(currentProgram);
	strncpySafe(name, program->Name, kVstMaxProgNameLen + 1);

	delete program->Data;
	delete program;
}

//-----------------------------------------------------------------------------------------
bool VstPluginBridge::getProgramNameIndexed (VstInt32 category, VstInt32 index, char* text)
{
	if(index < 0 || index >= numPrograms)
		return false;

	Program* program = Device->GetProgramData(index);
	strncpySafe(text, program->Name, kVstMaxProgNameLen + 1);

	delete program->Data;
	delete program;

	return true;
}

//-----------------------------------------------------------------------------------------
void VstPluginBridge::setParameter (VstInt32 index, float value)
{
	double val = (double)value;

	Event ev;
	ev.Data = &val;
	ev.DataLength = 8;
	ev.EventIndex = index;
	ev.Type = _PARAMETER;

	Device->SendEvent(&ev);
}

//-----------------------------------------------------------------------------------------
float VstPluginBridge::getParameter (VstInt32 index)
{
	ParameterInfo* paramInfo = Device->GetParameterInfo();
	float val = 0.0f;

	if(index >= 0 && index < ((int)paramInfo->ParameterCount))
		val = (float)paramInfo->Parameters[index].Value;

	delete paramInfo->Parameters;
	delete paramInfo;

	return val;
}

//-----------------------------------------------------------------------------------------
bool VstPluginBridge::getParameterProperties(VstInt32 index, VstParameterProperties* p)
{
	// Note: This isn't working in any host I've tried...

	ParameterInfo* paramInfo = Device->GetParameterInfo();

	if(index >= 0 && index < ((int)paramInfo->ParameterCount))
	{
		int steps = paramInfo->Parameters[index].Steps;
		if(steps > 0)
		{
			float stepSize = (float)(1.0 / (steps - 1));
			p->largeStepFloat = stepSize;
			p->smallStepFloat = stepSize;
			p->stepFloat = stepSize;
			p->flags = kVstParameterUsesFloatStep;
		}
	}

	delete paramInfo->Parameters;
	delete paramInfo;

	
	return true;
}

//-----------------------------------------------------------------------------------------
void VstPluginBridge::getParameterLabel (VstInt32 index, char* label)
{
	strncpySafe(label, "", kVstMaxLabelLen + 1);
}

//-----------------------------------------------------------------------------------------
void VstPluginBridge::getParameterDisplay (VstInt32 index, char* text)
{
	ParameterInfo* paramInfo = Device->GetParameterInfo();
	
	if(index >= 0  && index < ((int)paramInfo->ParameterCount))
	{
		char* disp = paramInfo->Parameters[index].Display;
		strncpySafe(text, disp, kVstMaxParamStrLen + 1);
	}

	delete paramInfo->Parameters;
	delete paramInfo;
}

//-----------------------------------------------------------------------------------------
void VstPluginBridge::getParameterName (VstInt32 index, char* label)
{
	ParameterInfo* paramInfo = Device->GetParameterInfo();
		
	if(index < 0 ||index >= ((int)paramInfo->ParameterCount))
		return;

	char* name = paramInfo->Parameters[index].Name;

	strncpySafe(label, name, kVstMaxProgNameLen + 1);
	
	delete paramInfo->Parameters;
	delete paramInfo;
}

//-----------------------------------------------------------------------------------------
int VstPluginBridge::getChunk (void** data, bool isPreset) 
{
	if(isPreset)
	{
		int currentProgram = Device->GetCurrentProgram();
		int size = SerializeProgram(Device, currentProgram, (unsigned char**)data);
		return size;
	}
	else
	{
		int size = SerializeBank(Device, (unsigned char**)data);
		return size;
	}

	return 0;
}

//-----------------------------------------------------------------------------------------
int VstPluginBridge::setChunk (void* data, VstInt32 byteSize, bool isPreset)
{
	int currentProgram = Device->GetCurrentProgram();

	if(isPreset)
		DeserializeProgram(Device, (unsigned char*)data, byteSize, currentProgram);
	else
		DeserializeBank(Device, (unsigned char*)data, byteSize);

	return 0;
}

//------------------------------------------------------------------------
void VstPluginBridge::resume ()
{
	Device->Start();
}

void VstPluginBridge::suspend()
{
	Device->Stop();
}

//-----------------------------------------------------------------------------------------

bool VstPluginBridge::getInputProperties (VstInt32 index, VstPinProperties* properties)
{
	// find all input ports
	PortInfo* portInfo = Device->GetPortInfo();
	Port inputPorts[16];
	int inputPortCount = 0;

	int count = (int)portInfo->PortCount;

	for(int i = 0; i <= count; i++)
	{
		if(portInfo->Ports[i].Direction == _INPUT)
		{
			inputPorts[inputPortCount] = portInfo->Ports[i];
			inputPortCount++;
		}
	}

	delete portInfo;

	int port = 0;
	int channel = 0;

	for(int i = 0; i < index; i++)
	{
		if(port >= inputPortCount)
			return false;

		channel++;

		if(channel >= ((int)inputPorts[port].NumberOfChannels))
		{
			port++;
			channel = 0;
		}
	}

	strncpySafe(properties->label, inputPorts[port].Name, kVstMaxLabelLen+1);
		
	if(inputPorts[port].NumberOfChannels == 1)
		properties->arrangementType = kSpeakerArrMono; // Mono
	else if(inputPorts[port].NumberOfChannels == 2)
		properties->arrangementType = kSpeakerArrStereo; // L R
	else if(inputPorts[port].NumberOfChannels == 3)
		properties->arrangementType = kSpeakerArr30Cine; // L R C
	else if(inputPorts[port].NumberOfChannels == 4)
		properties->arrangementType = kSpeakerArr40Music; // L R Ls Rs
	else if(inputPorts[port].NumberOfChannels == 5)
		properties->arrangementType = kSpeakerArr41Music; // L R Low Ls Rs ... can also be L R C Ls Rs (5.0)
	else if(inputPorts[port].NumberOfChannels == 6)
		properties->arrangementType = kSpeakerArr51; // 5.1 L R C Low Ls Rs
	else if(inputPorts[port].NumberOfChannels == 7)
		properties->arrangementType = kSpeakerArr61Cine; // L R C Lfe Ls Rs Cs
	else if(inputPorts[port].NumberOfChannels == 8)
		properties->arrangementType = kSpeakerArr71Cine; // L R C Lfe Ls Rs Lc Rc
	else if(inputPorts[port].NumberOfChannels == 9)
		properties->arrangementType = kSpeakerArr81Cine; // L R C Lfe Ls Rs Lc Rc Cs

	properties->flags = kVstPinIsActive | kVstPinUseSpeaker;

	// If first channel in a stereo output
	if(inputPorts[port].NumberOfChannels == 2 && channel == 0)
		properties->flags |= kVstPinIsStereo;	// make channel 1+2 stereo

	return true;
	
}

bool VstPluginBridge::getOutputProperties (VstInt32 index, VstPinProperties* properties)
{
	// find all output ports
	PortInfo* portInfo = Device->GetPortInfo();
	Port outputPorts[16];
	int outputPortCount = 0;

	int count = (int)portInfo->PortCount;

	for(int i = 0; i <= count; i++)
	{
		if(portInfo->Ports[i].Direction == _OUTPUT)
		{
			outputPorts[outputPortCount] = portInfo->Ports[i];
			outputPortCount++;
		}
	}

	delete portInfo;

	int port = 0;
	int channel = 0;

	for(int i = 0; i < index; i++)
	{
		if(port >= outputPortCount)
			return false;

		channel++;

		if(channel >= (int)(outputPorts[port].NumberOfChannels))
		{
			port++;
			channel = 0;
		}
	}

	strncpySafe(properties->label, outputPorts[port].Name, kVstMaxLabelLen+1);
		
	if(outputPorts[port].NumberOfChannels == 1)
		properties->arrangementType = kSpeakerArrMono; // Mono
	else if(outputPorts[port].NumberOfChannels == 2)
		properties->arrangementType = kSpeakerArrStereo; // L R
	else if(outputPorts[port].NumberOfChannels == 3)
		properties->arrangementType = kSpeakerArr30Cine; // L R C
	else if(outputPorts[port].NumberOfChannels == 4)
		properties->arrangementType = kSpeakerArr40Music; // L R Ls Rs
	else if(outputPorts[port].NumberOfChannels == 5)
		properties->arrangementType = kSpeakerArr41Music; // L R Low Ls Rs ... can also be L R C Ls Rs (5.0)
	else if(outputPorts[port].NumberOfChannels == 6)
		properties->arrangementType = kSpeakerArr51; // 5.1 L R C Low Ls Rs
	else if(outputPorts[port].NumberOfChannels == 7)
		properties->arrangementType = kSpeakerArr61Cine; // L R C Lfe Ls Rs Cs
	else if(outputPorts[port].NumberOfChannels == 8)
		properties->arrangementType = kSpeakerArr71Cine; // L R C Lfe Ls Rs Lc Rc
	else if(outputPorts[port].NumberOfChannels == 9)
		properties->arrangementType = kSpeakerArr81Cine; // L R C Lfe Ls Rs Lc Rc Cs

	properties->flags = kVstPinIsActive | kVstPinUseSpeaker;

	// If first channel in a stereo output
	if(outputPorts[port].NumberOfChannels == 2 && channel == 0)
		properties->flags |= kVstPinIsStereo;	// make channel 1+2 stereo

	return true;
}

//-----------------------------------------------------------------------------------------
bool VstPluginBridge::getEffectName (char* name)
{
	DeviceInfo* devInfo = Device->GetDeviceInfo();
	strncpySafe(name, devInfo->Name, kVstMaxEffectNameLen+1);
	delete devInfo;
	return true;
}

//-----------------------------------------------------------------------------------------
bool VstPluginBridge::getVendorString (char* text)
{
	DeviceInfo* devInfo = Device->GetDeviceInfo();
	strncpySafe(text, devInfo->Developer, kVstMaxVendorStrLen+1);
	delete devInfo;
	return true;
}

//-----------------------------------------------------------------------------------------
bool VstPluginBridge::getProductString (char* text)
{
	DeviceInfo* devInfo = Device->GetDeviceInfo();
	strncpySafe(text, devInfo->Name, kVstMaxProductStrLen+1);
	delete devInfo;
	return true;
}

//-----------------------------------------------------------------------------------------
VstInt32 VstPluginBridge::getVendorVersion ()
{ 
	DeviceInfo* devInfo = Device->GetDeviceInfo();
	int version = devInfo->Version;
	delete devInfo;
	return version;
}

//-----------------------------------------------------------------------------------------
VstInt32 VstPluginBridge::canDo (char* text)
{
	if (!strcmp (text, "receiveVstEvents"))
		return 1;
	if (!strcmp (text, "receiveVstMidiEvent"))
		return 1;

	if (!strcmp (text, "sendVstEvents"))
		return 1;
	if (!strcmp (text, "sendVstMidiEvent"))
		return 1;
	
	if (!strcmp (text, "receiveVstTimeInfo"))
		return 1;
	
	return -1;	// explicitly can't do; 0 => don't know
	// Can't Do this:
	// canDoOffline = "offline";
	// canDoMidiProgramNames = "midiProgramNames";
	// canDoBypass = "bypass";

	return -1;
}

//-----------------------------------------------------------------------------------------
VstInt32 VstPluginBridge::getNumMidiInputChannels ()
{
	return 16; // support all channels
}

//-----------------------------------------------------------------------------------------
VstInt32 VstPluginBridge::getNumMidiOutputChannels ()
{
	return 16; // support all channels
}


//-----------------------------------------------------------------------------------------
void VstPluginBridge::setSampleRate (float sampleRate)
{
	this->Samplerate = sampleRate;
	Device->HostChanged();
}

//-----------------------------------------------------------------------------------------
void VstPluginBridge::setBlockSize (VstInt32 blockSize)
{
	Device->HostChanged();
}

//-----------------------------------------------------------------------------------------
void VstPluginBridge::processReplacing (float** inputs, float** outputs, VstInt32 sampleFrames)
{
	for(int i=0; i < InputChannelCount; i++)
		for(int j=0; j<sampleFrames; j++)
			inputBuffers[i][j] = (double)inputs[i][j];

	Device->ProcessSample(inputBuffers, outputBuffers, sampleFrames);

	for(int i=0; i < OutputChannelCount; i++)
		for(int j=0; j<sampleFrames; j++)
			outputs[i][j] = (float)outputBuffers[i][j];
}

//-----------------------------------------------------------------------------------------
void VstPluginBridge::processDoubleReplacing (double** inputs, double** outputs, VstInt32 sampleFrames)
{
	Device->ProcessSample(inputs, outputs, sampleFrames);
}

//-----------------------------------------------------------------------------------------
VstInt32 VstPluginBridge::processEvents (VstEvents* ev)
{
	for (VstInt32 i = 0; i < ev->numEvents; i++)
	{
		int type = -1;
		unsigned char* data = 0;
		int size = 0;
		int delta = (ev->events[i])->deltaFrames;

		if ((ev->events[i])->type == kVstMidiType)
		{
			type = 0;
			VstMidiEvent* evMidi = (VstMidiEvent*)ev->events[i];
			data = (unsigned char*)evMidi->midiData;
			size = 3;
		}
		else if ((ev->events[i])->type == kVstSysExType)
		{
			type = 0;
			VstMidiSysexEvent* evSysex = (VstMidiSysexEvent*)ev->events[i];
			size = evSysex->dumpBytes;
			data = (unsigned char*)evSysex->sysexDump;
		}
		
		Event ev;
		ev.Data = data;
		ev.DataLength = size;
		ev.Type = _MIDI;
		ev.EventIndex = delta;

		Device->SendEvent(&ev);
	}

	return 1;
}

