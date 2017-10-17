using System;
using LinqToTwitter;
using ConsoleUtilities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
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
			
				/*
				foreach (Follower follower in Session.followed)
				{
					double t = (DateTime.Now - follower.Time).TotalMinutes;
					Cutil.Line("<Main> - Followers name: " + follower.ScreenName.PadRight(16) + " age: " + t, ConsoleColor.Gray);
				}

				foreach (Follower follower in Session.unfollowed)
				{
					Cutil.Line("<Main> - Unfollowed: name- " + follower.ScreenName + " time- " + follower.Time, ConsoleColor.Gray);
				}
				*/


			


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
					if (age > 604800 / 3.5 ) // 604800 = 7 days - unfollow, then dont follow again for a long time
					{
						Cutil.Line("<Unfollow> - " + f.ScreenName + " is of age." +" (" + age+")", ConsoleColor.DarkRed);
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
										Cutil.Line("<Unfollow> - User, " + f.ScreenName + ", not followed back unfollowing", ConsoleColor.DarkCyan);
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
			
				Cutil.Line("<BotMatrix> - Waiting " + interval + " minutes");
				Thread.Sleep(interval * 60000);

			}
		}


	}
}
