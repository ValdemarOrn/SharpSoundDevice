#ifndef AUDIODEVICE
#define AUDIODEVICE


class VstPluginBridge;

/* Defines */

enum PortDirection
{
	_OUTPUT = 0,
	_INPUT
};

enum DeviceType
{
	_GENERATOR = 1,
	_EFFECT,
	_MIDIDEVICE
};

enum EventType
{
	_PARAMETER = 1,
	_MIDI,
	_PROGRAM,
	_WINDOWSIZE
};

/* Device Info */
#pragma pack(4)
typedef struct
{
	char DeviceID[256];
	int VstId;
	char Name[256];
	char Developer[256];
	unsigned int Version;
	DeviceType Type;
	unsigned int ProgramCount;
	bool HasEditor;
	int EditorWidth;
	int EditorHeight;

} DeviceInfo;

/* Parameter Info */

#pragma pack(4)
typedef struct 
{
	char Name[256];
	unsigned int Index;

	//double Min;
	//double Max;
	unsigned int Steps;

	double Value;
	char Display[256];

} Parameter;

#pragma pack(4)
typedef struct
{
	unsigned int ParameterCount;
	Parameter* Parameters;

} ParameterInfo;


/* Port Info */

#pragma pack(4)
typedef struct 
{
	char Name[256];
	PortDirection Direction;
	unsigned int NumberOfChannels;

} Port;

#pragma pack(4)
typedef struct 
{
	unsigned int PortCount;
	Port Ports[32];

} PortInfo;


/* Program */

#pragma pack(4)
typedef struct
{
	char Name[256];
	unsigned char* Data;
	unsigned int DataSize;

} Program;


/* Event */

#pragma pack(4)
typedef struct
{
	EventType Type;
	int EventIndex;
	void* Data;
	unsigned int DataLength;

} Event;


class AudioDevice
{
protected:

	// These values are aggregate values from PortInfo.
	// used in the ProcessSample method to marshal data
	int InputChannelCount;
	int OutputChannelCount;

public:

	int AudioDeviceID;

	AudioDevice();
	~AudioDevice();

	void InitializeDevice();
	void DisposeDevice();

	void Start();
	void Stop();

	DeviceInfo* GetDeviceInfo();
	ParameterInfo* GetParameterInfo();
	PortInfo* GetPortInfo();
	int GetCurrentProgram();

	void SendEvent(Event* ev);
	void ProcessSample(double** input, double** output, unsigned int bufferSize);

	void OpenEditor(void* parentWindow);
	void CloseEditor();

	void SetProgramData(Program* program, int index);
	Program* GetProgramData(int index);

	void HostChanged();

	// Creates a new ref class of type HostInfoClass and assigns it as the HostInfo for the IAudioDevice
	void SetVstHost(VstPluginBridge* host);

};

// -------------------- Serialize Helpers --------------------

// Invokes the serialize operations. After these methods have been invoked the serialized
// data is accessible 
int SerializeProgram(AudioDevice* device, int programIndex, unsigned char** dataPointer);
int SerializeBank(AudioDevice* device, unsigned char** dataPointer);
void DeserializeProgram(AudioDevice* device, unsigned char* data, int dataLength, int programIndex);
void DeserializeBank(AudioDevice* device, unsigned char* data, int dataLength);

#endif
