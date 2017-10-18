using System;
using LinqToTwitter;
using ConsoleUtilities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using PromptGenerator;
using TweetSharp;

namespace BotMatrix4
{
	class MainClass
	{
		private static bool isRunning = true;
	
		public static void Main(string[] args)
		{
			Session.Initialize();
			Session.Login();


			Session.followed = Futil.LoadFollowers("following.log");
			Session.unfollowed = Futil.LoadFollowers("unfollowed.log");

			if (Session.followed == null)
			{
				Cutil.Line("<Main> - Initializing");
				Cutil.Line("<Main> - If are following 0 users, then this bot will not work.");
				try
				{
					Futil.FirstRun();
				}
				catch (Exception ex)
				{
					Cutil.Error("<Main> - " + ex.Message);
				}
			
			}
			else
			{

				Thread botMatrix = new Thread(BotMatrix);
				botMatrix.Start();
				Thread unfollow = new Thread(Unfollow);
				unfollow.Start();
				Thread prompts = new Thread (Prompts);
				prompts.Start ();
				Thread followFriday = new Thread(FollowFriday);
				followFriday.Start();
			
			}
			Console.ReadLine();
		}

		private static void BotMatrix()
		{
			Cutil.Line ("<BotMatrix> - Start");
			while (isRunning)
			{
				Random rnd = new Random();
				int interval = rnd.Next(4, 11);


				List<Status> statuses = Util.RetweetRandomHashtag();


				statuses = Util.FilterMeOut(statuses);


				if (Util.Ratio(Session.desiredRatio))
				{

					foreach (Status status in statuses)
					{
						if (Util.FollowFilter (status)) 
						{
							Util.FollowUserByStatus (status);
						}
						else 
						{
							Cutil.Line ("<Follow Filter> - Rejected" + status.User.ScreenNameResponse, ConsoleColor.DarkYellow);
						}
					}


				}




				Cutil.Line("<BotMatrix> - Waiting " + interval + " minutes");
				Thread.Sleep(interval * 60000);

			}
		}
		private static void Unfollow()
		{


			Cutil.Line ("<Unfollow> - Start");
			while (isRunning)
			{
				int count = 0;
				int interval = 60; //60 minutes per interval
				List<Follower> tempList = new List<Follower>(Session.followed);


				foreach (Follower f in tempList)
				{
					DateTime now = DateTime.Now;
					double age = (now - f.Time).TotalSeconds;
					if (age > Session.waitTime * 86400) 
					{
						Cutil.Line("<Unfollow> - " + f.ScreenName + " is of age." +" (" + age+")", ConsoleColor.Cyan);
						try
						{
							var friendships = Util.FindFriendship(Session.screenname, f.ScreenName);
							count++;
							if (count > 30) {
								Cutil.Line("<Unfollow> - Internal limit reached", ConsoleColor.DarkCyan);
								break;
							}
							if (friendships != null)
							{
								foreach (Friendship friend in friendships.Result)
								{

									if (friend.SourceRelationship.FollowedBy)
									{
										Cutil.Line("<Unfollow> - " + f.ScreenName + " follows me.", ConsoleColor.DarkCyan);
										Session.followed.Remove(Session.followed.SingleOrDefault(x => x.ScreenName == f.ScreenName));
										Futil.SaveFollowers(Session.followed, "following.log");
									}
									else
									{
										Cutil.Line("<Unfollow> - User, " + f.ScreenName + ", not followed back", ConsoleColor.DarkCyan);
										Futil.LogUnfollow(f.ScreenName);
										Util.UnfollowUserByScreenname(f.ScreenName);
									}
								}
							}
						}
						catch (Exception ex)
						{
							Cutil.Error("<Unfollow> - " + ex.Message);
						}
					}
					else
					{
						Cutil.Line("<Unfollow> - " + f.ScreenName + " is not of age.".PadRight(20), ConsoleColor.Cyan);
					}

				}
			
				Cutil.Line("<Unfollow> - Waiting " + interval + " minutes");
				Thread.Sleep(interval * 60000);

			}
		}
		private static void Prompts()
		{
			while (isRunning) 
			{
				Random rnd = new Random ();
				int interval = rnd.Next (60, 180);

				Util.SendTweet (ThreeCharacters.ReturnThreeCharacters() + "#WritingPrompt #ThreeRandomCharacters");

				Cutil.Line("<Prompts> - Waiting " + interval + " minutes");
				Thread.Sleep(interval * 60000);
			}
		}
		private static async void FollowFriday()
		{

			List<Follower> ffl = null;

			while (isRunning)
			{
				Random rnd = new Random();
				int interval = rnd.Next(30, 45);

				if (DateTime.Now.DayOfWeek == DayOfWeek.Friday && ffl != null) {
					if (ffl.Count > 7) {
						string a, b, c, d, e, f, g;
						a = " @" + ffl.Last ().ScreenName;
						ffl.RemoveAt (ffl.Count - 1);
						b = " @" + ffl.Last ().ScreenName;
						ffl.RemoveAt (ffl.Count - 1);
						c = " @" + ffl.Last ().ScreenName;
						ffl.RemoveAt (ffl.Count - 1);
						d = " @" + ffl.Last ().ScreenName;
						ffl.RemoveAt (ffl.Count - 1);
						e = " @" + ffl.Last ().ScreenName;
						ffl.RemoveAt (ffl.Count - 1);
						f = " @" + ffl.Last ().ScreenName;
						ffl.RemoveAt (ffl.Count - 1);
						g = " @" + ffl.Last ().ScreenName;
						ffl.RemoveAt (ffl.Count - 1);
						try {
							Util.SendTweet ("#FF " + a + b + c + d + e + f + g);
						} catch (Exception ex) {
							Cutil.Error ("<Follow Friday> - " + ex.Message);
						}
					}
				} else {
					

					try {
						ffl = new List<Follower> ();

						var followers = await Util.ReturnFriends (Session.screenname);

						foreach (User u in followers) {
							ffl.Add (new Follower (u.ScreenNameResponse, DateTime.Now));
						}

						switch(DateTime.Now.DayOfWeek){
						case DayOfWeek.Monday:
								interval = 4320;
							break;
						case DayOfWeek.Tuesday:
								interval = 2880;
							break;
						case DayOfWeek.Wednesday:
								interval = 1440;
							break;
						case DayOfWeek.Thursday:
								interval = 60;
							break;
						case DayOfWeek.Friday:
								interval = 0;
							break;
						case DayOfWeek.Saturday:
								interval = 4320;
							break;
						case DayOfWeek.Sunday:
								interval = 4320;
							break;

						}


					} catch (Exception ex) {
						Cutil.Error ("<Follow Friday> - " + ex.Message);
						interval = 60;
					}

				}

				Cutil.Line("<Follow Friday> - Waiting " + interval + " minutes");
				Thread.Sleep(interval * 60000);

			
			}



		}

	}
}
