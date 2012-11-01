#ifndef VST_HOST
#define VST_HOST

class VstPluginBridge;

#include "public.sdk/source/vst2.x/audioeffectx.h"
#include "AudioDevice.h"

//------------------------------------------------------------------------------------------
// VstPluginBridge
//------------------------------------------------------------------------------------------
class VstPluginBridge : public AudioEffectX
{
private:
	// 24 channels of 32 sample buffers
	double* inputBuffers[24];
	double* outputBuffers[24];

public:
	VstPluginBridge (audioMasterCallback audioMaster);
	~VstPluginBridge ();

	AudioDevice* Device;

	int InputChannelCount;
	int OutputChannelCount;
	void calculatePortCount();

	void suspend ();
	void resume ();

	void setSampleRate (float sampleRate);
	void setBlockSize (VstInt32 blockSize);
	
	void processReplacing (float** inputs, float** outputs, VstInt32 sampleFrames);
	void processDoubleReplacing (double** inputs, double** outputs, VstInt32 sampleFrames);

	void setParameter (VstInt32 index, float value);
	float getParameter (VstInt32 index);
	bool getParameterProperties(VstInt32 index, VstParameterProperties* p);

	int getProgram ();
	void setProgram (VstInt32 program);
	void setProgramName (char* name);
	void getProgramName (char* name);
	
	void getParameterLabel (VstInt32 index, char* label);
	void getParameterDisplay (VstInt32 index, char* text);
	void getParameterName (VstInt32 index, char* text);

	int getChunk (void** data, bool isPreset);
	int setChunk (void* data, VstInt32 byteSize,	bool isPreset);


	// ------------------ AudioEffectX ----------------------


	bool getProgramNameIndexed (VstInt32 category, VstInt32 index, char* text);

	bool getInputProperties (VstInt32 index, VstPinProperties* properties);
	bool getOutputProperties (VstInt32 index, VstPinProperties* properties);

	VstInt32 getNumMidiInputChannels ();
	VstInt32 getNumMidiOutputChannels ();

	VstInt32 processEvents (VstEvents* events);

	bool getEffectName (char* name);
	bool getVendorString (char* text);
	bool getProductString (char* text);
	VstInt32 getVendorVersion ();
	VstInt32 canDo (char* text);

	VstPlugCategory getPlugCategory () { return kPlugCategUnknown; }
};

#endif
