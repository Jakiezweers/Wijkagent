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
using WijkAgent2.Controllers;
using WijkAgent2.Database;
using WijkAgent2.Modals;

namespace WijkAgent2.Pages.delicten
{
    /// <summary>
    /// Interaction logic for view_delict.xaml
    /// </summary>
    public partial class ViewDelict : Page
    {
        readonly MainWindow mw; //Required mainwindow

        readonly int currDelictID; //Delict ID for the current opened delict.

        double longitude; //Longitude for showing tweets
        double latitude; //Latitude for showing tweets
        DateTime date; //Date used for date and showing tweets
        readonly int returnPage = 0; //Return page number for pathing.

        private readonly Connection cn = new Connection();

        public ViewDelict(MainWindow MW, int delictID, int originalPage)
        {
            Validator validator = new Validator();

            currDelictID = delictID;
            InitializeComponent();
            LoadDelict(delictID);
            mw = MW;
            returnPage = originalPage;

            int user_id = mw.GetUserID();
            validator.logged_in_user_id = user_id;
            if (validator.validate("Delicten_Wijzigen")) { BTNPasDelictAan.Visibility = Visibility.Visible; } else { BTNPasDelictAan.Visibility = Visibility.Hidden; }
        }

        //Method to retrieve twitter messages.
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

            TwitterConroller TC = new TwitterConroller();

            List<ITweet> TweetList = TC.getTwitterBerichten(longitude, latitude, date);

            Thread.Sleep(800);

            Dispatcher.BeginInvoke((Action)(() =>
            {
                List_Tweets.Children.Clear();
            }));

            if (TweetList.Count == 0) {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    Modals.Tweet tmp = new Modals.Tweet();
                    tmp.User.Text = "Geen berichten";
                    tmp.Date.Text = "";
                    tmp.tweet_text.Text = "";
                    tmp.Date.Visibility = Visibility.Hidden;
                    tmp.tweet_text.Visibility = Visibility.Hidden;
                    tmp.BtnShowTweet.Visibility = Visibility.Hidden;
                    List_Tweets.Children.Add(tmp);
                }));
            }
            else
            {
                foreach (ITweet tweet in TweetList)
                {
                    if (!tweet.IsRetweet)
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Modals.Tweet t = new Modals.Tweet();
                            t.User.Text = tweet.CreatedBy.ScreenName;
                            t.Date.Text = tweet.CreatedAt.ToString("dd-MM-yyyy h:mm tt");
                            t.tweet_text.Text = tweet.FullText;
                            t.Link.Text = tweet.Url;
                            List_Tweets.Children.Add(t);
                        }));
                    }
                }
            }

        }

        //Method to load in everything needed for the delict. First it loads the delict content itself. Then the categories bound to it. and as last it loads in the persons.
        private void LoadDelict(int currDelictID)
        {
            cn.OpenConection();
            SqlDataReader sqc = cn.DataReader("SELECT * FROM dbo.delict WHERE delict_id = " + currDelictID);
            while (sqc.Read())
            {
                string status = "";
                if ((int)sqc["status"] == 1)
                {
                    status = "Actief";
                }
                else if ((int)sqc["status"] == 0)
                {
                    status = "Inactief";
                }
                else if ((int)sqc["status"] == 3)
                {
                    DelictZipcodeLabel.Visibility = Visibility.Collapsed;
                    DelictHouseNumberLabel.Visibility = Visibility.Collapsed;
                    DelictStreetLabel.Visibility = Visibility.Collapsed;
                    DelictPlaceLabel.Visibility = Visibility.Collapsed;
                    Status3LB.Visibility = Visibility.Visible;
                }

                DelictPlaceLabel.Content += ": " + sqc["place"];
                DelictIDLabel.Content += ": " + sqc["delict_id"];
                DelictStreetLabel.Content += ": " + sqc["street"];
                DelictHouseNumberLabel.Content += ": " + sqc["housenumber"] + " " + sqc["housenumberAddition"];
                DelictZipcodeLabel.Content += ": " + sqc["zipcode"];
                DelictStatusLabel.Content += ": " + status;
                DelictDescriptionTB.Text = (string)sqc["description"];
                DelictDateLabel.Content += ": " + sqc["added_date"];

                longitude = (double)sqc["long"];
                latitude = (double)sqc["lat"];
                date = (DateTime)sqc["date"];
            }

            cn.CloseConnection();
            cn.OpenConection();

            SqlDataReader sqcd = cn.DataReader("SELECT category.name FROM category_delict JOIN category ON category.category_id = category_delict.category_id WHERE delict_id = " + currDelictID);

            while (sqcd.Read())
            {
                CategoryListbox.Items.Add(sqcd["name"]);
            }

            cn.CloseConnection();
            cn.OpenConection();

            SqlDataReader sqp = cn.DataReader("SELECT p.bsn, dp.type FROM delict_person dp JOIN person p on dp.person_id = p.person_id WHERE delict_id = " + currDelictID);

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

        //Backbutton method. Returnpage is for pathing.
        private void BackButton_Click(object sender, RoutedEventArgs e)
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

        //Edit delict button click. Opens the edit delict screen with the correct delict open.
        private void EditDelict_Click(object sender, RoutedEventArgs e)
        {
            mw.EditDelict(currDelictID,returnPage);
        }
    }
}
