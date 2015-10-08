using Google.Apis.Services;
using Google.Apis.YouTube.v3;
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

        YouTubeService youtubeService;
        string WinBetaChannelId = "UC70UzaroFf5GcyecHOGw-tw";
        ObservableCollection<Video> videos_data;

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
                ApiKey = Helper.Services.GetApiKey(),
                ApplicationName = "WinBeta Videos"
            });

            try
            {
                await GetVideos();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task GetVideos()
        {
            videos_data = null;
            videos_data = new ObservableCollection<Video>();
            DataContext = videos_data;

            var searchChannelRequest = youtubeService.Channels.List("contentDetails");
            searchChannelRequest.Id = WinBetaChannelId;
            searchChannelRequest.MaxResults = 1;

            var searchChannelResponse = await searchChannelRequest.ExecuteAsync();
            var channel = searchChannelResponse.Items.First();

            var channelUploadID = channel.ContentDetails.RelatedPlaylists.Uploads;

            var playlistRequest = youtubeService.PlaylistItems.List("snippet");
            playlistRequest.PlaylistId = channelUploadID;
            playlistRequest.MaxResults = 50;

            var playlistResponse = await playlistRequest.ExecuteAsync();

            foreach (var playlistItem in playlistResponse.Items)
            {
                Video video = new Video() {
                    Title = playlistItem.Snippet.Title,
                    Thumbnail = playlistItem.Snippet.Thumbnails.High.Url
                };

                videos_data.Add(video);
            }
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
