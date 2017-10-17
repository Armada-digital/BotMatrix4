using System;
using System.IO;
using System.Collections.Generic;
using LinqToTwitter;
using ConsoleUtilities;
using System.Linq;

namespace BotMatrix4
{



	public static class Futil
	{

		public async static void FirstRun()
		{
			try
			{
				long cursor = -1;

				Session.followed = new List<Follower>();
				Session.unfollowed = new List<Follower>();

				var userResponse = (from user in Session.service.User where user.Type == UserType.Lookup && user.ScreenNameList == Session.screenname select user).ToList();
				int following = 0;
				if (userResponse != null)
				{
					foreach (User user in userResponse)
					{
						following = user.FriendsCount;
					}


					var friendship = await (from friend in Session.service.Friendship
											where friend.Type == FriendshipType.FriendsList && friend.ScreenName == Session.screenname && friend.Cursor == cursor && friend.Count == following
											select friend).SingleOrDefaultAsync();

					if (friendship != null && friendship.Users != null && friendship.CursorMovement != null)
					{

						foreach (User user in friendship.Users)
						{
							Follower f = new Follower(user.ScreenNameResponse, DateTime.Now);
							Session.followed.Add(f);
						}

					}

					SaveFollowers(Session.followed, "following.log");

					Session.unfollowed.Add(new Follower(Session.screenname, DateTime.Now));
					SaveFollowers(Session.unfollowed, "unfollowed.log");


				}
				else
				{
					Cutil.Error("<First Run> - Failed, please try again");
				}
			}
			catch (Exception ex)
			{
				Cutil.Error("<First Run> - " + ex.Message);
			}
				Cutil.Line("<First Run> - Please restart");
		}

		public static void SaveFollowers(List<Follower> followers, string file)
		{

			//Cutil.Line("<Saving Followers> - Saving to '/" + file + "'");

			using (Stream stream = File.Open(Session.path + file, FileMode.Create))
			{
				var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

				bformatter.Serialize(stream, followers);
				stream.Close();
			}
		}

		public static List<Follower> LoadFollowers(string file)
		{
			//deserialize
			//Cutil.Line("<Load Followers> - Loading from '/" + file +"'");

			using (Stream stream = File.Open(Session.path + file, FileMode.Open))
			{
				var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				if (stream.Length != 0)
				{
					List<Follower> followers = (List<Follower>)bformatter.Deserialize(stream);
					stream.Close();
					return followers;
				}
				else
				{

					return null;
				}


			}



		}

		public static void Record(Status status)
		{
			using (Stream stream = File.Open(Session.path + "following.log", FileMode.Append))
			{
				var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

			

				Session.followed.Add(new Follower(status.ScreenName, DateTime.Now));

				bformatter.Serialize(stream, Session.followed);

				Cutil.Line("<Record follower> - Recorded", ConsoleColor.Cyan);

			}
		}
	}




	[Serializable]
	public class Follower
	{
		private string screenname;
		private DateTime time;

		public Follower(string screenname, DateTime time)
		{
			this.screenname = screenname;
			this.time = time;
		}

		public string ScreenName
		{
			get { return screenname; }
		}
		public DateTime Time
		{
			get { return time; }
		}
	}


}