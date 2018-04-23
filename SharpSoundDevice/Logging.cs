using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SharpSoundDevice
{
	/// <summary>
	/// Logging framework for SharpSoundDevice bootstrapping code.
	/// 
	/// Not to be used by plugin devices!
	/// </summary>
	public static class Logging
	{
		private static readonly string logfile;
		private static readonly FileStream logfileStream;
		private static readonly ConcurrentQueue<string> logQueue;

		public static ConcurrentBag<Action<string>> LogHandlers;

		static Logging()
		{
			LogHandlers = new ConcurrentBag<Action<string>>();
			logQueue = new ConcurrentQueue<string>();
			var dir = Path.Combine(Environment.ExpandEnvironmentVariables("%AppData%"), "SharpSoundDevice", "Logs");
			Directory.CreateDirectory(dir);
			var filename = string.Format("SharpSoundDevice-{0:yyyy-MM-dd-HHmmss}.log", DateTime.Now);
			logfile = Path.Combine(dir, filename);

			Log(string.Format("Initializing SharpSoundDevice system.\nProcess: {0}\nExecutable: {1}", 
				Process.GetCurrentProcess().ProcessName,
				Process.GetCurrentProcess().MainModule.FileName));

			CleanupOldLogs(dir);

			// logger lives for entire process lifespan, no need to dispose
			logfileStream = new FileStream(logfile, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
			var flusher = new Thread(() => FlushLogs());
			flusher.Priority = ThreadPriority.Lowest;
			flusher.IsBackground = true;
			flusher.Start();

			AppDomain.CurrentDomain.ProcessExit += (s, e) => FlushInternal();
		}

		/// <summary>
		/// Removes log files older than 60 days
		/// </summary>
		/// <param name="dir"></param>
		private static void CleanupOldLogs(string dir)
		{
			var files = Directory.GetFiles(dir).Where(x => Path.GetFileName(x).StartsWith("SharpSoundDevice-")).ToArray();
			foreach (var file in files)
			{
				var dateString = Path.GetFileNameWithoutExtension(file.Replace("SharpSoundDevice-", ""));
				DateTime date;
				var ok = DateTime.TryParseExact(dateString, "yyyy-MM-dd-HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
				if (!ok)
					continue;

				if ((DateTime.Today - date.Date).TotalDays > 60)
				{
					try
					{
						File.Delete(file);
					}
					catch (Exception ex)
					{
						Log(string.Format("Unable to clean up old logfile '{0}'\n{1}", file, ex.GetTrace()));
					}
				}
			}
		}

		private static void FlushLogs()
		{
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

		public static void Log(string message)
		{
			var ts = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] ";
			var msg = ts + message;
			Console.WriteLine(msg);
			logQueue.Enqueue(msg);
			foreach (var handler in LogHandlers)
				handler(msg);
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
			Log($"Exception raised by DeviceId {id}:" + msg);
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
