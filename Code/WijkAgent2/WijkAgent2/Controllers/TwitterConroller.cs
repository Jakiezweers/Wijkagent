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
    class TwitterConroller
    {
        public List<ITweet> getTwitterBerichten()
        {
            Thread.Sleep(450);
            // Set up your credentials (https://apps.twitter.com)
            Auth.SetUserCredentials("itpO8X73ey8dkZTyGJVsIx5sI", "WKs54HvEZJdxnKkNm8apcyhIEcqCEKcYaKbvpxyoKnhSx6RZMc", "3374540458-5LHiTuas6A4PCrWQKkzYhf71MlEbUekNq1PPw7E", "DArMiCPh51mCi0BywNplin9rRvRZayixrUqnUnYpgXfs9");

            var searchParameter = new SearchTweetsParameters("")
            {
                GeoCode = new GeoCode(52.516773, 6.083022, 1, DistanceMeasure.Kilometers),
                MaximumNumberOfResults = 100,
                SearchType = SearchResultType.Recent,
                Until = new DateTime(2019, 12, 20),
            };

            var tweets = Search.SearchTweets(searchParameter);

            List<ITweet> tweet_list = tweets.ToList();

            return tweet_list;
        }
    }
}
