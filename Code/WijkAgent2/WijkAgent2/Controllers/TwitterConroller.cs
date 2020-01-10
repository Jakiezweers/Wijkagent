using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace WijkAgent2.Controllers
{
    public class TwitterConroller
    {
        //Get tweets based on LONG, LAT & Date
        public List<ITweet> getTwitterBerichten(double longitude, double latitutde, DateTime Date)
        {
            //Wait 450MS
            Thread.Sleep(450);
            // Setting Credentials
            Auth.SetUserCredentials("itpO8X73ey8dkZTyGJVsIx5sI", "WKs54HvEZJdxnKkNm8apcyhIEcqCEKcYaKbvpxyoKnhSx6RZMc", "3374540458-5LHiTuas6A4PCrWQKkzYhf71MlEbUekNq1PPw7E", "DArMiCPh51mCi0BywNplin9rRvRZayixrUqnUnYpgXfs9");

            //Setting Search Parameters
            var searchParameter = new SearchTweetsParameters("")
            {
                GeoCode = new GeoCode(latitutde, longitude, 15, DistanceMeasure.Kilometers),
                MaximumNumberOfResults = 100,
                SearchType = SearchResultType.Recent,
                Until = Date,
            };

            //Getting tweets Based on Search Parameters
            var tweets = Search.SearchTweets(searchParameter);

            //Convert to list
            List<ITweet> tweet_list = tweets.ToList();

            //Return the list of ITweets
            return tweet_list;
        }
    }
}
