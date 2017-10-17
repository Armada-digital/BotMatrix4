using System;
using System.IO;
using ConsoleUtilities;
using LinqToTwitter;
using System.Collections.Generic;
namespace BotMatrix4
{
	public class Session
	{
		public static TwitterContext service;
		public static string path;
		public static string customer_key;
		public static string customer_key_secret;
		public static string access_token;
		public static string access_token_secret;

		public static List<string> hashtags = new List<string>();
		public static List<string> followwords = new List<string>();
		public static List<string> filterwords = new List<string> ();

		public static string screenname;
		public static ulong userID;
		public static double desiredRatio;

		public static List<Follower> followed;
		public static List<Follower> unfollowed;


		public static void Initialize ()
		{
			bool fileEnded = false;
			string loc = System.Reflection.Assembly.GetEntryAssembly ().Location;
			string name = System.AppDomain.CurrentDomain.FriendlyName;
			path = loc.Substring (0, loc.Length - name.Length);

			//followed = new List<Follower>();
			//unfollowed = new List<Follower>();

			using (StreamReader r = new StreamReader (path + "bot.config")) {


				while (fileEnded == false)
				{
					string line = r.ReadLine();
					if (line != null && !line.StartsWith("#") && line != "")
					{
						customer_key = line;
						fileEnded = true;
					}
				}
				fileEnded = false;

				while (fileEnded == false)
				{
					string line = r.ReadLine();
					if (line != null && !line.StartsWith("#") && line != "")
					{
						customer_key_secret = line;
						fileEnded = true;
					}
				}
				fileEnded = false;

				while (fileEnded == false)
				{
					string line = r.ReadLine();
					if (line != null && !line.StartsWith("#") && line != "")
					{
						access_token = line;
						fileEnded = true;
					}
				}
				fileEnded = false;

				while (fileEnded == false)
				{
					string line = r.ReadLine();
					if (line != null && !line.StartsWith("#") && line != "")
					{
						access_token_secret = line;
						fileEnded = true;
					}
				}
				fileEnded = false;

				while (fileEnded == false)
				{
					string line = r.ReadLine();
					if (line != null && !line.StartsWith("#") && line != "")
					{
						screenname = line;
						fileEnded = true;
					}
				}
				fileEnded = false;

				while (fileEnded == false)
				{
					string line = r.ReadLine();
					if (line != null && !line.StartsWith("#") && line != "")
					{
						try
						{
							desiredRatio = double.Parse(line);
						}
						catch (Exception ex)
						{
							Cutil.Error("<Initialize> - Please enter valid desiredratio in config file");
							Cutil.Error("<Initialize) - " + ex.Message);
						}
						fileEnded = true;
					}
				}
				fileEnded = false;




				while (fileEnded == false) {
					string line = r.ReadLine ();

					if (line == null || line.StartsWith ("#") || line == "") {
						//do nothing
					} else if (line.Contains ("<end>")) {
						fileEnded = true;
					} else {
						hashtags.Add (line);
					}

				}
				fileEnded = false;

				while (fileEnded == false) {
					string line = r.ReadLine ();

					if (line == null || line.StartsWith ("#") || line == "") {
						//do nothing
					} else if (line.Contains ("<end>")) {
						fileEnded = true;
					} else {
						followwords.Add (line);
					}

				}
				fileEnded = false;

				while (fileEnded == false) {
					string line = r.ReadLine ();

					if (line == null || line.StartsWith ("#") || line == "") {
						//do nothing
					} else if (line.Contains ("<end>")) {
						fileEnded = true;
					} else {
						filterwords.Add (line);
					}

				}
				fileEnded = false;
				Cutil.Line ("<Initialize> - Session initialization complete");

				Console.Title = "BotMatrix4 - " + screenname;
			}
		}

		public static void Login()
		{
			

			var auth = new SingleUserAuthorizer
			{
				CredentialStore = new InMemoryCredentialStore
				{
					ConsumerKey = Session.customer_key,
					ConsumerSecret = Session.customer_key_secret,
					OAuthToken = Session.access_token,
					OAuthTokenSecret = Session.access_token_secret
				}
			};


			service = new TwitterContext(auth);



		}
	}
}



