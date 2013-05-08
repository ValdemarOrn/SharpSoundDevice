    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using SharpSoundDevice;
    using System.Globalization;

    namespace SimpleSynth
    {
	    public class Plugin : IAudioDevice
	    {
		    private DeviceInfo DevInfo;
		    private int Semitone;
		    private int Pitch;
		    private double Gate;
		    private double Phase;
		    private double Samplerate;

		    public int CurrentProgram { get; private set; }
		    public DeviceInfo DeviceInfo { get { return DevInfo; } }
		    public IHostInfo HostInfo { get; set; }
		    public Parameter[] ParameterInfo { get; private set; }
		    public Port[] PortInfo { get; private set; }

		    public Plugin()
		    {
			    Samplerate = 48000;
			    DevInfo = new DeviceInfo();
			    DevInfo.Developer = "Valdemar Erlingsson";
			    DevInfo.DeviceID = "Valdemar Erlingsson - SimpleSynth";
			    DevInfo.EditorHeight = 0;
			    DevInfo.EditorWidth = 0;
			    DevInfo.HasEditor = false;
			    DevInfo.Name = "SimpleSynth";
			    DevInfo.ProgramCount = 1;
			    DevInfo.Type = DeviceType.Generator;
			    DevInfo.Version = 1000;
			    DevInfo.VstId = DeviceUtilities.GenerateIntegerId(DevInfo.DeviceID);

			    ParameterInfo = new Parameter[1]
			    {
				    new Parameter() { Display = "0", Index = 0, Name = "Semitones", Steps = 25, Value = 0.5 }
			    };

			    PortInfo = new Port[1]
			    {
				    new Port() { Direction = PortDirection.Output, Name = "Stereo Output", NumberOfChannels = 2 }
			    };
		    }

		    public void InitializeDevice() { }
		    public void DisposeDevice() { }
		    public void Start() { }
		    public void Stop() { }
		    public void OpenEditor(IntPtr parentWindow) { }
		    public void CloseEditor() { }

		    public void HostChanged() 
		    {
			    Samplerate = HostInfo.SampleRate;
		    }

		    public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		    {
			    int note = Pitch + Semitone;
			    double frequency = 440 * Math.Pow(2, (note - 69) / 12.0);
			    var delta = frequency / Samplerate;

			    for (int i = 0; i < bufferSize; i++)
			    {
				    double val = Math.Sin(2 * Math.PI * Phase) * Gate;
				    output[0][i] = val;
				    output[1][i] = val;
				    Phase += delta;
			    }
		    }

		    public void SendEvent(Event ev)
		    {
			    if (ev.Type == EventType.Parameter && ev.EventIndex < ParameterInfo.Length)
			    {
				    Semitone = (int)Math.Round(((double)ev.Data - 0.5) * 24.0);

				    ParameterInfo[ev.EventIndex].Value = (double)ev.Data;
				    ParameterInfo[ev.EventIndex].Display = Semitone.ToString();
			    }
			    else if (ev.Type == EventType.Midi)
			    {
				    byte[] data = (byte[])ev.Data;
				    if ((data[0] & 0xF0) == 0x80) // 0x80 is midi note-off
				    {
					    Gate = 0.0;
				    }
				    else if ((data[0] & 0xF0) == 0x90) // 0x90 is midi note-on
				    {
					    Pitch = data[1];
					    Gate = data[2] / 255.0;
				    }
			    }
		    }

		    public Program GetProgramData(int index)
		    {
			    string programText = String.Format(CultureInfo.InvariantCulture,
				    "{0:0.00}", ParameterInfo[0].Value);
			    byte[] programData = Encoding.ASCII.GetBytes(programText);

			    var program = new Program();
			    program.Name = "Program 1";
			    program.Data = programData;
			    return program;
		    }

		    public void SetProgramData(Program program, int index)
		    {
			    string programText = Encoding.ASCII.GetString(program.Data);
			    double value = Convert.ToDouble(programText, CultureInfo.InvariantCulture);

			    Semitone = (int)Math.Round((value - 0.5) * 24.0);
			    ParameterInfo[0].Value = value;
			    ParameterInfo[0].Display = Semitone.ToString();
		    }
	    }
    }
