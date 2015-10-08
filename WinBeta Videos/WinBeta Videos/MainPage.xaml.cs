using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace WinBeta_Videos
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        // The apps accent colour (WinBetas theme)
        Color AppAccent = new Color()
        {
            R = 0,
            G = 120,
            B = 215
        };

        Color AppAccentHover = new Color()
        {
            R = 0,
            G = 131,
            B = 235
        };

        YouTubeService youtubeService; // Used for accessing API
        string WinBetaChannelId = "UC70UzaroFf5GcyecHOGw-tw"; // WinBeta Channel ID
        ObservableCollection<Video> videos_data; // Holds the videos

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += OnPageLoaded;

            // Get the Application View
            var applicationView = ApplicationView.GetForCurrentView();

            // Set the Titlebar Colours
            applicationView.TitleBar.BackgroundColor = AppAccent;
            applicationView.TitleBar.ButtonBackgroundColor = AppAccent;
            applicationView.TitleBar.ButtonHoverBackgroundColor = AppAccentHover;
            applicationView.TitleBar.ButtonHoverForegroundColor = Colors.White;
            applicationView.TitleBar.ForegroundColor = Colors.White;
            applicationView.TitleBar.ButtonForegroundColor = Colors.White;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // Initialize the YouTube Service
            youtubeService = new YouTubeService(new BaseClientService.Initializer() {
                ApiKey = Helper.GetApiKey(), // Hidden Away (API_KEY)
                ApplicationName = "WinBeta Videos"
            });

            try
            {
                await GetVideos();
            }
            catch (Exception ex)
            {
                // TODO Handle
            }
        }

        private async void refreshButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                await GetVideos();
            }
            catch (Exception ex)
            {
                // TODO Handle
            }
        }

        private async Task GetVideos()
        {
            // Show Progress Ring
            progressRing.IsActive = true;

            // Reset DataContext and Lists
            videos_data = null;
            videos_data = new ObservableCollection<Video>();
            DataContext = videos_data;

            // Send a request to the YouTube API to search for channels
            var searchChannelRequest = youtubeService.Channels.List("contentDetails");
            searchChannelRequest.Id = WinBetaChannelId; // The WinBeta Channel ID
            searchChannelRequest.MaxResults = 1; // Only get one result (Only can be one channel with the same channel ID)

            // API response
            var searchChannelResponse = await searchChannelRequest.ExecuteAsync();
            var channel = searchChannelResponse.Items.First(); // Choose the first (and only) item

            // Send a request to the YouTube API to search for playlists on youtube channel 
            // (for now only grab upload playlist, later on will grab all playlists and let user filter)
            var playlistRequest = youtubeService.PlaylistItems.List("snippet");
            playlistRequest.PlaylistId = channel.ContentDetails.RelatedPlaylists.Uploads; // Get the uploads playlist
            playlistRequest.MaxResults = 50; // Max of 50 results

            // API response
            var playlistResponse = await playlistRequest.ExecuteAsync();

            // Loop through all items in upload playlist
            foreach (var playlistItem in playlistResponse.Items)
            {
                // Create a video object to pass into the data context
                Video video = new Video() {
                    Title = playlistItem.Snippet.Title, // Video Title
                    Thumbnail = GetVideoThumbnail(playlistItem), // Video thumbnail <- This function gets the highest quality avaliable
                    Date = ConvertVideoDateTime(playlistItem.Snippet.PublishedAt) // Get the published date (formated and converted to correct time zone)
                };

                // Add video to data context list
                videos_data.Add(video);
            }

            // Hide Progress Ring
            progressRing.IsActive = false;
        }

        private string GetVideoThumbnail(PlaylistItem playlistItem)
        {
            // Check if max res image is avaliable (not all videos have them)
            if (playlistItem.Snippet.Thumbnails.Maxres != null)
                return playlistItem.Snippet.Thumbnails.Maxres.Url; // Max Res
            else // If not get next best thing
                return playlistItem.Snippet.Thumbnails.Medium.Url; // Medium Res
        }

        private string ConvertVideoDateTime(DateTime? dt)
        {
            // Check if DateTime is not Null
            if (dt != null)
            {
                // Convert to local timezone
                DateTime date = dt.Value.ToLocalTime();

                // Format the Date section
                var startDate = date.DayOfWeek + " " + date.Day + " " + GetMonthFromString(date.Month.ToString());

                // Total Minutes
                var minutes = date.TimeOfDay.Minutes.ToString();

                // Add the zero infront of the minutes
                if (minutes.Length == 1) { minutes = "0" + minutes; }

                // Basic time
                var time = date.TimeOfDay.Hours + ":" + minutes + " AM";

                // Convert to 12 hour time
                if (date.TimeOfDay.Hours >= 13)
                {
                    time = (date.TimeOfDay.Hours - 12) + ":" + minutes + " PM";
                }

                // Return nice formated date
                return "Uploaded: " + startDate + " at " + time;
            }
            else
            {
                return "No Upload Date";
            }          
        }

        // Basic function, converts month number to name
        public string GetMonthFromString(string month)
        {
            switch (month)
            {
                case "1":
                    month = "Janurary";
                    break;
                case "2":
                    month = "Feburary";
                    break;
                case "3":
                    month = "March";
                    break;
                case "4":
                    month = "April";
                    break;
                case "5":
                    month = "May";
                    break;
                case "6":
                    month = "June";
                    break;
                case "7":
                    month = "July";
                    break;
                case "8":
                    month = "August";
                    break;
                case "9":
                    month = "September";
                    break;
                case "10":
                    month = "October";
                    break;
                case "11":
                    month = "November";
                    break;
                case "12":
                    month = "December";
                    break;
            }
            return month;
        }


        public class Video
        {
            public String Title { get; set; }
            public String VideoId { get; set; }
            public String Thumbnail { get; set; }
            public String Date { get; set; }
            public String ViewCount { get; set; }
        }

        
    }
}
