using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SharpSoundDevice
{
	public static class LogAppender
	{
		public static int UdpPort = 43219;

		private static bool isStarted;
		private static string logfile;
		private static FileStream logfileStream;
		private static ConcurrentQueue<string> logQueue;
		
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern int AllocConsole();

		public static void Start()
		{
			if (isStarted)
				return;

			AllocConsole();

			logQueue = new ConcurrentQueue<string>();
			var dir = Path.Combine(Environment.ExpandEnvironmentVariables("%AppData%"), "SharpSoundDevice", "Logs");
			Directory.CreateDirectory(dir);
			var filename = string.Format("SharpSoundDevice-{0:yyyy-MM-dd-HHmmss}.log", DateTime.Now);
			logfile = Path.Combine(dir, filename);

			logQueue.Enqueue(string.Format("Initializing SharpSoundDevice system.\nProcess: {0}\nExecutable: {1}\nLogAppender AppDomain:{2}",
				Process.GetCurrentProcess().ProcessName,
				Process.GetCurrentProcess().MainModule.FileName,
				AppDomain.CurrentDomain.FriendlyName));


			new Thread(() => UdpListen()) { Priority = ThreadPriority.Lowest, IsBackground = true }.Start();

			// logger lives for entire process lifespan, no need to dispose
			new Thread(() => FlushLogs()) { Priority = ThreadPriority.Lowest, IsBackground = true }.Start();
			
			AppDomain.CurrentDomain.ProcessExit += (s, e) => FlushInternal();
			isStarted = true;
		}

		private static void UdpListen()
		{
			var ep = new IPEndPoint(IPAddress.Loopback, UdpPort);
			var listener = new UdpClient(ep);

			while (true)
			{
				var data = listener.Receive(ref ep);
				if (data != null && data.Length > 0)
				{
					var text = Encoding.UTF8.GetString(data);
					logQueue.Enqueue(text);
				}
			}
		}

		private static void FlushLogs()
		{
			logfileStream = new FileStream(logfile, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

			while (true)
			{
				Thread.Sleep(1000);

				if (logQueue.Count == 0)
					continue;

				FlushInternal();
			}
		}

		private static void FlushInternal()
		{
			var sb = new StringBuilder();

			while (true)
			{
				string line;
				var ok = logQueue.TryDequeue(out line);
				if (!ok)
					break;

				sb.AppendLine(line);
			}

			try
			{
				var data = Encoding.UTF8.GetBytes(sb.ToString());
				logfileStream.Write(data, 0, data.Length);
				logfileStream.Flush();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to append logfile:\n{0}", ex.GetTrace());
			}
		}
	}

	/// <summary>
	/// Logging framework for SharpSoundDevice bootstrapping code.
	/// 
	/// Not to be used by plugin devices!
	/// </summary>
	public static class Logging
	{
		private static IPEndPoint ep;
		private static readonly UdpClient sender;

		static Logging()
		{
			//logSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			//ep = new IPEndPoint(IPAddress.Loopback, LogAppender.UdpPort);

			sender = new UdpClient(0);
			ep = new IPEndPoint(IPAddress.Loopback, LogAppender.UdpPort);
		}

		public static void Log(string message)
		{
			var ts = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "][" + AppDomain.CurrentDomain.FriendlyName + "] ";
			Console.WriteLine(ts + message);
			var bytes = Encoding.UTF8.GetBytes(ts + message);
            sender.Send(bytes, bytes.Length, ep);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		/// <param name="e"></param>
		public static void LogDeviceException(IAudioDevice device, Exception e)
		{
			LogDeviceException(Interop.GetID(device), e);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="e"></param>
		public static void LogDeviceException(int id, Exception e)
		{
			var msg = e.GetTrace();
			Log("DeviceId " + id + ": " + msg);
		}

		/// <summary>
		/// put all exception details into string
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		internal static string GetTrace(this Exception e)
		{
			var message = "";
			var processed = new HashSet<Exception>();

			var ex = e;
			while (ex != null)
			{
				if (processed.Contains(ex))
					break; // sometimes inner exceptions have circular references!

				processed.Add(ex);
				message += "\r\n==== " + ex.Message + " ====\r\n";
				message += ex.StackTrace;
				ex = e.InnerException;
			}

			return message;
		}
	}
}
