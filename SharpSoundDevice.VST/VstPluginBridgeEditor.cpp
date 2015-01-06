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

bool VstPluginBridgeEditor::onKeyDown(VstKeyCode& keyCode)
{
	GuiEvent data;
	data.Type = _KEYDOWN;
	data.Key = keyCode.character;
	data.Modifier = keyCode.modifier;
	data.Virtual = keyCode.virt;
	data.Scroll = 0.0;

	Event ev;
	ev.Data = &data;
	ev.DataLength = 0;
	ev.EventIndex = 0;
	ev.Type = _GUIEVENT;
	return Device->SendEvent(&ev);
}

bool VstPluginBridgeEditor::onKeyUp(VstKeyCode& keyCode)
{
	GuiEvent data;
	data.Type = _KEYUP;
	data.Key = keyCode.character;
	data.Modifier = keyCode.modifier;
	data.Virtual = keyCode.virt;
	data.Scroll = 0.0;

	Event ev;
	ev.Data = &data;
	ev.DataLength = 0;
	ev.EventIndex = 0;
	ev.Type = _GUIEVENT;
	return Device->SendEvent(&ev);
}

bool VstPluginBridgeEditor::onWheel(float distance)
{
	GuiEvent data;
	data.Type = _MOUSEWHEEL;
	data.Key = 0;
	data.Modifier = 0;
	data.Virtual = 0;
	data.Scroll = distance;

	Event ev;
	ev.Data = &data;
	ev.DataLength = 0;
	ev.EventIndex = 0;
	ev.Type = _GUIEVENT;
	return Device->SendEvent(&ev);
}

