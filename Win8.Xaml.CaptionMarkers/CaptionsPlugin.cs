using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#endif

namespace Microsoft.PlayerFramework.CaptionMarkers
{
    /// <summary>
    /// Represents a plugin for the player framework that can show closed captions
    /// </summary>
    public sealed class CaptionsPlugin : IPlugin
    {
        const string DefaultMarkerType = "caption";

        CaptionsPanel captionsPanel;
        Panel captionsContainer;
        CancellationTokenSource cts;

        /// <summary>
        /// Creates a new instance of CaptionsPanel
        /// </summary>
        public CaptionsPlugin()
        {
            CaptionDuration = TimeSpan.FromSeconds(2);
            MarkerType = DefaultMarkerType;
        }

        /// <inheritdoc /> 
        public MediaPlayer MediaPlayer { get; set; }

        /// <inheritdoc /> 
        public void Load()
        {
            var mediaContainer = MediaPlayer.Containers.OfType<Panel>().FirstOrDefault(c => c.Name == MediaPlayerTemplateParts.MediaContainer);
            captionsContainer = mediaContainer.Children.OfType<Panel>().FirstOrDefault(c => c.Name == MediaPlayerTemplateParts.CaptionsContainer);
            if (captionsContainer != null)
            {
                if (!MediaPlayer.AvailableCaptions.Any()) // if there are caption tracks specified, this means there is another plugin that will handle this work. Do nothing.
                {
                    captionsPanel = new CaptionsPanel();
                    captionsPanel.Style = CaptionsPanelStyle;
                    ActiveCaptions = new ObservableCollection<ActiveCaption>();
                    captionsPanel.ActiveCaptions = ActiveCaptions;
                    captionsContainer.Children.Add(captionsPanel);
                    cts = new CancellationTokenSource();
                    MediaPlayer.MarkerReached += MediaPlayer_MarkerReached;
                    MediaPlayer.CaptionsInvoked += MediaPlayer_CaptionsInvoked;
                }
            }
        }

        /// <inheritdoc /> 
        public void Update(IMediaSource mediaSource)
        {
            ActiveCaptions.Clear();
        }

        /// <inheritdoc /> 
        public void Unload()
        {
            MediaPlayer.MarkerReached -= MediaPlayer_MarkerReached;
            MediaPlayer.CaptionsInvoked -= MediaPlayer_CaptionsInvoked;
            cts.Cancel();
            cts = null;
            captionsContainer.Children.Remove(captionsPanel);
            captionsContainer = null;
            captionsPanel.ActiveCaptions = null;
            captionsPanel = null;
            ActiveCaptions = null;
        }

        /// <summary>
        /// Gets or sets whether or not captions are enabled
        /// </summary>
        public string MarkerType { get; set; }

        /// <summary>
        /// Gets or sets whether or not captions are enabled
        /// </summary>
        public TimeSpan CaptionDuration { get; set; }

        /// <summary>
        /// Gets or sets the style to be used for the CaptionsPanel
        /// </summary>
        public Style CaptionsPanelStyle { get; set; }

        /// <summary>
        /// Gets the list of active captions
        /// </summary>
        protected ObservableCollection<ActiveCaption> ActiveCaptions { get; private set; }
        
        void MediaPlayer_CaptionsInvoked(object sender, object e)
        {
            MediaPlayer.IsCaptionsActive = !MediaPlayer.IsCaptionsActive;
        }

        async void MediaPlayer_MarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
        {
            if (MediaPlayer.IsCaptionsActive)
            {
                if (MarkerType == null || e.Marker.Type == MarkerType)
                {
                    var activeCaption = new ActiveCaption() { Text = e.Marker.Text };
                    ActiveCaptions.Add(activeCaption);
                    try
                    {
#if SILVERLIGHT
                        await TaskEx.Delay(CaptionDuration, cts.Token);
#else
                        await Task.Delay(CaptionDuration, cts.Token);
#endif
                    }
                    catch (OperationCanceledException) { /* ignore */ }
                    finally
                    {
                        if (ActiveCaptions != null)  // we could have been unloaded while we waited.
                        {
                            ActiveCaptions.Remove(activeCaption);
                        }
                    }
                }
            }
        }
    }
}
