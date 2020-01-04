using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using WijkAgent2.Classes;
using WijkAgent2.Database;
using WijkAgent2.Modals;

namespace WijkAgent2.Pages.delicten
{
    /// <summary>
    /// Interaction logic for view_delict.xaml
    /// </summary>
    public partial class view_delict : Page
    {
        MainWindow mw;
        int viewDelictID;
        private Connection cn = new Connection();
        int returnPage = 0;
        
        public view_delict(MainWindow MW, int delictID, int originalPage)
        {
            viewDelictID = delictID;
            InitializeComponent();
            LoadDelict(delictID);
            mw = MW;
            returnPage = originalPage;
            LoadComments();
        }

        private void Get_Tweets()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Modals.Tweet tmp = new Modals.Tweet();
                tmp.User.Text = "Een moment geduld....";
                tmp.Date.Text = "";
                tmp.tweet_text.Text = "";
                tmp.Date.Visibility = Visibility.Hidden;
                tmp.tweet_text.Visibility = Visibility.Hidden;
                tmp.BtnShowTweet.Visibility = Visibility.Hidden;
                List_Tweets.Children.Add(tmp);
            }));
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
            Thread.Sleep(800);
            Dispatcher.BeginInvoke((Action)(() =>
            {
                List_Tweets.Children.Clear();
            }));

            List<ITweet> tweet_list = tweets.ToList();
            foreach (ITweet tweet in tweet_list)
            {
                if (!tweet.IsRetweet)
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Modals.Tweet t = new Modals.Tweet();
                        t.User.Text = tweet.CreatedBy.ScreenName;
                        t.Date.Text = tweet.CreatedAt.ToString("dd-MM-yyyy h:mm tt");
                        t.tweet_text.Text = tweet.FullText;
                        List_Tweets.Children.Add(t);
                    }));
                }
            }
        }

        private void LoadDelict(int viewDelictID)
        {
            cn.OpenConection();
            SqlDataReader sqc = cn.DataReader("SELECT * FROM dbo.delict WHERE delict_id = " + viewDelictID);
            while (sqc.Read())
            {
                string status;
                if ((int)sqc["status"] == 1)
                {
                    status = "Actief";
                }
                else
                {
                    status = "Inactief";
                }

                DelictPlaceLabel.Content += ": " + sqc["place"];
                DelictIDLabel.Content += ": " + sqc["delict_id"];
                DelictStreetLabel.Content += ": " + sqc["street"];
                DelictHouseNumberLabel.Content += ": " + sqc["housenumber"] + " " + sqc["housenumberAddition"];
                DelictZipcodeLabel.Content += ": " + sqc["zipcode"];
                DelictStatusLabel.Content += ": " + status;
                DelictDescriptionTB.Text = (string)sqc["description"];
                DelictDateLabel.Content += ": " + sqc["added_date"];
            }

            cn.CloseConnection();
            cn.OpenConection();

            SqlDataReader sqcd = cn.DataReader("SELECT category.name FROM category_delict JOIN category ON category.category_id = category_delict.category_id WHERE delict_id = " + viewDelictID);

            while (sqcd.Read())
            {
                CategoryListbox.Items.Add(sqcd["name"]);
            }

            cn.CloseConnection();
            cn.OpenConection();

            SqlDataReader sqp = cn.DataReader("SELECT p.bsn, dp.type FROM delict_person dp JOIN person p on dp.person_id = p.person_id WHERE delict_id = " + viewDelictID);

            while (sqp.Read())
            {
                string text = sqp["type"] + " - " + sqp["bsn"];
                PersonenListbox.Items.Add(text);
            }
            cn.CloseConnection();

            Thread thr = new Thread(Get_Tweets);
            thr.IsBackground = true;
            thr.Start();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(returnPage == 1)
            {
                mw.ShowDelictenList(true);
                return;
            }
            if (returnPage == 2)
            {
                mw.ShowDelictenArchive();
                return;
            }
            if (returnPage == 3)
            {
                mw.LoadHomeScreen();
                return;
            }
            mw.LoadHomeScreen();
        }
        private void EditDelict_Click(object sender, RoutedEventArgs e)
        {
            mw.EditDelict(viewDelictID,returnPage);
        }

        /**Sending comment to database on the 'place comment' click*/
        private void PlaceComment_Click(object sender, RoutedEventArgs e)
        {

            int user = mw.GetUserID();
            int delict = viewDelictID;
            string comment = WriteCommentTextBox.Text;
            //DateTime date = DateTime.Today;
            Console.WriteLine($"{user} | {delict} | {comment} |");

            try
            {
                cn.OpenConection();
                string query = $"INSERT INTO delict_comment (user_id, delict_id, comment, date_added) VALUES ({user}, {delict}, '{comment}', GETDATE())";

                using (SqlCommand sqlCommand = new SqlCommand(query))
                {
                    sqlCommand.Connection = cn.GetConnection();
                    sqlCommand.ExecuteScalar();
                    // CommentTest.Content = "Comment verzonden";
                }

                cn.CloseConnection();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
        }

        /**Fetching all comments from database and putting them in List*/
        private List<Comment> GetCommentList()
        {
            List<Comment> comments = new List<Comment>();
            string query = $"SELECT name, comment, date_added FROM delict_comment dc JOIN dbo.[User] u ON dc.user_id = u.user_id WHERE delict_id = {viewDelictID} ORDER BY date_added DESC";
            try
            {
                cn.OpenConection();

                SqlDataReader reader = cn.DataReader(query);
                Console.WriteLine("Now reading . . .");
                while (reader.Read())
                {
                    Comment c = new Comment();
                    c.CommentPoster = (string)reader["name"];
                    c.CommentText = (string)reader["comment"];
                    c.CommentDate = (DateTime)reader["date_added"];
                    comments.Add(c);
                }
                cn.CloseConnection();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return comments;
        }

        public void LoadComments()
        {
            GetCommentList();
            

            foreach (Comment comment in GetCommentList())
            {
                Console.WriteLine(comment);
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    Modals.CommentLayout commentLayout = new Modals.CommentLayout();
                    commentLayout.CommentTextLabel.Content = comment.CommentText;
                    //commentLayout.CommentUserImage.Source =  
                    commentLayout.CommentUserName.Content = comment.CommentPoster;
                    commentLayout.CommentDateLabel.Content = comment.CommentDate.ToString();
                    CommentListPanel.Children.Add(commentLayout);
                }));

            }
        }
    }
}
