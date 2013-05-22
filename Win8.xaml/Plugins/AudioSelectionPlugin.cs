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
    public sealed class AudioSelectionPlugin : IPlugin
    {
        InteractionType deactivationMode;
        private AudioSelectionView audioSelectionView;

        /// <summary>
        /// Gets or sets the style to be used for the CaptionSelectorView
        /// </summary>
        public Style AudioSelectionViewStyle { get; set; }

        /// <inheritdoc /> 
        public MediaPlayer MediaPlayer { get; set; }

        /// <inheritdoc /> 
        public void Load()
        {
            MediaPlayer.AudioSelectionInvoked += MediaPlayer_AudioSelectionInvoked;
        }

        /// <inheritdoc /> 
        public void Update(IMediaSource mediaSource)
        {
            // nothing to do
        }

        /// <inheritdoc /> 
        public void Unload()
        {
            MediaPlayer.AudioSelectionInvoked -= MediaPlayer_AudioSelectionInvoked;
            OnClose();
        }

        Panel SettingsContainer
        {
            get
            {
                return MediaPlayer.Containers.OfType<Panel>().FirstOrDefault(c => c.Name == MediaPlayerTemplateParts.SettingsContainer);
            }
        }

        void MediaPlayer_AudioSelectionInvoked(object sender, AudioSelectionInvokedEventArgs e)
        {
            if (MediaPlayer.AvailableAudioStreams.Any())
            {
                audioSelectionView = new AudioSelectionView();
                if (AudioSelectionViewStyle != null) audioSelectionView.Style = AudioSelectionViewStyle;
                audioSelectionView.SetBinding(FrameworkElement.DataContextProperty, new Binding() { Path = new PropertyPath("InteractiveViewModel"), Source = MediaPlayer });
                SettingsContainer.Visibility = Visibility.Visible;
                SettingsContainer.Children.Add(audioSelectionView);
                audioSelectionView.Close += SelectorView_Close;
                deactivationMode = MediaPlayer.InteractiveDeactivationMode;
                MediaPlayer.InteractiveDeactivationMode = InteractionType.None;
            }
        }

        void SelectorView_Close(object sender, object e)
        {
            OnClose();
        }

        private void OnClose()
        {
            if (audioSelectionView != null)
            {
                audioSelectionView.Close -= SelectorView_Close;
                audioSelectionView.Visibility = Visibility.Collapsed;
                SettingsContainer.Children.Remove(audioSelectionView);
                SettingsContainer.Visibility = Visibility.Collapsed;
                MediaPlayer.InteractiveDeactivationMode = deactivationMode;
                audioSelectionView = null;
            }
        }
    }
}
