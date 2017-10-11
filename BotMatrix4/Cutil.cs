using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
namespace ConsoleUtilities
{
	public class Cutil
	{
		private static string path;
		public static void Initialize()
		{

			string loc = System.Reflection.Assembly.GetEntryAssembly ().Location;
			string name = System.AppDomain.CurrentDomain.FriendlyName;
			path = loc.Substring (0, loc.Length - name.Length);
			//Line( "Cutil Initialized. Log path: " + path);
		}
		private static Object thisLock = new Object();
		public static void Log (string text)
		{
			lock(thisLock)
			{
				using (StreamWriter w = File.AppendText(path + "bot.log"))
				{
					w.WriteLine($"<{DateTime.Now}> " + text);
					w.Close();
				}
			}
		}

		public static void Line (string text)
		{
			
			Console.WriteLine($"<{DateTime.Now}> - " + text);
			Log($"<{DateTime.Now}> - " + text);

		}

		public static void Line (string text, ConsoleColor color)
		{
			
			Console.ForegroundColor = color;
			Console.WriteLine($"<{DateTime.Now}> - " + text);
			Log($"<{DateTime.Now}> - " + text);
			Console.ResetColor();
		}

		public static void Error(Exception ex)
		{



			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"<{DateTime.Now}> - " + ex.Message);
			Log($"<{DateTime.Now}> - " + ex.Message);
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine(ex.StackTrace);
			Log(ex.StackTrace);
			Console.ResetColor();
		}

		public static void Error(string text)
		{

			//StackTrace stackTrace = new StackTrace();
			//string method = stackTrace.GetFrame(1).GetMethod().Name;

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"<{DateTime.Now}> - " + text);
			Log($"<{DateTime.Now}> - " + text);
			Console.ResetColor();
		}

	}
}

