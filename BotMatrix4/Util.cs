using System;
using LinqToTwitter;
using ConsoleUtilities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BotMatrix4
{
	public class Util
	{

		#region Search Functions
		public async static Task<List<Friendship>> FindFriendship(string source, string target)
		{
			try
			{
				var friendships = await (from friendship in Session.service.Friendship
										 where friendship.Type == FriendshipType.Show && friendship.SourceScreenName == source && friendship.TargetScreenName == target
										 select friendship).ToListAsync();
				if (friendships != null)
				{
					return friendships;
				}
				else
				{
					return null;
				}

			}
			catch (Exception ex)
			{
				Cutil.Error("<Find Friendship> - " + ex.Message);
				return null;
			}
		}

		public async static Task<List<User>> ReturnFriends(string source)
		{
			var friendList =await(from f in Session.service.Friendship
									where f.Type == FriendshipType.FriendsList && f.ScreenName == source
									select f).SingleOrDefaultAsync();
			if (friendList != null)
			{
				return friendList.Users;
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region Follow Functions
		public async static void UnfollowUserByScreenname(string screenname)
		{
			try
			{

				Cutil.Line("<Unfollow user by ScreenName> - Screenname: " + screenname, ConsoleColor.DarkCyan);

				var user = await Session.service.DestroyFriendshipAsync(screenname, default(CancellationToken));

				if (user != null)
				{
					Cutil.Line("<Unfollow user by ScreenName> - user unfollowed.", ConsoleColor.DarkCyan);

					Session.followed.Remove(Session.followed.SingleOrDefault(x => x.ScreenName == screenname));
					Session.unfollowed.Add(new Follower(screenname, DateTime.Now));
					Futil.SaveFollowers(Session.unfollowed, "unfollowed.log");
					Futil.SaveFollowers(Session.followed, "following.log");
				}

			}
			catch (Exception ex)
			{
				Cutil.Error("Unfollow user by ScreenName> " + ex);
			}

		}
		public async static void FollowUserByStatus(Status status)
		{
			try
			{

				//check if we have unfollowed them before, then abort if needed
				if (CheckUnfollowed(status))
				{

					string screenname = status.User.ScreenNameResponse;

					Cutil.Line("<Followe user by status> - Screenname: " + screenname + " Attempting follow.", ConsoleColor.Yellow);

					if (FollowFilter(status))
					{
						//Followe user:

						var user = await Session.service.CreateFriendshipAsync(screenname, true, default(CancellationToken)); // <- Thats the bastard!


						if (user != null && Session.followed.Any(x => x.ScreenName == screenname) == false)
						{
							Cutil.Line("<Follow user by status> - " + screenname + "success", ConsoleColor.Yellow);

							Session.followed.Add(new Follower(screenname, DateTime.Now));
							Futil.SaveFollowers(Session.followed, "following.log");
						}
						else
						{
							Cutil.Line("<Followuser by status> - User is null, or already followed.", ConsoleColor.Yellow);
						}

					}
					else
					{
						Cutil.Line("<Follow user by status> - Rejected user, " + screenname + ", did not meet criteria.", ConsoleColor.DarkYellow);
					}
				}
				else
				{
					Cutil.Line("<Follow user by status> - Already followed and unfollowed this user: " + status.User.ScreenNameResponse, ConsoleColor.DarkYellow);
				}

			}
			catch (Exception ex)
			{
				Cutil.Error("<Follow user by status> - " + ex.Message);
			}
		}
		#endregion

		#region Hashtag Functions
		/// <summary>
		/// Retweets the random hashtag, and returns the list of candidates as a generic list.
		/// </summary>
		public static List<Status> RetweetRandomHashtag()
		{

			Random rnd = new Random();
			int randomNumberHashtag = rnd.Next(0, Session.hashtags.Count);
			List<Status> tweets = Search(Session.hashtags[randomNumberHashtag]).ToList();


			if (tweets != null)
			{
				Retweet(tweets[rnd.Next(0, tweets.Count)]);
			}
			else
			{
				Cutil.Error("<Retweet random hashtag> - Tweet failed, search retruned null.");
			}

			return tweets;
		}
		#endregion Hashtag Functions

		#region FollowFriday

		public static List<User> GetFriends(string source)
		{
			var list = ReturnFriends(source);
			return null;
		}

		#endregion

		#region Simple Functions
		public async static void Retweet(Status status)
		{
			Cutil.Line("<Retweet> - Retweeting: " + status.StatusID);
			try
			{
				var retweet = await Session.service.RetweetAsync(status.StatusID);

				if(retweet != null && retweet.RetweetedStatus != null && retweet.RetweetedStatus.User != null){
					Cutil.Line("<Retweet> - Retweeted:", ConsoleColor.Green);
					Cutil.Line("<Retweet> - User: " + retweet.RetweetedStatus.User.ScreenNameResponse, ConsoleColor.Green);
					Cutil.Line("<Retweet> - Tweet Id: " + retweet.RetweetedStatus.StatusID, ConsoleColor.Green);

				}
			}
			catch (Exception ex)
			{
				Cutil.Error("<Retweet> -" + ex.Message);
				RetweetRandomHashtag ();//this line isnt really generic enough to go here
			}

		}

		public static void SendTweet(string text)
		{
			Session.service.TweetAsync(text);
			Cutil.Line("<Send tweet> - Tweeted: " + text);		}
		#endregion 

		#region filtration

		public static bool CheckUnfollowed(Status status)
		{
			if (Session.unfollowed.Any(s => s.ScreenName.Contains(status.User.ScreenNameResponse)))
			{
				return false; //false because we have already unfollowed this person, so don't follow them again
			}
			else
			{
				return true; //we have not unfollowed this person.
			}
		}

		public static List<Status> FilterMeOut(List<Status> statuses)
		{


			statuses.RemoveAll(x => x.User.ScreenNameResponse.ToLower() == Session.screenname.ToLower());

			return statuses;
		}

		public static bool FollowFilter(Status status)
		{
			string userText = status.User.Description;

			foreach (string word in Session.followwords)
			{
				if (userText.Contains(word))
					return true;
			}
			return false;
		}
		/// <summary>
		/// Returns false if the filter detects any of the filterwords in the status.
		/// </summary>
		public static bool Filter(Status status)
		{
			string statusText = status.Text;

			foreach (string word in Session.filterwords)
			{
				if (statusText.Contains(word))
					return false;
			}
			return true;
		}
		/// <summary>
		/// Check bot owners follower vs following ratio. Returns true if ratio is less than desiredRatio
		/// </summary>
		/// <returns>The ratio.</returns>
		/// <param name="desiredRatio">Desired ratio.</param>
		public static bool Ratio(double desiredRatio)
		{

			var userResponse = (from user in Session.service.User where user.Type == UserType.Lookup && user.ScreenNameList == Session.screenname select user).ToList();

			double following = 0;
			double followers = 0;
			double actualRatio = 0;

			foreach (User user in userResponse)
			{
				following = user.FriendsCount;
				followers = user.FollowersCount;
				actualRatio = following / followers;
			}

			if (actualRatio <= desiredRatio)
			{
				Cutil.Line("<Ratio> - Ratio: " + actualRatio + ". Acceptable", ConsoleColor.Magenta);
				return true;
			}
			else
			{
				Cutil.Line("<Ratio> - Ratio: " + actualRatio + ". Unacceptable", ConsoleColor.DarkMagenta);
				return false;
			}



		}
		/// <summary>
		/// Search the specified hashtag - and returns results as a generic list.
		/// </summary>
		/// <param name="hashtag">Hashtag.</param>
		public static List<Status> Search(string hashtag)
		{
			IQueryable<Search> searches = from search in Session.service.Search
										  where search.Type == SearchType.Search && search.Count == 10 && search.Query == "#" + hashtag && search.SearchLanguage == "en"
										  select search;

			List<Search> results = searches.ToList();

			List<Status> result = results[0].Statuses;

			return result;

		}
		#endregion filtration

	}
}
