using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.PlayerFramework.Samples.Common;
using Microsoft.PlayerFramework.Adaptive;
using Windows.Storage.Pickers;
using Microsoft.PlayerFramework.TimedText;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Microsoft.PlayerFramework.Samples
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CffOfflinePage : Page
    {
        private NavigationHelper navigationHelper;

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        AdaptivePlugin adaptivePlugin;

        public CffOfflinePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            adaptivePlugin = new AdaptivePlugin();
            player.Plugins.Add(adaptivePlugin);

            adaptivePlugin.InstreamCaptionsEnabled = true;
            var ttmlPlugin = new CaptionsPlugin();
            player.Plugins.Add(ttmlPlugin);
            OpenFile();
        }
        
        private async void OpenFile()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            openPicker.FileTypeFilter.Add(".uvu");
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                adaptivePlugin.DownloaderPlugin = new Microsoft.AdaptiveStreaming.Dash.CffOfflineDownloaderPlugin(file);
                player.Source = new Uri(string.Format("ms-sstr://local/{0}", file.Name)); // create a dummy url, this can actually be anything.
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            backButton.Command = this.navigationHelper.GoBackCommand;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            player.Dispose();
            base.OnNavigatedFrom(e);
        }
    }
}