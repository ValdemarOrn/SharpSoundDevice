#ifndef __sdeditor__
#include "VstPluginBridgeEditor.h"
#endif

VstPluginBridgeEditor::VstPluginBridgeEditor (AudioEffect *effect) : AEffEditor (effect)
{
	// init the size of the plugin
	rect.left   = 0;
	rect.top    = 0;
	rect.right  = 100; // overwritten when host calls Open
	rect.bottom = 100;
}

VstPluginBridgeEditor::~VstPluginBridgeEditor ()
{
}

bool VstPluginBridgeEditor::open (void *ptr)
{
	AEffEditor::open (ptr);

	DeviceInfo* devInfo = Device->GetDeviceInfo();

	rect.top = 0;
	rect.left = 0;
	rect.right  = devInfo->EditorWidth;
	rect.bottom = devInfo->EditorHeight;

	delete devInfo;

	Device->OpenEditor(ptr);
	Open = true;

	return false;
}

void VstPluginBridgeEditor::close ()
{
	Device->CloseEditor();
	Open = false;
}

bool VstPluginBridgeEditor::getRect (ERect **ppErect)
{
	*ppErect = &rect;
	return false;
	
	
}