#ifndef VST_HOST_EDITOR
#define VST_HOST_EDITOR

#ifndef __aeffeditor__
#include "aeffeditor.h"
#endif

#include "AudioDevice.h"
#include <windows.h>

class VstPluginBridgeEditor : public AEffEditor
{
public:
	AudioDevice* Device;

	VstPluginBridgeEditor (AudioEffect* effect);
	virtual ~VstPluginBridgeEditor ();

	virtual bool open (void* ptr);
	virtual void close ();
	virtual bool isOpen () { return Open; }

	virtual bool onKeyDown(VstKeyCode& keyCode);
	virtual bool onKeyUp(VstKeyCode& keyCode);
	virtual bool onWheel(float distance);

	bool Open;
	bool getRect (ERect** rect);

protected:
	ERect rect;
};

#endif
