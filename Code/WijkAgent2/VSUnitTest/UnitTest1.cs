using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tweetinvi.Models;
using WijkAgent2.Controllers;

namespace VSUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestTwitterFeed()
        {
            TwitterConroller TC = new TwitterConroller();
            List<ITweet> TweetList = TC.getTwitterBerichten(52.4976754, 6.0815151, DateTime.Now);
            Console.WriteLine(TweetList.Count);
        }
    }
}
