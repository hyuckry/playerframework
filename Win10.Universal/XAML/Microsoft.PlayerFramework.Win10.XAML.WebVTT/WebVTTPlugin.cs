﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.Media.WebVTT;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.Foundation;

namespace Microsoft.PlayerFramework.WebVTT
{
    /// <summary>
    /// A player framework plugin capable of displaying timed text captions.
    /// </summary>
    public partial class WebVTTPlugin : PluginBase
    {
        WebVTTPanel captionsPanel;
        Panel captionsContainer;
        DispatcherTimer timer;
        MediaMarkerManager markerManager;

        /// <summary>
        /// Creates a new instance of the CaptionsPlugin
        /// </summary>
        public WebVTTPlugin()
        {
            PollingInterval = TimeSpan.FromSeconds(10);
        }

        /// <summary>
        /// Gets or sets the amount of time to check the server for updated data. Only applies when MediaPlayer.IsLive = true
        /// </summary>
        public TimeSpan PollingInterval { get; set; }

        /// <summary>
        /// Gets whether the captions panel is visible. Returns true if any captions were found.
        /// </summary>
        public bool IsSourceLoaded { get; private set; }

        /// <summary>
        /// Gets the Panel control used to display captions. This can be used to modify the default styles use on captions.
        /// </summary>
        public WebVTTPanel CaptionsPanel { get { return captionsPanel; } }

        /// <summary>
        /// Raised when a parsing error occurs.
        /// </summary>
        public event EventHandler<ParseFailedEventArgs> ParseFailed;

        /// <summary>
        /// Gets or sets the style to be used for the TimedTextCaptions
        /// </summary>
        public Style CaptionsPanelStyle { get; set; }

        void MediaPlayer_SelectedCaptionChanged(object sender, RoutedPropertyChangedEventArgs<PlayerFramework.Caption> e)
        {
            if (e.OldValue != null)
            {
                e.OldValue.PayloadChanged -= caption_PayloadChanged;
                e.OldValue.PayloadAugmented -= caption_PayloadAugmented;
            }
            MediaPlayer.IsCaptionsActive = (e.NewValue as Caption != null);
            UpdateCaption(e.NewValue as Caption);
            if (e.NewValue != null)
            {
                e.NewValue.PayloadChanged += caption_PayloadChanged;
                e.NewValue.PayloadAugmented += caption_PayloadAugmented;
            }

            if (captionsPanel != null)
                captionsPanel.ResetStyle();
        }

        void MediaPlayer_PositionChanged(object sender, RoutedPropertyChangedEventArgs<TimeSpan> e)
        {
            if (MediaPlayer.SelectedCaption != null)
            {
                captionsPanel.UpdatePosition(MediaPlayer.Position);
                markerManager.CheckMarkerPositions(MediaPlayer.Position);
            }
        }

        void MediaPlayer_IsLiveChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (MediaPlayer.IsLive)
            {
                InitializeTimer();
            }
            else
            {
                ShutdownTimer();
            }
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = PollingInterval;
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void ShutdownTimer()
        {
            if (timer != null)
            {
                timer.Tick -= timer_Tick;
                if (timer.IsEnabled) timer.Stop();
                timer = null;
            }
        }

        void timer_Tick(object sender, object e)
        {
            var caption = MediaPlayer.SelectedCaption as Caption;
            RefreshCaption(caption);
        }

        /// <inheritdoc /> 
        protected override bool OnActivate()
        {
            var mediaContainer = MediaPlayer.Containers.OfType<Panel>().FirstOrDefault(c => c.Name == MediaPlayerTemplateParts.MediaContainer);
            captionsContainer = mediaContainer.Children.OfType<Panel>().FirstOrDefault(c => c.Name == MediaPlayerTemplateParts.CaptionsContainer);
            if (captionsContainer != null)
            {
                markerManager = new MediaMarkerManager();
                markerManager.MarkerLeft += MarkerManager_MarkerLeft;
                markerManager.MarkerReached += MarkerManager_MarkerReached;
                captionsPanel = new WebVTTPanel();
                if (CaptionsPanelStyle != null) captionsPanel.Style = CaptionsPanelStyle;
                MediaPlayer.IsCaptionsActive = (MediaPlayer.SelectedCaption as Caption != null);
                captionsContainer.Children.Add(captionsPanel);
                UpdateCaption(MediaPlayer.SelectedCaption as Caption);

                MediaPlayer.PositionChanged += MediaPlayer_PositionChanged;
                MediaPlayer.SelectedCaptionChanged += MediaPlayer_SelectedCaptionChanged;
                MediaPlayer.IsLiveChanged += MediaPlayer_IsLiveChanged;
                if (MediaPlayer.IsLive) InitializeTimer();

                return true;
            }
            return false;
        }

        /// <inheritdoc /> 
        protected override void OnDeactivate()
        {
            MediaPlayer.PositionChanged -= MediaPlayer_PositionChanged;
            MediaPlayer.SelectedCaptionChanged -= MediaPlayer_SelectedCaptionChanged;
            MediaPlayer.IsLiveChanged -= MediaPlayer_IsLiveChanged;
            MediaPlayer.IsCaptionsActive = false;
            captionsContainer.Children.Remove(captionsPanel);
            captionsContainer = null;
            captionsPanel.Clear();
            captionsPanel = null;
            markerManager.MarkerLeft -= MarkerManager_MarkerLeft;
            markerManager.MarkerReached -= MarkerManager_MarkerReached;
            markerManager.Clear();
            markerManager = null;
            IsSourceLoaded = false;
        }

        void MarkerManager_MarkerLeft(object sender, MediaMarkerEventArgs e)
        {
            HideCue((WebVTTCue)e.Marker.Content);
        }

        void MarkerManager_MarkerReached(object sender, MediaMarkerEventArgs e)
        {
            ShowCue((WebVTTCue)e.Marker.Content);
        }

        void ShowCue(WebVTTCue cue)
        {
            captionsPanel.ActiveCues.Add(cue);
        }

        void HideCue(WebVTTCue cue)
        {
            captionsPanel.ActiveCues.Remove(cue);
        }

        /// <summary>
        /// Updates the current caption track.
        /// Will cause the caption source to download and get parsed, and will will start playing.
        /// </summary>
        /// <param name="caption">The caption track to use.</param>
        public void UpdateCaption(Caption caption)
        {
            markerManager.Clear();
            captionsPanel.Clear();
            RefreshCaption(caption);
        }

        void caption_PayloadChanged(object sender, EventArgs e)
        {
            RefreshCaption(sender as Caption);
        }

        void caption_PayloadAugmented(object sender, PayloadAugmentedEventArgs e)
        {
            AugmentCaption(sender as Caption, e.Payload, e.StartTime, e.EndTime);
        }

        private async void AugmentCaption(Caption caption, object payload, TimeSpan startTime, TimeSpan endTime)
        {
            if (caption != null)
            {
                string result = null;
                if (payload is byte[])
                {
                    var byteArray = (byte[])payload;
                    result = await Task.Run(() => System.Text.Encoding.UTF8.GetString(byteArray, 0, byteArray.Length));
                }
                else if (payload is string)
                {
                    result = (string)payload;
                }
                if (result != null)
                {
                    allTasks = EnqueueTask(() => AugmentWebVTT(result, startTime, endTime), allTasks);
                    await allTasks;
                }
            }
        }

        private async Task<WebVTTDocument> LoadWebVTTDocument(string webvtt, TimeSpan startTime, TimeSpan endTime)
        {
            // parse on a background thread
            return await Task.Run(() => WebVTTParser.ParseDocument(webvtt, startTime, endTime));
        }

        private async Task AugmentWebVTT(string webvtt, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                var doc = await LoadWebVTTDocument(webvtt, startTime, endTime);
                // merge
                foreach (var cue in doc.Cues)
                {
                    bool found = false;
                    foreach (var marker in markerManager.MediaMarkers)
                    {
                        if (marker.Begin == cue.Begin && marker.End == cue.End) // assume its the same one if begin and end match.
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        markerManager.MediaMarkers.Add(new MediaMarker() { Begin = cue.Begin, End = cue.End, Content = cue });
                    }
                }
            }
            catch (Exception ex)
            {
                if (ParseFailed != null) ParseFailed(this, new ParseFailedEventArgs(ex));
            }
        }

        private async Task LoadWebVTT(string webvtt)
        {
            try
            {
                var doc = await LoadWebVTTDocument(webvtt, TimeSpan.Zero, TimeSpan.MaxValue);
                foreach (var cue in doc.Cues)
                {
                    markerManager.MediaMarkers.Add(new MediaMarker() { Begin = cue.Begin, End = cue.End, Content = cue });
                }
            }
            catch (Exception ex)
            {
                if (ParseFailed != null) ParseFailed(this, new ParseFailedEventArgs(ex));
            }
        }

        private async void RefreshCaption(Caption caption)
        {
            if (caption != null)
            {
                string result = null;
                if (caption.Payload is Uri)
                {
                    try
                    {
                        result = await ((Uri)caption.Payload).LoadToString();
                    }
                    catch
                    {
                        // TODO: expose event to log errors
                        return;
                    }
                }
                else if (caption.Payload is byte[])
                {
                    var byteArray = (byte[])caption.Payload;
                    result = await Task.Run(() => System.Text.Encoding.UTF8.GetString(byteArray, 0, byteArray.Length));
                }
                else if (caption.Payload is string)
                {
                    result = (string)caption.Payload;
                }

                if (result != null)
                {
                    allTasks = EnqueueTask(() => LoadWebVTT(result), allTasks);
                    await allTasks;
                    IsSourceLoaded = true;

                    // refresh the caption based on the current position. Fixes issue where caption is changed while paused.
                    if (IsLoaded) // make sure we didn't get unloaded by the time this completed.
                    {
                        captionsPanel.UpdatePosition(MediaPlayer.Position);
                    }
                }
            }
        }

        Task allTasks;
        static async Task EnqueueTask(Func<Task> newTask, Task taskQueue)
        {
            if (taskQueue != null) await taskQueue;
            await newTask();
        }
    }

    public sealed class ParseFailedEventArgs : EventArgs
    {
        internal ParseFailedEventArgs(Exception error)
        {
            Error = error;
        }

        public Exception Error { get; private set; }
    }
}