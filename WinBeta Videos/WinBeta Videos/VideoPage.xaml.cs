using MyToolkit.Multimedia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WinBeta_Videos
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPage : Page
    {
        MainPage.Video video;
        YouTubeQuality selectedQuality = YouTubeQuality.QualityHigh;
        YouTubeUri mainVideo;
        DataTransferManager dataTransferManager;

        public VideoPage()
        {
            this.InitializeComponent();

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                    a.Handled = true;
                }
            };

            dataTransferManager = DataTransferManager.GetForCurrentView();
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var dataPackage = args.Request.Data;
            dataPackage.Properties.Title = "WinBeta Videos";
            dataPackage.Properties.Description = "Sharing Video Link";
            dataPackage.SetWebLink(new Uri("http://youtube.com/watch?v=" + video.Id));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            video = (MainPage.Video)e.Parameter;

            videosTitle.Text = video.Title;

            try
            {
                await LoadPage();
            }
            catch (AggregateException ex)
            {
                MessageDialog m = new MessageDialog("Could play video: " + ex.Message, "WinBeta Videos Error");
                await m.ShowAsync();
            }
        }

        private async Task LoadPage()
        {
            progressRing.IsActive = true;

            try
            {
                mainVideo = await MyToolkit.Multimedia.YouTube.GetVideoUriAsync(video.Id, selectedQuality);
                mediaPlayer.Source = mainVideo.Uri;
                mediaPlayer.Play();
                progressRing.IsActive = false;
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2146233088)
                {
                    MessageDialog m = new MessageDialog("Quality Not Supported, try something else", "WinBeta Videos Error");
                    await m.ShowAsync();
                } else
                {
                    MessageDialog m = new MessageDialog("Could play video: " + ex.Message, "WinBeta Videos Error");
                    await m.ShowAsync();
                }
                            
                progressRing.IsActive = false;
            }     
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var s = sender as MenuFlyoutItem;

            switch (s.Text)
            {
                case "144p":
                    selectedQuality = YouTubeQuality.Quality144P;
                    break;
                case "240p":
                    selectedQuality = YouTubeQuality.Quality240P;
                    break;
                case "270p":
                    selectedQuality = YouTubeQuality.Quality270P;
                    break;
                case "360p":
                    selectedQuality = YouTubeQuality.Quality360P;
                    break;
                case "480p":
                    selectedQuality = YouTubeQuality.Quality480P;
                    break;
                case "520p":
                    selectedQuality = YouTubeQuality.Quality520P;
                    break;
                case "720p":
                    selectedQuality = YouTubeQuality.Quality720P;
                    break;
                case "1080p":
                    selectedQuality = YouTubeQuality.Quality1080P;
                    break;
                case "4k":
                    selectedQuality = YouTubeQuality.Quality2160P;
                    break;
                default:
                    MessageDialog m = new MessageDialog("Ubnknown Quality", "WinBeta Videos Error");
                    await m.ShowAsync();
                    break;
            }

            try
            {
                await LoadPage();
            }
            catch (AggregateException ex)
            {
                MessageDialog m = new MessageDialog("Could play video: " + ex.Message, "WinBeta Videos Error");
                await m.ShowAsync();
            }

        }

        private void shareButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            dataTransferManager.DataRequested -= OnDataRequested;
            dataTransferManager.DataRequested += OnDataRequested;

            DataTransferManager.ShowShareUI();
        }
    }
}
