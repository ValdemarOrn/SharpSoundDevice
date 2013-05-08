# Introduction

SharpSoundDevice is a library that allows you to develop VST-compatible plugins (or "devices") using .NET. A plugin developer can reference the SharpSoundDevice.dll assembly, develop a plugin that implmenents the IAudioDevice interface and load it in any VST 2.4 -compatible host.

![](img/Flow_small.png)

# Features

* Quickly build audio plugins using C# (or another .NET language).
* Use WinForms or WPF to develop user interfaces.
* Open source, MIT Licensed.

# Quickstart

The quickstart contains 2 demo plugins and covers the development process.

[View the quickstart guide](/Quickstart.md)

# Download

Stable versions are provided as compiled 32 bit Windows binaries. (64 bit support it planned)

[Get the latest version from the Github repository.](../../tree/master/Builds)

# License

SharpSoundDevice is licensed under the MIT license.

[View the license.](/license.txt)

# Example Plugins

First, look at the Quickstart. Once you get through that, have a look at [my plugin repository](https://github.com/ValdemarOrn/SharpSoundPlugins)

# Documentation

Documentation is still limited, but (almost) all public entities in the SharpSoundDevice library have code comments that should answer most questions.

[Documented source code for SharpSoundDevice Assembly](../../tree/master/SharpSoundDevice)

## Current Limitations

* **Currently the plugin bridge only supports one IAudioDevice per assembly!** That means, if you create more than one class inside your .NET assembly that implements this interface, it might not be able to load it. This is a limitation that may (or may not :) be changed in the future. Until then, **strictly one device per assembly, please!**
* 32-Bit only (for now). 64 bit support is scheduled. In theory, it should work just fine, I just haven't had the time to verify that it does. But you need to compile your own binaries (no 64 bit compiled binaries are currently available).
	* Note that only the plugin bridge needs to be compiled specially. .NET assemblies, as long as they're compiled by Target: AnyCPU, will work with both 32 and 64 bit hosts.
* Windows only. Linux support is a possibility by using Mono, but requires writing a new bridge adapter.

## To Build

You don't need to build the library from source in order to develop plugins. You can download the latest stable version [from here](../../tree/master/Builds)

You need VST 2.4 SDK to compile from source. The SDK is *not* open source and must be downloaded directly from Steinberg. However, compiled version of the libraries can be freely distributed in compliance with the license.