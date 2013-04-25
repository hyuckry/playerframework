using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.PlayerFramework.Captions
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(IPlugin))]
    public partial class CaptionsPlugin : IPlugin, INotifyPropertyChanged
    {
        CaptionsSelectionPanel captionsSelectionPanel;
        CaptionsPanel captionsPanel;
        Panel captionsContainer;
        Panel settingsContainer;
        CaptionManager captionManager = new CaptionManager();
        ObservableCollection<CaptionMarker> activeCaptions;

        public IList<CaptionMarker> CaptionMarkers
        {
            get { return captionManager.Markers as IList<CaptionMarker>; }
            set { captionManager.Markers = value; }
        }

        public bool IsCaptionsSettingsVisible { get; private set; }

        public bool IsCaptionsVisible { get; private set; }

        bool isEnabled;
        /// <summary>
        /// Gets or sets whether or not captions are enabled
        /// </summary>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                if (PropertyChanged != null)
                {
                    OnPropertyChanged(() => IsEnabled);
                }
                MediaPlayer.IsCaptionsVisible = isEnabled;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void IPlugin.Load()
        {
            LoadCaptions();

            MediaPlayer.IsCaptionsActiveChanged += MediaPlayer_IsCaptionsActiveChanged;
            ((IPlugin)this).Update(MediaPlayer);
        }

        void IPlugin.Update(IMediaSource mediaSource)
        {
            // load config
            MediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
            IsEnabled = GetIsEnabled((DependencyObject)mediaSource);
            if (GetAutoLoadMarkers((DependencyObject)mediaSource))
            {
                CaptionMarkers = null;
                MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            }
            else
            {
                CaptionMarkers = GetCaptions((DependencyObject)mediaSource);
            }
        }

        void IPlugin.Unload()
        {
            MediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
            UnloadCaptions();
            MediaPlayer.IsCaptionsActiveChanged -= MediaPlayer_IsCaptionsActiveChanged;
        }

        public MediaPlayer MediaPlayer { get; set; }

        void LoadCaptions()
        {
            settingsContainer = MediaPlayer.Containers.OfType<Panel>().FirstOrDefault(e => e.Name == MediaPlayerTemplateParts.SettingsContainer);
            if (settingsContainer != null)
            {
                captionsSelectionPanel = new CaptionsSelectionPanel() { Visibility = Visibility.Collapsed };
                captionsSelectionPanel.Closed += captionsSelectionPanel_Closed;
                captionsSelectionPanel.Selected += captionsSelectionPanel_Selected;
                settingsContainer.Children.Add(captionsSelectionPanel);
                IsCaptionsSettingsVisible = true;
            }

            captionsContainer = MediaPlayer.Containers.OfType<Panel>().FirstOrDefault(e => e.Name == MediaPlayerTemplateParts.CaptionsContainer);
            if (captionsContainer != null)
            {
                captionsPanel = new CaptionsPanel();

                activeCaptions = new ObservableCollection<CaptionMarker>();
                captionsPanel.ActiveCaptions = activeCaptions;
                captionsContainer.Children.Add(captionsPanel);
                IsCaptionsVisible = true;
                MediaPlayer.PositionChanged += MediaPlayer_PositionChanged;

                captionManager.ShowCaption += captionManager_ShowCaption;
                captionManager.HideCaption += captionManager_HideCaption;
            }
        }

        void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            MediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
            CaptionMarkers = CaptionMarkerConverter.Convert(MediaPlayer.Markers);
        }

        void MediaPlayer_PositionChanged(object sender, RoutedPropertyChangedEventArgs<TimeSpan> e)
        {
            captionManager.Process(e.NewValue, MediaPlayer.IsScrubbing);
        }

        void captionsSelectionPanel_Selected(object sender, EventArgs e)
        {
            captionsSelectionPanel.Visibility = Visibility.Collapsed;
        }

        void captionsSelectionPanel_Closed(object sender, System.EventArgs e)
        {
            MediaPlayer.IsCaptionsActive = false;
            captionsSelectionPanel.Visibility = Visibility.Collapsed;
        }

        void UnloadCaptions()
        {
            if (IsCaptionsSettingsVisible)
            {
                settingsContainer.Children.Remove(captionsSelectionPanel);
                settingsContainer = null;
                captionsSelectionPanel = null;
                IsCaptionsSettingsVisible = false;

                MediaPlayer.PositionChanged -= MediaPlayer_PositionChanged;
                captionManager.ShowCaption -= captionManager_ShowCaption;
                captionManager.HideCaption -= captionManager_HideCaption;
            }

            if (IsCaptionsVisible)
            {
                captionsContainer.Children.Remove(captionsPanel);
                captionsContainer = null;
                captionsPanel = null;
                IsCaptionsVisible = false;
                activeCaptions = null;
            }
        }

        protected void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            OnPropertyChanged(GetPropertyName(property));
        }

        static string GetPropertyName<T>(Expression<Func<T>> property)
        {
            return (property.Body as MemberExpression).Member.Name;
        }

        void MediaPlayer_IsCaptionsActiveChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            captionsSelectionPanel.Visibility = e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        void captionManager_HideCaption(object sender, CaptionCueEventArgs e)
        {
            activeCaptions.Remove(e.Caption);
        }

        void captionManager_ShowCaption(object sender, CaptionCueEventArgs e)
        {
            activeCaptions.Add(e.Caption);
        }
    }
}
