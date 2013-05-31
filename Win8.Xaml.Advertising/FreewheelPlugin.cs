using System;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VideoAdvertising;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if SILVERLIGHT
using System.Windows;
#else
using Windows.UI.Xaml;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace Microsoft.PlayerFramework.Advertising
{
    /// <summary>
    /// A plugin that is capable of downloading a VMAP source file, parsing it and using it to schedule when ads should play.
    /// </summary>
    public sealed class FreeWheelPlugin :
#if SILVERLIGHT
 DependencyObject
#else
 FrameworkElement
#endif
, IPlugin
    {
        readonly Dictionary<IAdvertisement, FWTemporalAdSlot> adSlots = new Dictionary<IAdvertisement, FWTemporalAdSlot>();
        private CancellationTokenSource cts;
        private FWAdResponse adResponse;
        private bool trackingEnded;
        private PlayTimeTrackingEvent lastTrackingEvent;
        const string TrackingEventArea = "FreeWheel";
        AdScheduleController adScheduler;
        CancellationTokenSource adCts;
        bool isLoaded;

        /// <summary>
        /// Creates a new instance of FreeWheelPlugin
        /// </summary>
        public FreeWheelPlugin()
        {
            Advertisements = new ObservableCollection<IAdvertisement>();
        }

        /// <inheritdoc /> 
        public MediaPlayer MediaPlayer { get; set; }

        /// <inheritdoc /> 
        public void Load()
        {
            WirePlayer();
            adScheduler = new AdScheduleController();
            adScheduler.MediaPlayer = MediaPlayer;
            adScheduler.EvaluateOnForwardOnly = EvaluateOnForwardOnly;
            adScheduler.SeekToAdPosition = SeekToAdPosition;
            adScheduler.InterruptScrub = InterruptScrub;
            adScheduler.PreloadTime = PreloadTime;
            adScheduler.Advertisements = Advertisements;
            adScheduler.AdStarting += adScheduler_AdStarting;
            adScheduler.Initialize();
            cts = new CancellationTokenSource();
            if (adResponse != null) ShowCompanions();
            isLoaded = true;
        }

        /// <inheritdoc /> 
        public void Update(IMediaSource mediaSource)
        {
            adSlots.Clear();
            Source = VmapScheduler.GetSource((DependencyObject)mediaSource);
            adScheduler.Update(AdScheduler.GetAdvertisements((DependencyObject)mediaSource));
        }

        /// <inheritdoc /> 
        public void Unload()
        {
            cts.Cancel();
            cts = null;
            MediaPlayer.UnloadAllCompanions();
            adScheduler.Uninitialize();
            adScheduler.AdStarting -= adScheduler_AdStarting;
            adScheduler.Dispose();
            UnwirePlayer();
            isLoaded = false;
        }
        
        /// <summary>
        /// Identifies the EvaluateOnForwardOnly dependency property.
        /// </summary>
        public static DependencyProperty EvaluateOnForwardOnlyProperty { get { return evaluateOnForwardOnlyProperty; } }
        static readonly DependencyProperty evaluateOnForwardOnlyProperty = DependencyProperty.Register("EvaluateOnForwardOnly", typeof(bool), typeof(FreeWheelPlugin), new PropertyMetadata(true, (s, e) => ((FreeWheelPlugin)s).OnEvaluateOnForwardOnlyChanged((bool)e.OldValue, (bool)e.NewValue)));

        void OnEvaluateOnForwardOnlyChanged(bool oldValue, bool newValue)
        {
            if (adScheduler != null)
            {
                adScheduler.EvaluateOnForwardOnly = newValue;
            }
        }

        /// <summary>
        /// Gets or sets whether seeking or scrubbing back in time can trigger ads. Set to false to allow ads to be played when seeking backwards. Default is true.
        /// </summary>
        public bool EvaluateOnForwardOnly
        {
            get { return (bool)GetValue(EvaluateOnForwardOnlyProperty); }
            set { SetValue(EvaluateOnForwardOnlyProperty, value); }
        }

        /// <summary>
        /// Identifies the SyncToAdPosition dependency property.
        /// </summary>
        public static DependencyProperty SeekToAdPositionProperty { get { return seekToAdPositionProperty; } }
        static readonly DependencyProperty seekToAdPositionProperty = DependencyProperty.Register("SeekToAdPosition", typeof(bool), typeof(FreeWheelPlugin), new PropertyMetadata(true, (s, e) => ((FreeWheelPlugin)s).OnSeekToAdPositionChanged((bool)e.OldValue, (bool)e.NewValue)));

        void OnSeekToAdPositionChanged(bool oldValue, bool newValue)
        {
            if (adScheduler != null)
            {
                adScheduler.SeekToAdPosition = newValue;
            }
        }

        /// <summary>
        /// Gets or sets whether the position should be set set to that of the ad when an ad is scrubbed or seeked over. Default is true.
        /// </summary>
        public bool SeekToAdPosition
        {
            get { return (bool)GetValue(SeekToAdPositionProperty); }
            set { SetValue(SeekToAdPositionProperty, value); }
        }

        /// <summary>
        /// Identifies the InterruptScrub dependency property.
        /// </summary>
        public static DependencyProperty InterruptScrubProperty { get { return interruptScrubProperty; } }
        static readonly DependencyProperty interruptScrubProperty = DependencyProperty.Register("InterruptScrub", typeof(bool), typeof(FreeWheelPlugin), new PropertyMetadata(true, (s, e) => ((FreeWheelPlugin)s).OnInterruptScrubChanged((bool)e.OldValue, (bool)e.NewValue)));

        void OnInterruptScrubChanged(bool oldValue, bool newValue)
        {
            if (adScheduler != null)
            {
                adScheduler.InterruptScrub = newValue;
            }
        }

        /// <summary>
        /// Gets or sets whether or not scrubbing is interrupted if an ad is encountered. Default is true.
        /// </summary>
        public bool InterruptScrub
        {
            get { return (bool)GetValue(InterruptScrubProperty); }
            set { SetValue(InterruptScrubProperty, value); }
        }

        /// <summary>
        /// Identifies the PreloadTime dependency property.
        /// </summary>
        public static DependencyProperty PreloadTimeProperty { get { return preloadTimeProperty; } }
        static readonly DependencyProperty preloadTimeProperty = DependencyProperty.Register("PreloadTime", typeof(TimeSpan?), typeof(FreeWheelPlugin), new PropertyMetadata(AdScheduleController.DefaultPreloadTime, (s, e) => ((FreeWheelPlugin)s).OnPreloadTimeChanged(e.OldValue as TimeSpan?, e.NewValue as TimeSpan?)));

        void OnPreloadTimeChanged(TimeSpan? oldValue, TimeSpan? newValue)
        {
            if (adScheduler != null)
            {
                adScheduler.PreloadTime = newValue;
            }
        }

        /// <summary>
        /// Gets or sets the amount of time before an ad will occur that preloading will begin. Set to null to disable preloading. Default is 5 seconds.
        /// </summary>
        public TimeSpan? PreloadTime
        {
            get { return (TimeSpan?)GetValue(PreloadTimeProperty); }
            set { SetValue(PreloadTimeProperty, value); }
        }

        /// <summary>
        /// Identifies the Advertisements dependency property.
        /// </summary>
        public static DependencyProperty AdvertisementsProperty { get { return advertisementsProperty; } }
        static readonly DependencyProperty advertisementsProperty = DependencyProperty.Register("Advertisements", typeof(IList<IAdvertisement>), typeof(FreeWheelPlugin), new PropertyMetadata(null, (s, e) => ((FreeWheelPlugin)s).OnAdvertisementsChanged(e.OldValue as IList<IAdvertisement>, e.NewValue as IList<IAdvertisement>)));

        void OnAdvertisementsChanged(IList<IAdvertisement> oldValue, IList<IAdvertisement> newValue)
        {
            if (adScheduler != null)
            {
                adScheduler.Advertisements = newValue;
            }
        }

        /// <summary>
        /// Provides the list of ads to schedule. You can add or remove ads to/from this collection during playback.
        /// </summary>
        public IList<IAdvertisement> Advertisements
        {
            get { return GetValue(AdvertisementsProperty) as IList<IAdvertisement>; }
            set { SetValue(AdvertisementsProperty, value); }
        }

        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static DependencyProperty SourceProperty { get { return sourceProperty; } }
        static readonly DependencyProperty sourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(FreeWheelPlugin), null);

        /// <summary>
        /// Gets or sets the source Uri of the VMAP file
        /// </summary>
        public Uri Source
        {
            get { return GetValue(SourceProperty) as Uri; }
            set { SetValue(SourceProperty, value); }
        }

        private void WirePlayer()
        {
            MediaPlayer.MediaLoading += mediaPlayer_MediaLoading;
            MediaPlayer.MediaClosed += mediaPlayer_MediaClosed;
        }

        private void UnwirePlayer()
        {
            MediaPlayer.MediaLoading -= mediaPlayer_MediaLoading;
            MediaPlayer.MediaClosed -= mediaPlayer_MediaClosed;
        }

        private void trackingPlugin_EventTracked(object sender, IEventTrackedEventArgs e)
        {
            if (e.TrackingEvent.Area == TrackingEventArea)
            {
                var eventCallback = e.TrackingEvent.Data as FWEventCallback;
                if (eventCallback != null)
                {
                    foreach (var url in eventCallback.GetUrls())
                    {
                        TrackVideoViewMarker(url, e);
                    }
                }
            }
        }

        private void TrackVideoViewMarker(string url, IEventTrackedEventArgs e)
        {
            if (!trackingEnded)
            {
                bool isStart;
                bool isEnd;
                TimeSpan currentPlayTime;
                if (e.TrackingEvent is PositionTrackingEvent)
                {
                    var playTimeTrackingPlugin = MediaPlayer.Plugins.OfType<PlayTimeTrackingPlugin>().FirstOrDefault();
                    if (playTimeTrackingPlugin == null) throw new Exception("PlayTimeTrackingPlugin not found; required for FreeWheelPlugin");
                    var positionTrackingEvent = (PositionTrackingEvent)e.TrackingEvent;
                    if (!positionTrackingEvent.PositionPercentage.HasValue || positionTrackingEvent.PositionPercentage.Value != 1) throw new Exception("Invalid tracking event was registered for FreeWheelPlugin");
                    isStart = false;
                    isEnd = true;
                    currentPlayTime = playTimeTrackingPlugin.PlayTime;
                    trackingEnded = true; // set this flag to prevent further tracking
                }
                else if (e.TrackingEvent is PlayTimeTrackingEvent)
                {
                    var playTimeTrackingEvent = (PlayTimeTrackingEvent)e.TrackingEvent;
                    isStart = playTimeTrackingEvent.PlayTime == TimeSpan.Zero;
                    isEnd = false;
                    currentPlayTime = playTimeTrackingEvent.PlayTime;
                }
                else
                {
                    throw new ArgumentException();
                }

                TimeSpan delta;
                if (lastTrackingEvent != null)
                {
                    delta = currentPlayTime - lastTrackingEvent.PlayTime;
                }
                else
                {
                    delta = currentPlayTime;
                }
                lastTrackingEvent = e.TrackingEvent as PlayTimeTrackingEvent;

                var newUrl = url + string.Format("{3}init={0}&ct={1}&last={2}", isStart ? 1 : 0, (int)Math.Round(delta.TotalSeconds), isEnd ? 1 : 0, url.Contains("?") ? "&" : "?");
                AdTracking.Current.FireTracking(newUrl);
            }
        }

        private async void mediaPlayer_MediaLoading(object sender, MediaLoadingEventArgs e)
        {
            if (isLoaded)
            {
                if (Source != null)
                {
                    var deferral = e.DeferrableOperation.GetDeferral();
                    adCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
                    deferral.Canceled += deferral_Canceled;
                    try
                    {
#if NETFX_CORE
                        await loadAds(Source, adCts.Token);
#else
                        await LoadAds(Source, adCts.Token);
#endif
                    }
                    catch { /* ignore */ }
                    finally
                    {
                        deferral.Complete();
                        deferral.Canceled -= deferral_Canceled;
                        adCts.Dispose();
                        adCts = null;
                    }
                }
            }
        }

        void deferral_Canceled(object sender, object e)
        {
            adCts.Cancel();
        }

#if NETFX_CORE
        /// <summary>
        /// Loads ads from a source URI. Note, this is called automatically if you set the source before the MediaLoading event fires and normally does not need to be called.
        /// </summary>
        /// <param name="source">The SmartXML source URI</param>
        /// <returns>A task to await on.</returns>
        public IAsyncAction LoadAds(Uri source)
        {
            return AsyncInfo.Run(c => loadAds(source, c));
        }

        async Task loadAds(Uri source, CancellationToken c)
#else
        /// <summary>
        /// Loads ads from a source URI. Note, this is called automatically if you set the source before the MediaLoading event fires and normally does not need to be called.
        /// </summary>
        /// <param name="source">The SmartXML source URI</param>
        /// <param name="c">A cancellation token that allows you to cancel a pending operation</param>
        /// <returns>A task to await on.</returns>
        public async Task LoadAds(Uri source, CancellationToken c)
#endif
        {
            adSlots.Clear();
            var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(c, cts.Token).Token;

#if SILVERLIGHT
            adResponse = await FreeWheelFactory.LoadSource(source, cancellationToken);
#else
            adResponse = await FreeWheelFactory.LoadSource(source).AsTask(cancellationToken);
#endif

            var videoTracking = adResponse.SiteSection.VideoPlayer.VideoAsset.EventCallbacks.FirstOrDefault(ec => ec.Name == FWEventCallback.VideoView);
            if (videoTracking != null)
            {
                // use the tracking plugins to help with tracking markers. Create it if it doesn't exist.
                var positionTrackingPlugin = MediaPlayer.Plugins.OfType<PositionTrackingPlugin>().FirstOrDefault();
                if (positionTrackingPlugin == null)
                {
                    positionTrackingPlugin = new PositionTrackingPlugin();
                    MediaPlayer.Plugins.Add(positionTrackingPlugin);
                }
                positionTrackingPlugin.EventTracked += trackingPlugin_EventTracked;
                lastTrackingEvent = null; // reset
                trackingEnded = false;
                positionTrackingPlugin.TrackingEvents.Add(new PositionTrackingEvent() { PositionPercentage = 1, Data = videoTracking, Area = TrackingEventArea });

                var playTimeTrackingPlugin = MediaPlayer.Plugins.OfType<PlayTimeTrackingPlugin>().FirstOrDefault();
                if (playTimeTrackingPlugin == null)
                {
                    playTimeTrackingPlugin = new PlayTimeTrackingPlugin();
                    MediaPlayer.Plugins.Add(playTimeTrackingPlugin);
                }
                playTimeTrackingPlugin.EventTracked += trackingPlugin_EventTracked;
                for (int i = 0; i < 60; i = i + 15)
                    playTimeTrackingPlugin.TrackingEvents.Add(new PlayTimeTrackingEvent() { PlayTime = TimeSpan.FromSeconds(i), Data = videoTracking, Area = TrackingEventArea });
                for (int i = 60; i < 60 * 3; i = i + 30)
                    playTimeTrackingPlugin.TrackingEvents.Add(new PlayTimeTrackingEvent() { PlayTime = TimeSpan.FromSeconds(i), Data = videoTracking, Area = TrackingEventArea });
                for (int i = 60 * 3; i < 60 * 10; i = i + 60)
                    playTimeTrackingPlugin.TrackingEvents.Add(new PlayTimeTrackingEvent() { PlayTime = TimeSpan.FromSeconds(i), Data = videoTracking, Area = TrackingEventArea });
                for (int i = 60 * 10; i < 60 * 30; i = i + 120)
                    playTimeTrackingPlugin.TrackingEvents.Add(new PlayTimeTrackingEvent() { PlayTime = TimeSpan.FromSeconds(i), Data = videoTracking, Area = TrackingEventArea });
                for (int i = 60 * 30; i < 60 * 60; i = i + 300)
                    playTimeTrackingPlugin.TrackingEvents.Add(new PlayTimeTrackingEvent() { PlayTime = TimeSpan.FromSeconds(i), Data = videoTracking, Area = TrackingEventArea });
                for (int i = 60 * 60; i < 60 * 180; i = i + 600)
                    playTimeTrackingPlugin.TrackingEvents.Add(new PlayTimeTrackingEvent() { PlayTime = TimeSpan.FromSeconds(i), Data = videoTracking, Area = TrackingEventArea });
            }

            var videoAsset = adResponse.SiteSection.VideoPlayer.VideoAsset;
            if (videoAsset != null)
            {
                foreach (var adSlot in videoAsset.AdSlots)
                {
                    IAdvertisement ad = null;
                    switch (adSlot.TimePositionClass)
                    {
                        case "preroll":
                            ad = new PrerollAdvertisement();
                            break;
                        case "postroll":
                            ad = new PostrollAdvertisement();
                            break;
                        default:
                            var midroll = new MidrollAdvertisement();
                            midroll.Time = adSlot.TimePosition;
                            ad = midroll;
                            break;
                    }

#if SILVERLIGHT
                    var payload = await FreeWheelFactory.GetAdDocumentPayload(adSlot, adResponse, cancellationToken);
#else
                    var payload = await FreeWheelFactory.GetAdDocumentPayload(adSlot, adResponse).AsTask(cancellationToken);
#endif
                    ad.Source = new AdSource(payload, DocumentAdPayloadHandler.AdType);

                    Advertisements.Add(ad);
                    adSlots.Add(ad, adSlot);
                }
            }

            ShowCompanions();
        }

        private void mediaPlayer_MediaClosed(object sender, MediaClosedEventArgs e)
        {
            var playTimeTrackingPlugin = MediaPlayer.Plugins.OfType<PlayTimeTrackingPlugin>().FirstOrDefault();
            if (playTimeTrackingPlugin != null)
            {
                playTimeTrackingPlugin.EventTracked -= trackingPlugin_EventTracked;
            }

            var positionTrackingPlugin = MediaPlayer.Plugins.OfType<PlayTimeTrackingPlugin>().FirstOrDefault();
            if (positionTrackingPlugin != null)
            {
                positionTrackingPlugin.EventTracked -= trackingPlugin_EventTracked;
            }
        }

        private void ShowCompanions()
        {
            foreach (var companionCreative in FreeWheelFactory.GetNonTemporalCompanions(adResponse))
            {
                MediaPlayer.ShowCompanion(companionCreative);
            }
        }

        void adScheduler_AdStarting(object sender, AdStartingEventArgs e)
        {
            var ad = e.Ad;
            if (adSlots.ContainsKey(ad)) // app could have manually added ads besides those from FreeWheel
            {
                var adSlot = adSlots[ad];
                var slotImpression = adSlot.EventCallbacks.FirstOrDefault(ec => ec.Type == FWCallbackType.Impression && ec.Name == FWEventCallback.SlotImpression);
                if (slotImpression != null)
                {
                    foreach (var url in slotImpression.GetUrls())
                    {
                        AdTracking.Current.FireTracking(url);
                    }
                }
            }
        }

    }
}
