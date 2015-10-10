using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//Google APIs
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

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

        Color AppNotFocused = new Color()
        {
            R = 230,
            G = 230,
            B = 230
        };

        YouTubeService youtubeService; // Used for accessing API
        string WinBetaChannelId = "UC70UzaroFf5GcyecHOGw-tw"; // WinBeta Channel ID
        ObservableCollection<Video> videos_data; // Holds the videos

        Playlist selectedPlaylist = null;

        string mainVideoPageToken = null;

        public MainPage()
        {
            this.InitializeComponent();
           // Loaded += OnPageLoaded;

            // Get the Application View
            var applicationView = ApplicationView.GetForCurrentView();

            // Set the Titlebar Colours
            applicationView.TitleBar.BackgroundColor = AppAccent;
            applicationView.TitleBar.ButtonBackgroundColor = AppAccent;
            applicationView.TitleBar.ButtonHoverBackgroundColor = AppAccentHover;
            applicationView.TitleBar.ButtonHoverForegroundColor = Colors.White;
            applicationView.TitleBar.ForegroundColor = Colors.White;
            applicationView.TitleBar.ButtonForegroundColor = Colors.White;

            applicationView.TitleBar.InactiveBackgroundColor = AppNotFocused;
            applicationView.TitleBar.ButtonInactiveBackgroundColor = AppNotFocused;

            applicationView.TitleBar.ButtonInactiveForegroundColor = Colors.DarkGray;
            applicationView.TitleBar.InactiveForegroundColor = Colors.DarkGray;

            applicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private void OnScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var verticalOffset = sv.VerticalOffset;
            var maxVerticalOffset = sv.ScrollableHeight;

            if (maxVerticalOffset < 0 || verticalOffset == maxVerticalOffset)
            {
                VideoGetNextPage();
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back) return;

            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Helper.GetApiKey(), // Hidden Away (API_KEY)
                ApplicationName = "WinBeta Videos"
            });

            await RunGetVideos();

        }

      //  private async void OnPageLoaded(object sender, RoutedEventArgs e)
        //{

            // Initialize the YouTube Service
           // youtubeService = new YouTubeService(new BaseClientService.Initializer() {
           //     ApiKey = Helper.GetApiKey(), // Hidden Away (API_KEY)
           //     ApplicationName = "WinBeta Videos"
           // });

           // await RunGetVideos();



       // }

        private async void refreshButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await RunGetVideos();
        }

        private async void clearFilter_Click(object sender, RoutedEventArgs e)
        {
            selectedPlaylist = null;
            videosTitle.Text = "Videos";

            await RunGetVideos();

        }

        private async void FilterVideos(Playlist p)
        {
            selectedPlaylist = p;
            videosTitle.Text = "Videos - " + p.Title;

            await RunGetVideos();
        }

        private async Task RunGetVideos()
        {
            try
            {
                // Reset DataContext and Lists
                mainVideoPageToken = null;             
                videos_data = null;
                videos_data = new ObservableCollection<Video>();
                DataContext = videos_data;

                await GetVideos();
            }
            catch (Exception ex)
            {
                MessageDialog m = new MessageDialog("Could not load videos: " + ex.Message, "WinBeta Videos Error");
                await m.ShowAsync();
            }
        }

        private async void VideoGetNextPage()
        {
            try
            {
                await GetVideos();
            }
            catch (Exception ex)
            {
                MessageDialog m = new MessageDialog("Could not load videos: " + ex.Message, "WinBeta Videos Error");
                await m.ShowAsync();
            }
        }

        private async Task GetVideos()
        {
            if (mainVideoPageToken == "eol")
            {
                return;
            }

            // Show Progress Ring
            progressRing.IsActive = true;

           

            // Send a request to the YouTube API to search for channels
            var searchChannelRequest = youtubeService.Channels.List("contentDetails");
            searchChannelRequest.Id = WinBetaChannelId; // The WinBeta Channel ID
            searchChannelRequest.MaxResults = 1; // Only get one result (Only can be one channel with the same channel ID)

            // API response
            var searchChannelResponse = await searchChannelRequest.ExecuteAsync();
            var channel = searchChannelResponse.Items.First(); // Choose the first (and only) item

            // Check if there are any items in the flyout (by default threes 2, the clear button, and seperator)
            if (filterFlyout.Items.Count == 2)
            {
                // Playlist Request
                var allPlaylistsRequest = youtubeService.Playlists.List("snippet, id");
                allPlaylistsRequest.ChannelId = WinBetaChannelId;
                allPlaylistsRequest.MaxResults = 50; // Max of 50 results

                // Playlist Response
                var allPlaylistsResponse = await allPlaylistsRequest.ExecuteAsync();

                //  Loop through all the playlists
                foreach (var playlistItem in allPlaylistsResponse.Items)
                {
                    // Create a playlist object
                    Playlist playlist = new Playlist()
                    {
                        Title = playlistItem.Snippet.Title,
                        ID = playlistItem.Id
                    };

                    // Command (To be used in the flyout items)
                    var MyCommand = new DelegateCommand<Playlist>(FilterVideos);

                    // Add a new Flyout item
                    filterFlyout.Items.Add(new MenuFlyoutItem()
                    {
                        Text = playlist.Title, // Playlist Title
                        Command = MyCommand, // Command to run
                        CommandParameter = playlist // Command Parameter
                    });
                }        
            }

            // If there is no selected playlist
            if (selectedPlaylist == null)
            {
                // Create default uploads playlist
                selectedPlaylist = new Playlist()
                {
                    Title = "All Uploads",
                    ID = channel.ContentDetails.RelatedPlaylists.Uploads // Uploads ID
                };
            }

            // Send a request to the YouTube API to search for playlists on youtube channel 
            // (for now only grab upload playlist, later on will grab all playlists and let user filter)
            var playlistRequest = youtubeService.PlaylistItems.List("snippet, contentDetails");
            playlistRequest.PlaylistId = selectedPlaylist.ID; // Get the uploads playlist
            playlistRequest.MaxResults = 10; // Max of 10 results

           
            if (mainVideoPageToken != null)
            {
                playlistRequest.PageToken = mainVideoPageToken;
            }

             // API response
             var playlistResponse = await playlistRequest.ExecuteAsync();


            if (playlistResponse.NextPageToken != null)
                mainVideoPageToken = playlistResponse.NextPageToken;
            else
                mainVideoPageToken = "eol";

            // Loop through all items in upload playlist
            foreach (var playlistItem in playlistResponse.Items)
            {
                // Create a video object to pass into the data context
                videos_data.Add (new Video() {
                    Title = playlistItem.Snippet.Title, // Video Title
                    Thumbnail = GetVideoThumbnail(playlistItem.Snippet.Thumbnails), // Video thumbnail <- This function gets the highest quality avaliable
                    Date = ConvertVideoDateTime(playlistItem.Snippet.PublishedAt), // Get the published date (formated and converted to correct time zone)
                    Id = playlistItem.ContentDetails.VideoId
                });
            }

            // Hide Progress Ring
            progressRing.IsActive = false;
        }

        private async void mainSearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            // Show Progress Ring
            progressRing.IsActive = true;

            // Reset DataContext and Lists
            videos_data = null;
            videos_data = new ObservableCollection<Video>();
            DataContext = videos_data;

            videosTitle.Text = "Videos - " + mainSearchBox.QueryText;

            var searchChannelVideosRequest = youtubeService.Search.List("snippet");
            searchChannelVideosRequest.Q = mainSearchBox.QueryText;
            searchChannelVideosRequest.ChannelId = WinBetaChannelId;
            searchChannelVideosRequest.MaxResults = 25;

            var searchChannelVideosResponse = await searchChannelVideosRequest.ExecuteAsync();

            foreach (var videoItem in searchChannelVideosResponse.Items)
            {
                if (videoItem.Id.Kind == "youtube#video")
                {
                    // Create a video object to pass into the data context
                    Video video = new Video()
                    {
                        Title = videoItem.Snippet.Title, // Video Title
                        Thumbnail = GetVideoThumbnail(videoItem.Snippet.Thumbnails), // Video thumbnail <- This function gets the highest quality avaliable
                        Date = ConvertVideoDateTime(videoItem.Snippet.PublishedAt), // Get the published date (formated and converted to correct time zone)
                        Id = videoItem.Id.VideoId
                    };

                    // Add video to data context list
                    videos_data.Add(video);
                }
            }

            // Hide Progress Ring
            progressRing.IsActive = false;
        }

        private string GetVideoThumbnail(ThumbnailDetails thumb)
        {
            // Check if max res image is avaliable (not all videos have them)
            if (thumb.Maxres != null)
                return thumb.Maxres.Url; // Max Res
            else // If not get next best thing
                return thumb.Medium.Url; // Medium Res
        }

        private void videoBox_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Video r = (Video)((FrameworkElement)e.OriginalSource).DataContext;
            this.Frame.Navigate(typeof(VideoPage), r);
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
            public String Id { get; set; }
            public String Thumbnail { get; set; }
            public String Date { get; set; }
        }

        public class Playlist
        {
            public String Title { get; set; }
            public String ID { get; set; }
        }
    }
}
