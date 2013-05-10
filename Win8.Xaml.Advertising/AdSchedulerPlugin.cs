using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.VideoAdvertising;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#endif

namespace Microsoft.PlayerFramework.Advertising
{
    /// <summary>
    /// The primary plugin used to help schedule ads.
    /// Internally, this plugin calls AdHandlerPlugin when it is time to actually play or preload an ad.
    /// </summary>
    public sealed class AdSchedulerPlugin :
#if SILVERLIGHT
        DependencyObject
#else
        FrameworkElement
#endif
, IPlugin
    {
        AdScheduleController adScheduler;

        /// <summary>
        /// Creates a new instance of AdSchedulerPlugin
        /// </summary>
        public AdSchedulerPlugin()
        {
            Advertisements = new ObservableCollection<IAdvertisement>();
        }

        /// <inheritdoc /> 
        public MediaPlayer MediaPlayer { get; set; }

        /// <inheritdoc /> 
        public void Load()
        {
            adScheduler = new AdScheduleController();
            adScheduler.EvaluateOnForwardOnly = EvaluateOnForwardOnly;
            adScheduler.SeekToAdPosition = SeekToAdPosition;
            adScheduler.InterruptScrub = InterruptScrub;
            adScheduler.PreloadTime = PreloadTime;
            adScheduler.Advertisements = Advertisements;
            adScheduler.Initialize();
        }

        /// <inheritdoc /> 
        public void Update(IMediaSource mediaSource)
        {
            adScheduler.Update(AdScheduler.GetAdvertisements((DependencyObject)mediaSource));
        }

        /// <inheritdoc /> 
        public void Unload()
        {
            adScheduler.Uninitialize();
            adScheduler.Dispose();
        }
        
        /// <summary>
        /// Identifies the EvaluateOnForwardOnly dependency property.
        /// </summary>
        public static DependencyProperty EvaluateOnForwardOnlyProperty { get { return evaluateOnForwardOnlyProperty; } }
        static readonly DependencyProperty evaluateOnForwardOnlyProperty = DependencyProperty.Register("EvaluateOnForwardOnly", typeof(bool), typeof(AdSchedulerPlugin), new PropertyMetadata(true, (s, e) => ((AdSchedulerPlugin)s).OnEvaluateOnForwardOnlyChanged((bool)e.OldValue, (bool)e.NewValue)));

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
        static readonly DependencyProperty seekToAdPositionProperty = DependencyProperty.Register("SeekToAdPosition", typeof(bool), typeof(AdSchedulerPlugin), new PropertyMetadata(true, (s, e) => ((AdSchedulerPlugin)s).OnSeekToAdPositionChanged((bool)e.OldValue, (bool)e.NewValue)));

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
        static readonly DependencyProperty interruptScrubProperty = DependencyProperty.Register("InterruptScrub", typeof(bool), typeof(AdSchedulerPlugin), new PropertyMetadata(true, (s, e) => ((AdSchedulerPlugin)s).OnInterruptScrubChanged((bool)e.OldValue, (bool)e.NewValue)));

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
        static readonly DependencyProperty preloadTimeProperty = DependencyProperty.Register("PreloadTime", typeof(TimeSpan?), typeof(AdSchedulerPlugin), new PropertyMetadata(AdScheduleController.DefaultPreloadTime, (s, e) => ((AdSchedulerPlugin)s).OnPreloadTimeChanged(e.OldValue as TimeSpan?, e.NewValue as TimeSpan?)));

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
        static readonly DependencyProperty advertisementsProperty = DependencyProperty.Register("Advertisements", typeof(IList<IAdvertisement>), typeof(AdSchedulerPlugin), new PropertyMetadata(null, (s, e) => ((AdSchedulerPlugin)s).OnAdvertisementsChanged(e.OldValue as IList<IAdvertisement>, e.NewValue as IList<IAdvertisement>)));

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
    }
}
