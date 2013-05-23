#define CODE_ANALYSIS

using System.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.Generic;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// A plugin used to allow the user 
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Correctly named architectural pattern")]
    public sealed class CaptionSelectionPlugin : IPlugin
    {
        /// <summary>
        /// Gets or sets the style to be used for the CaptionSelectionView
        /// </summary>
        public Style CaptionSelectionViewStyle { get; set; }

        CaptionSelectionView captionSelectionView;

        /// <inheritdoc /> 
        public MediaPlayer MediaPlayer { get; set; }

        /// <inheritdoc /> 
        public void Load()
        {
            MediaPlayer.CaptionSelectionInvoked += MediaPlayer_CaptionSelectionInvoked;
        }

        /// <inheritdoc /> 
        public void Update(IMediaSource mediaSource)
        {
            // nothing to do
        }

        /// <inheritdoc /> 
        public void Unload()
        {
            MediaPlayer.CaptionSelectionInvoked -= MediaPlayer_CaptionSelectionInvoked;
            OnClose();
        }

        Panel SettingsContainer
        {
            get
            {
                return MediaPlayer.Containers.OfType<Panel>().FirstOrDefault(c => c.Name == MediaPlayerTemplateParts.SettingsContainer);
            }
        }

        InteractionType deactivationMode;
        void MediaPlayer_CaptionSelectionInvoked(object sender, object e)
        {
            if (MediaPlayer.AvailableCaptions.Any())
            {
                captionSelectionView = new CaptionSelectionView();
                if (CaptionSelectionViewStyle != null) captionSelectionView.Style = CaptionSelectionViewStyle;
                captionSelectionView.SetBinding(FrameworkElement.DataContextProperty, new Binding() { Path = new PropertyPath("InteractiveViewModel"), Source = MediaPlayer });
                SettingsContainer.Visibility = Visibility.Visible;
                SettingsContainer.Children.Add(captionSelectionView);
                captionSelectionView.Close += captionSelectionView_Close;
                deactivationMode = MediaPlayer.InteractiveDeactivationMode;
                MediaPlayer.InteractiveDeactivationMode = InteractionType.None;
            }
        }

        void captionSelectionView_Close(object sender, object e)
        {
            OnClose();
        }

        private void OnClose()
        {
            if (captionSelectionView != null)
            {
                captionSelectionView.Close -= captionSelectionView_Close;
                captionSelectionView.Visibility = Visibility.Collapsed;
                SettingsContainer.Children.Remove(captionSelectionView);
                SettingsContainer.Visibility = Visibility.Collapsed;
                MediaPlayer.InteractiveDeactivationMode = deactivationMode;
                captionSelectionView = null;
            }
        }
    }
}
