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
using System.Windows.Threading;
#else
using Windows.UI.Xaml;
using Windows.System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
#endif

namespace Microsoft.PlayerFramework.Advertising
{
    /// <summary>
    /// A plugin that is capable of downloading a VMAP source file, parsing it and using it to schedule when ads should play.
    /// </summary>
    public sealed class VmapSchedulerPlugin :
#if SILVERLIGHT
        DependencyObject
#else
 FrameworkElement
#endif
, IPlugin
    {
        readonly Dictionary<IAdvertisement, VmapAdBreak> adBreaks = new Dictionary<IAdvertisement, VmapAdBreak>();
        private CancellationTokenSource cts;
        private DispatcherTimer timer;
        AdScheduleController adScheduler;
        CancellationTokenSource adCts;

        /// <summary>
        /// Creates a new instance of VmapSchedulerPlugin
        /// </summary>
        public VmapSchedulerPlugin()
        {
            PollingInterval = TimeSpan.FromSeconds(10);
            Advertisements = new ObservableCollection<IAdvertisement>();
        }

        /// <inheritdoc /> 
        public MediaPlayer MediaPlayer { get; set; }

        /// <inheritdoc /> 
        public void Load()
        {
            adScheduler = new AdScheduleController();
            adScheduler.MediaPlayer = MediaPlayer;
            adScheduler.EvaluateOnForwardOnly = EvaluateOnForwardOnly;
            adScheduler.SeekToAdPosition = SeekToAdPosition;
            adScheduler.InterruptScrub = InterruptScrub;
            adScheduler.PreloadTime = PreloadTime;
            adScheduler.Advertisements = Advertisements;
            adScheduler.AdStarting += adScheduler_AdStarting;
            adScheduler.AdCompleted += adScheduler_AdCompleted;
            adScheduler.Initialize();
            cts = new CancellationTokenSource();
            WirePlayer();
        }

        /// <inheritdoc /> 
        public void Update(IMediaSource mediaSource)
        {
            adBreaks.Clear();
            Source = VmapScheduler.GetSource((DependencyObject)mediaSource);
            adScheduler.Update(AdScheduler.GetAdvertisements((DependencyObject)mediaSource));
        }

        /// <inheritdoc /> 
        public void Unload()
        {
            cts.Cancel();
            cts = null;
            UnwirePlayer();
            adScheduler.Uninitialize();
            adScheduler.AdStarting -= adScheduler_AdStarting;
            adScheduler.AdCompleted -= adScheduler_AdCompleted;
            adScheduler.Dispose();
        }

        /// <summary>
        /// Identifies the EvaluateOnForwardOnly dependency property.
        /// </summary>
        public static DependencyProperty EvaluateOnForwardOnlyProperty { get { return evaluateOnForwardOnlyProperty; } }
        static readonly DependencyProperty evaluateOnForwardOnlyProperty = DependencyProperty.Register("EvaluateOnForwardOnly", typeof(bool), typeof(VmapSchedulerPlugin), new PropertyMetadata(true, (s, e) => ((VmapSchedulerPlugin)s).OnEvaluateOnForwardOnlyChanged((bool)e.OldValue, (bool)e.NewValue)));

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
        static readonly DependencyProperty seekToAdPositionProperty = DependencyProperty.Register("SeekToAdPosition", typeof(bool), typeof(VmapSchedulerPlugin), new PropertyMetadata(true, (s, e) => ((VmapSchedulerPlugin)s).OnSeekToAdPositionChanged((bool)e.OldValue, (bool)e.NewValue)));

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
        static readonly DependencyProperty interruptScrubProperty = DependencyProperty.Register("InterruptScrub", typeof(bool), typeof(VmapSchedulerPlugin), new PropertyMetadata(true, (s, e) => ((VmapSchedulerPlugin)s).OnInterruptScrubChanged((bool)e.OldValue, (bool)e.NewValue)));

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
        static readonly DependencyProperty preloadTimeProperty = DependencyProperty.Register("PreloadTime", typeof(TimeSpan?), typeof(VmapSchedulerPlugin), new PropertyMetadata(AdScheduleController.DefaultPreloadTime, (s, e) => ((VmapSchedulerPlugin)s).OnPreloadTimeChanged(e.OldValue as TimeSpan?, e.NewValue as TimeSpan?)));

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
        static readonly DependencyProperty advertisementsProperty = DependencyProperty.Register("Advertisements", typeof(IList<IAdvertisement>), typeof(VmapSchedulerPlugin), new PropertyMetadata(null, (s, e) => ((VmapSchedulerPlugin)s).OnAdvertisementsChanged(e.OldValue as IList<IAdvertisement>, e.NewValue as IList<IAdvertisement>)));

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
        /// Gets or sets the amount of time to check the server for updated data. Only applies when MediaPlayer.IsLive = true
        /// </summary>
        public TimeSpan PollingInterval { get; set; }

        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static DependencyProperty SourceProperty { get { return sourceProperty; } }
        static readonly DependencyProperty sourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(VmapSchedulerPlugin), null);

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
            MediaPlayer.IsLiveChanged += mediaPlayer_IsLiveChanged;
            if (MediaPlayer.IsLive) InitializeTimer();
        }

        private void UnwirePlayer()
        {
            MediaPlayer.MediaLoading -= mediaPlayer_MediaLoading;
            MediaPlayer.IsLiveChanged -= mediaPlayer_IsLiveChanged;
            ShutdownTimer();
        }

        void mediaPlayer_IsLiveChanged(object sender, RoutedEventArgs e)
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

        async void timer_Tick(object sender, object e)
        {
            try
            {
#if SILVERLIGHT
                var vmap = await VmapFactory.LoadSource(Source, cts.Token);
#else
                var vmap = await VmapFactory.LoadSource(Source).AsTask(cts.Token);
#endif
                // remove all ads that were not found new info
                foreach (var adBreak in adBreaks.Where(existingBreak => !vmap.AdBreaks.Any(newBreak => newBreak.BreakId == existingBreak.Value.BreakId)))
                {
                    Advertisements.Remove(adBreak.Key);
                }
                // create new ads for those that do not already exist
                foreach (var adBreak in vmap.AdBreaks.Where(newBreak => !adBreaks.Values.Any(existingBreak => existingBreak.BreakId == newBreak.BreakId)))
                {
                    CreateAdvertisement(adBreak);
                }
            }
            catch { /* ignore */ }
        }

        async void mediaPlayer_MediaLoading(object sender, MediaLoadingEventArgs e)
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

        void deferral_Canceled(object sender, object e)
        {
            adCts.Cancel();
        }

#if NETFX_CORE
        /// <summary>
        /// Loads ads from a source URI. Note, this is called automatically if you set the source before the MediaLoading event fires and normally does not need to be called.
        /// </summary>
        /// <param name="source">The VMAP source URI</param>
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
        /// <param name="source">The VMAP source URI</param>
        /// <param name="c">A cancellation token that allows you to cancel a pending operation</param>
        /// <returns>A task to await on.</returns>
        public async Task LoadAds(Uri source, CancellationToken c)
#endif
        {
            adBreaks.Clear();
            var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(c, cts.Token).Token;
#if SILVERLIGHT
            var vmap = await VmapFactory.LoadSource(source, cancellationToken);
#else
            var vmap = await VmapFactory.LoadSource(source).AsTask(cancellationToken);
#endif
            foreach (var adBreak in vmap.AdBreaks)
            {
                CreateAdvertisement(adBreak);
            }
        }

        private void CreateAdvertisement(VmapAdBreak adBreak)
        {
            IAdvertisement ad = null;
            switch (adBreak.TimeOffset)
            {
                case "start":
                    ad = new PrerollAdvertisement();
                    break;
                case "end":
                    ad = new PostrollAdvertisement();
                    break;
                default:
                    var offset = FlexibleOffset.Parse(adBreak.TimeOffset);
                    if (offset != null)
                    {
                        var midroll = new MidrollAdvertisement();
                        if (!offset.IsAbsolute)
                        {
                            midroll.TimePercentage = offset.RelativeOffset;
                        }
                        else
                        {
                            midroll.Time = offset.AbsoluteOffset;
                        }
                        ad = midroll;
                    }
                    break;
            }

            if (ad != null)
            {
                ad.Source = GetAdSource(adBreak.AdSource);
                if (ad.Source != null)
                {
                    Advertisements.Add(ad);
                    adBreaks.Add(ad, adBreak);
                }
            }
        }

        void adScheduler_AdStarting(object sender, AdStartingEventArgs e)
        {
            var ad = e.Ad;
            if (adBreaks.ContainsKey(ad)) // app could have manually added ads besides those in vmap
            {
                VmapAdBreak adBreak = adBreaks[ad];
                TrackEvent(adBreak.TrackingEvents.Where(te => te.EventType == VmapTrackingEventType.BreakStart));
            }
        }

        void adScheduler_AdCompleted(object sender, AdCompletedEventArgs e)
        {
            var ad = e.Ad;
            if (adBreaks.ContainsKey(ad)) // app could have manually added ads besides those in vmap
            {
                VmapAdBreak adBreak = adBreaks[ad];
                if (e.Error == null)
                {
                    TrackEvent(adBreak.TrackingEvents.Where(te => te.EventType == VmapTrackingEventType.BreakEnd));
                }
                else
                {
                    TrackEvent(adBreak.TrackingEvents.Where(te => te.EventType == VmapTrackingEventType.Error));
                }
            }
        }

        static void TrackEvent(IEnumerable<VmapTrackingEvent> events)
        {
            foreach (var trackingUri in events.Select(e => e.TrackingUri))
            {
                AdTracking.Current.FireTrackingUri(trackingUri);
            }
        }

        /// <summary>
        /// Creates an IAdSource from a VMAP AdSource (required for the AdHandlerPlugin to play the ad).
        /// </summary>
        /// <param name="source">The VMAP AdSource object</param>
        /// <returns>An IAdSource object that can be played by the AdHandlerPlugin. Returns null if insufficient data is available.</returns>
        public static IAdSource GetAdSource(VmapAdSource source)
        {
            IAdSource result = null;
            if (!string.IsNullOrEmpty(source.VastData))
            {
                result = new AdSource(source.VastData, VastAdPayloadHandler.AdType);
            }
            else if (!string.IsNullOrEmpty(source.CustomAdData))
            {
                result = new AdSource(source.CustomAdData, source.CustomAdDataTemplateType);
            }
            else if (source.AdTag != null)
            {
                result = new RemoteAdSource(source.AdTag, source.AdTagTemplateType);
            }

            if (result != null)
            {
                result.AllowMultipleAds = source.AllowMultipleAds;
                result.MaxRedirectDepth = source.FollowsRedirect ? new int?() : 0;
            }

            return result;
        }
    }
}
