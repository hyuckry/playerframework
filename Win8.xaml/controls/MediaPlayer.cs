#define CODE_ANALYSIS

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using System.Windows.Data;
using System.Linq.Expressions;
using System.Windows.Input;
using System.Resources;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Reflection;
using Windows.Media.PlayTo;
using Windows.Media.Protection;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.ApplicationModel.Resources.Core;
using Windows.ApplicationModel;
using Windows.Storage.Streams;
using Windows.Media;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Represents a media player used to play video or audio and optionally allow the user interact.
    /// This is the primary class in the Microsoft Media Platform Player Framework.
    /// This player offers a super-set of the MediaElement API.
    /// Optional plugins can be automatically detected to help extend or modify the default behavior.
    /// </summary>
    public sealed partial class MediaPlayer : Control, IMediaSource, IDisposable
    {
        /// <summary>
        /// Instantiates a new instance of the MediaPlayer class.
        /// </summary>
        public MediaPlayer()
        {
            this.DefaultStyleKey = typeof(MediaPlayer);
            cts = new CancellationTokenSource();
            AllowMediaStartingDeferrals = true;
            AutoLoadPlugins = true;

            SetValueWithoutCallback(SupportedPlaybackRatesProperty, DefaultSupportedPlaybackRates);
            SetValueWithoutCallback(VisualMarkersProperty, new ObservableCollection<VisualMarker>());
            SetValueWithoutCallback(AvailableCaptionsProperty, new List<ICaption>());
            SetValueWithoutCallback(AvailableAudioStreamsProperty, new List<IAudioStream>());
            SetValueWithoutCallback(TimeFormatConverterProperty, new StringFormatConverter() { StringFormat = DefaultTimeFormat });
            LoadPlugins(new ObservableCollection<IPlugin>());
            UpdateTimer.Interval = UpdateInterval;
            UpdateTimer.Tick += UpdateTimer_Tick;
            InitializeTemplateDefinitions();
        }

        partial void InitializeTemplateDefinitions();

        partial void UninitializeTemplateDefinitions();

        /// <summary>
        /// Provides a cancellation token for all async operations that should be cancelled during Dispose
        /// </summary>
        private CancellationTokenSource cts;

        /// <summary>
        /// The timer used to update the position and other frequently changing properties.
        /// </summary>
        private readonly DispatcherTimer UpdateTimer = new DispatcherTimer();

        /// <summary>
        /// Indicates the playback rate that should be set after scrubbing.
        /// </summary>
        private double rateAfterScrub;

        /// <summary>
        /// Indicates that plugins have been loaded, if true, all new plugins to get loaded when added
        /// </summary>
        private bool pluginsLoaded;

        /// <summary>
        /// Remembers the scrub start position to be relayed in future scrub events.
        /// </summary>
        private TimeSpan startScrubPosition;

        /// <summary>
        /// Remembers the previous position before the last update.
        /// </summary>
        private TimeSpan previousPosition;

#if !SILVERLIGHT
        /// <summary>
        /// Helps identify the most recent pending seek.
        /// </summary>
        private object pendingSeekOperation;

        /// <summary>
        /// The task for the current pending seek.
        /// </summary>
        //private Task currentSeek;
        private TaskCompletionSource<RoutedEventArgs> currentSeek;

        private MediaExtensionManager mediaExtensionManager;
#endif

        /// <summary>
        /// Gets whether or not the media has successfully opened.
        /// Note: returns false if the media has failed.
        /// </summary>
        bool IsMediaOpened
        {
            get
            {
                switch (PlayerState)
                {
                    case PlayerState.Opened:
                    case PlayerState.Started:
                    case PlayerState.Starting:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private bool isMediaLoaded;
        /// <summary>
        /// Gets whether or not the media is actually set on the underlying MediaElement.
        /// </summary>
        bool IsMediaLoaded
        {
            get { return isMediaLoaded; }
            set
            {
                isMediaLoaded = value;
                if (isMediaLoaded)
                {
                    SetValue(PlayerStateProperty, PlayerState.Loaded);
                    UpdateTimer.Start();
                }
                else
                {
                    SetValue(PlayerStateProperty, PlayerState.Unloaded);
                    UpdateTimer.Stop();
                }
            }
        }

        /// <summary>
        /// Indicates the state has changed but filters out changes when the state changes to buffering. 
        /// Buffering is a special case that makes it hard to determine if the video is actually playing, loading or paused.
        /// </summary>
        event RoutedEventHandler CurrentStateChangedBufferingIgnored;

        Action pendingLoadAction;
        /// <summary>
        /// Holds the action to set the source so we can delay things
        /// </summary>
        Action PendingLoadAction
        {
            get { return pendingLoadAction; }
            set
            {
                pendingLoadAction = value;
                if (pendingLoadAction != null)
                {
                    SetValue(PlayerStateProperty, PlayerState.Pending);
                }
            }
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public override async void OnApplyTemplate()
#else
        protected override async void OnApplyTemplate()
#endif
        {
            if (IsTemplateApplied)
            {
                UninitializeTemplateChildren();
                DestroyTemplateChildren();
            }

            base.OnApplyTemplate();
            SetDefaultVisualStates();
            GetTemplateChildren();
            InitializeTemplateChildren();

            if (!pluginsLoaded)
            {
                InitializePlugins();
                pluginsLoaded = true;
            }

            if (!IsTemplateApplied)
            {
                try
                {
                    var deferrableOperation = new MediaPlayerDeferrableOperation(cts);
                    if (Initializing != null) Initializing(this, new InitializingEventArgs(deferrableOperation));
                    await deferrableOperation.Task;
                    IsTemplateApplied = true;
                    OnInitialized();
                }
                catch (OperationCanceledException) { }
            }
        }

        /// <summary>
        /// Occurs immediately after the template is applied and all plugins are loaded
        /// </summary>
        void OnInitialized()
        {
            if (Initialized != null) Initialized(this, new InitializedEventArgs());
        }

        #region Plugins

        void InitializePlugins()
        {
            // initialize any plugins already in the collection
            foreach (var plugin in Plugins.ToList())
            {
                plugin.Load();
            }

            // auto load default plugins
            if (AutoLoadPlugins)
            {
                var PluginsManager = new PluginsFactory();
                PluginsManager.ImportPlugins();
                if (PluginsManager.Plugins != null)
                {
                    // we want to load the plugins ourselves instead of in this event. This allows us to add them all before loading the first one.
                    plugins.CollectionChanged -= Plugins_CollectionChanged;
                    try
                    {
                        foreach (var plugin in PluginsManager.Plugins)
                        {
                            plugins.Add(plugin);
                        }
                        foreach (var plugin in PluginsManager.Plugins)
                        {
                            plugin.MediaPlayer = this;
                            plugin.Load();
                        }
                    }
                    finally // turn on the event again
                    {
                        plugins.CollectionChanged += Plugins_CollectionChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether default plugins should be automatically added.
        /// Set to false to optimize if you are not using any plugins or if you want to manually set which plugins are connected.
        /// You can programmatically connect plugins by adding to the Plugins collection. Default is true.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Correctly named architectural pattern")]
        public bool AutoLoadPlugins { get; set; }

        ObservableCollection<IPlugin> plugins;
        /// <summary>
        /// Gets the collection of connected plugins. You can dynamically add a plugin to the collection at any time and it will be appropriately wired when added and unwired when removed.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Correctly named architectural pattern")]
        public IList<IPlugin> Plugins
        {
            get { return plugins; }
        }

        void LoadPlugins(ObservableCollection<IPlugin> value)
        {
            if (plugins != null)
            {
                plugins.CollectionChanged -= Plugins_CollectionChanged;
                foreach (var plugin in plugins)
                {
                    if (pluginsLoaded) plugin.Unload();
                    plugin.MediaPlayer = null;
                }
            }

            plugins = value;

            if (plugins != null)
            {
                plugins.CollectionChanged += Plugins_CollectionChanged;
                foreach (var plugin in plugins)
                {
                    plugin.MediaPlayer = this;
                    if (pluginsLoaded) plugin.Load();
                }
            }
        }

        void Plugins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var plugin in e.NewItems.Cast<IPlugin>())
                {
                    plugin.MediaPlayer = this;
                    if (pluginsLoaded) plugin.Load();
                }
            }

            if (e.OldItems != null)
            {
                foreach (var plugin in e.OldItems.Cast<IPlugin>())
                {
                    if (pluginsLoaded) plugin.Unload();
                    plugin.MediaPlayer = null;
                }
            }
        }

        #endregion

        #region Methods

#if SILVERLIGHT
        /// <summary>
        /// Sends a request to generate a log which will then be raised through the LogReady event.
        /// </summary>
        public void RequestLog()
        {
            _RequestLog();
        }

        /// <summary>
        /// This sets the source of the MediaElement to a subclass of System.Windows.Media.MediaStreamSource.
        /// </summary>
        /// <param name="mediaStreamSource">A subclass of System.Windows.Media.MediaStreamSource.</param>
        public void SetSource(MediaStreamSource mediaStreamSource)
        {
            RegisterApplyTemplateAction(async () =>
            {
                if (AutoLoad || mediaStreamSource == null)
                {
                    bool proceed = true;
                    MediaLoadingInstruction loadingResult = null;
                    if (mediaStreamSource != null)
                    {
                        loadingResult = await OnMediaLoadingAsync(mediaStreamSource);
                        proceed = loadingResult != null;
                    }
                    if (proceed)
                    {
                        LoadSource(loadingResult);
                    }
                    else
                    {
                        OnMediaFailure(null);
                    }
                }
                else
                {
                    PendingLoadAction = () => SetSource(mediaStreamSource);
                }
            });
        }

        /// <summary>
        /// Sets the MediaElement.Source property using the supplied stream.
        /// </summary>
        /// <param name="stream">A stream that contains a valid media source.</param>
        public void SetSource(Stream stream)
        {
            RegisterApplyTemplateAction(async () =>
            {
                if (AutoLoad || stream == null)
                {
                    bool proceed = true;
                    MediaLoadingInstruction loadingResult = null;
                    if (stream != null)
                    {
                        loadingResult = await OnMediaLoadingAsync(stream);
                        proceed = loadingResult != null;
                    }
                    if (proceed)
                    {
                        LoadSource(loadingResult);
                    }
                    else
                    {
                        OnMediaFailure(null);
                    }
                }
                else
                {
                    PendingLoadAction = () => SetSource(stream);
                }
            });
        }

        /// <summary>
        /// Changes the display mode (or Stretch property) when called to the next in the list and starts at the beginning if at the end of the list.
        /// The order is Uniform, UniformToFill, Fill, None.
        /// </summary>
        public void CycleDisplayMode()
        {
            switch (Stretch)
            {
                case Stretch.Uniform:
                    Stretch = Stretch.UniformToFill;
                    break;
                case Stretch.UniformToFill:
                    Stretch = Stretch.Fill;
                    break;
                case Stretch.Fill:
                    Stretch = Stretch.None;
                    break;
                case Stretch.None:
                    Stretch = Stretch.Uniform;
                    break;
            }
        }
#else

        /// <summary>
        /// Applies an audio effect to playback. Takes effect for the next source set on the MediaElement.
        /// </summary>
        /// <param name="effectID">The identifier for the desired effect.</param>
        /// <param name="effectOptional">True if the effect should not block playback in cases where the effect cannot be used at run time. False if playback should be blocked in cases where the effect cannot be used at run time.</param>
        /// <param name="effectConfiguration">A property set that transmits property values to specific effects as selected by effectID.</param>
        public void AddAudioEffect(string effectID, bool effectOptional, IPropertySet effectConfiguration)
        {
            _AddAudioEffect(effectID, effectOptional, effectConfiguration);
        }

        /// <summary>
        /// Applies a video effect to playback. Takes effect for the next source set on the MediaElement.
        /// </summary>
        /// <param name="effectID">The identifier for the desired effect.</param>
        /// <param name="effectOptional">True if the effect should not block playback in cases where the effect cannot be used at run time. False if playback should be blocked in cases where the effect cannot be used at run time.</param>
        /// <param name="effectConfiguration">A property set that transmits property values to specific effects as selected by effectID.</param>
        public void AddVideoEffect(string effectID, bool effectOptional, IPropertySet effectConfiguration)
        {
            _AddVideoEffect(effectID, effectOptional, effectConfiguration);
        }

        /// <summary>
        /// Removes all effects for the next source set for this MediaElement.
        /// </summary>
        public void RemoveAllEffects()
        {
            _RemoveAllEffects();
        }

        /// <summary>
        /// Returns an enumeration value that describes the likelihood that the current MediaElement and its client configuration can play that media source.
        /// </summary>
        /// <param name="type">A string that describes the desired type as a MIME string.</param>
        /// <returns>A value of the enumeration that describes the likelihood that the source can be played by the current media engine.</returns>
        public MediaCanPlayResponse CanPlayType(string type)
        {
            return _CanPlayType(type);
        }

        /// <summary>
        /// Gets the language for a given audio stream.
        /// </summary>
        /// <param name="index">The index of the audio stream.</param>
        /// <returns>The language for the audio stream.</returns>
        public string GetAudioStreamLanguage(int? index)
        {
            return _GetAudioStreamLanguage(index);
        }

        /// <summary>
        /// Sets the Source property using the supplied stream.
        /// </summary>
        /// <param name="stream">The stream that contains the media to load.</param>
        /// <param name="mimeType">The MIME type of the media resource, expressed as the string form typically seen in HTTP headers and requests.</param>
        public void SetSource(IRandomAccessStream stream, string mimeType)
        {
            RegisterApplyTemplateAction(async () =>
            {
                if (AutoLoad || stream == null)
                {
                    bool proceed = true;
                    MediaLoadingInstruction loadingResult = null;
                    if (stream != null)
                    {
                        loadingResult = await OnMediaLoadingAsync(stream, mimeType);
                        proceed = loadingResult != null;
                    }
                    if (proceed)
                    {
                        LoadSource(loadingResult);
                    }
                    else
                    {
                        OnMediaFailure(null);
                    }
                }
                else
                {
                    PendingLoadAction = () => SetSource(stream, mimeType);
                }
            });
        }

#endif

        /// <summary>
        /// Decreases the PlaybackRate. When called, PlaybackRate will halve until it reaches 1. Once it reaches 1, it will flip to negative numbers causing the player to rewind.
        /// </summary>
        public void DecreasePlaybackRate()
        {
            var ascendingPlaybackRates = SupportedPlaybackRates.Where(r => r <= -1 || r >= 1).OrderByDescending(r => r);
            var availableRates = ascendingPlaybackRates.SkipWhile(r => r >= PlaybackRate);
            if (availableRates.Any())
            {
                PlaybackRate = availableRates.First();
            }
        }

        /// <summary>
        /// Increases the PlaybackRate. When called, PlaybackRate will double until it reaches -1. Once it reaches -1, it will flip to positive numbers causing the player to fast forward.
        /// </summary>
        public void IncreasePlaybackRate()
        {
            var ascendingPlaybackRates = SupportedPlaybackRates.Where(r => r <= -1 || r >= 1).OrderBy(r => r);
            var availableRates = ascendingPlaybackRates.SkipWhile(r => r <= PlaybackRate);
            if (availableRates.Any())
            {
                PlaybackRate = availableRates.First();
            }
        }

        /// <summary>
        /// Supports Instant Replay by subtracting the amount of time specified by the ReplayOffset property from the current Position.
        /// </summary>
        public void Replay()
        {
            TimeSpan newPosition = Position.Subtract(ReplayOffset);
            if (newPosition < StartTime)
            {
                newPosition = StartTime;
            }

            Position = newPosition;

            if (CurrentState == MediaElementState.Paused)
            {
                Play();
            }
        }

        /// <summary>
        /// Stops and closes the current media source. Fires MediaClosed.
        /// </summary>
        public void Unload()
        {
            Source = null;
        }

        /// <summary>
        /// Plays the media or resets the PlaybackRate if already playing.
        /// </summary>
        public void PlayResume()
        {
            if (PlaybackRate != DefaultPlaybackRate)
            {
                PlaybackRate = DefaultPlaybackRate;
            }
            Play();
        }

        /// <summary>
        /// Stops and resets media to be played from the beginning.
        /// </summary>
        public void Stop()
        {
            _Stop();
        }

        /// <summary>
        /// Pauses media at the current position.
        /// </summary>
        public void Pause()
        {
            _Pause();
        }

        /// <summary>
        /// Plays media from the current position.
        /// </summary>
        public async void Play()
        {
            if (PlayerState == PlayerState.Started || await OnMediaStartingAsync())
            {
                _Play();
            }
        }

        /// <summary>
        /// Invokes the captions selection dialog.
        /// </summary>
        public void InvokeCaptionSelection()
        {
            OnInvokeCaptionSelection(new CaptionsInvokedEventArgs());
        }

        /// <summary>
        /// Invokes the audio stream selection dialog.
        /// </summary>
        public void InvokeAudioSelection()
        {
            OnInvokeAudioSelection(new AudioSelectionInvokedEventArgs());
        }

        /// <summary>
        /// Seeks to the live position during live playback.
        /// </summary>
        public void SeekToLive()
        {
            OnSeekToLive(new GoLiveInvokedEventArgs());
            if (LivePosition.HasValue)
            {
                bool canceled;
                Seek(LivePosition.Value, out canceled);
            }
        }

        void Seek(TimeSpan position, out bool cancel)
        {
            var previousPosition = Position;
            var args = new SeekingEventArgs(previousPosition, position);
            OnSeeking(args);
            cancel = args.Canceled;
            if (!args.Canceled)
            {
                var t = SeekAsync(position);
            }
        }

        void SkipAhead(TimeSpan position)
        {
            var skippingEventArgs = new SkippingEventArgs(position);
            if (SkippingAhead != null) SkippingAhead(this, skippingEventArgs);
            if (!skippingEventArgs.Canceled)
            {
                var previousPosition = Position;
                var args = new SeekingEventArgs(previousPosition, position);
                OnSeeking(args);
                if (!args.Canceled)
                {
                    var t = SeekAsync(position);
                }
            }
        }

        void SkipBack(TimeSpan position)
        {
            var skippingEventArgs = new SkippingEventArgs(position);
            if (SkippingBack != null) SkippingBack(this, skippingEventArgs);
            if (!skippingEventArgs.Canceled)
            {
                var previousPosition = Position;
                var args = new SeekingEventArgs(previousPosition, position);
                OnSeeking(args);
                if (!args.Canceled)
                {
                    var t = SeekAsync(position);
                }
            }
        }

        void CompleteScrub(TimeSpan position, bool alreadyCanceled, out bool canceled)
        {
            var args = new CompletingScrubEventArgs(startScrubPosition, position);
            args.Canceled = alreadyCanceled;
            OnCompletingScrub(args);
            canceled = args.Canceled;
            if (!canceled)
            {
                if (!SeekWhileScrubbing)
                {
                    var t = SeekAsync(position);
                }
            }
            PlaybackRate = rateAfterScrub;
            SetValue(IsScrubbingProperty, false);
        }

        void StartScrub(TimeSpan position, out bool canceled)
        {
            startScrubPosition = Position;
            rateAfterScrub = PlaybackRate;
            var args = new StartingScrubEventArgs(position);
            OnStartingScrub(args);
            canceled = args.Canceled;
            if (!canceled)
            {
                PlaybackRate = 0;
                SetValue(IsScrubbingProperty, true);
            }
        }

        void Scrub(TimeSpan position, out bool canceled)
        {
            var args = new ScrubbingEventArgs(startScrubPosition, position);
            OnScrubbing(args);
            canceled = args.Canceled;
            if (!canceled)
            {
                if (SeekWhileScrubbing)
                {
                    var t = SeekAsync(position);
                }
            }
        }

        #endregion

        #region Events
#if SILVERLIGHT
        /// <summary>
        /// Occurs when the log is ready.
        /// </summary>
        public event LogReadyRoutedEventHandler LogReady;

        /// <summary>
        /// Occurs when the Stretch property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<Stretch> StretchChanged;

#else
        /// <summary>
        /// Occurs when the seek point of a requested seek operation is ready for playback.
        /// </summary>
        public event RoutedEventHandler SeekCompleted;

#endif

        /// <summary>
        /// Occurs when the template is loaded for the first time and all plugins have been loaded.
        /// </summary>
        public event EventHandler<InitializedEventArgs> Initialized;

        /// <summary>
        /// Occurs immediately before Initialized fires but is deferrable (therefore allowing custom async operations to take place before Xaml properties are applied).
        /// </summary>
        public event EventHandler<InitializingEventArgs> Initializing;

        /// <summary>
        /// Occurs when the SelectedCaption property changed.
        /// </summary>
        public event EventHandler<SelectedCaptionChangedEventArgs> SelectedCaptionChanged;

        /// <summary>
        /// Occurs when the SelectedAudioStream property changed.
        /// </summary>
        public event EventHandler<SelectedAudioStreamChangedEventArgs> SelectedAudioStreamChanged;

        /// <summary>
        /// Occurs when the PlayerState property changed. This is different from the MediaState.
        /// </summary>
        public event EventHandler<PlayerStateChangedEventArgs> PlayerStateChanged;

        /// <summary>
        /// Occurs just before the source is set and offers the ability to perform blocking async operations.
        /// </summary>
        public event EventHandler<MediaLoadingEventArgs> MediaLoading;

        /// <summary>
        /// Occurs just before the MediaEnded event fires and offers the ability to perform blocking async operations.
        /// </summary>
        public event EventHandler<MediaEndingEventArgs> MediaEnding;

        /// <summary>
        /// Occurs when the timer fires and gives an opportunity to update info without creating a separate timer.
        /// This only fires while media is open and continues to fire even after it's ended or while paused.
        /// </summary>
        public event EventHandler<UpdatedEventArgs> Updated;

        /// <summary>
        /// Occurs when the BufferingProgress property changes.
        /// </summary>
        public event RoutedEventHandler BufferingProgressChanged;

        /// <summary>
        /// Occurs when the value of the CurrentState property changes.
        /// </summary>
        public event RoutedEventHandler CurrentStateChanged;

        /// <summary>
        /// Occurs when the DownloadProgress property has changed.
        /// </summary>
        public event RoutedEventHandler DownloadProgressChanged;

        /// <summary>
        /// Occurs when a timeline marker is encountered during media playback.
        /// </summary>
        public event TimelineMarkerRoutedEventHandler MarkerReached;

        /// <summary>
        /// Occurs when the MediaElement is no longer playing audio or video.
        /// </summary>
        public event EventHandler<MediaEndedEventArgs> MediaEnded;

        /// <summary>
        /// Occurs when there is an error associated with the media Source.
        /// </summary>
        public event ExceptionRoutedEventHandler MediaFailed;

        /// <summary>
        /// Occurs when the playback of new media is about to start.
        /// </summary>
        public event EventHandler<MediaStartingEventArgs> MediaStarting;

        /// <summary>
        /// Occurs when the playback of new media has actually started.
        /// </summary>
        public event EventHandler<MediaStartedEventArgs> MediaStarted;

        /// <summary>
        /// Occurs when the MediaElement has opened the media source audio or video.
        /// </summary>
        public event RoutedEventHandler MediaOpened;

        /// <summary>
        /// Occurs when the PlaybackRate property changes.
        /// </summary>
        public event RateChangedRoutedEventHandler RateChanged;

        /// <summary>
        /// Occurs when the MediaElement source has been closed (set to null).
        /// </summary>
        public event EventHandler<MediaClosedEventArgs> MediaClosed;

        /// <summary>
        /// Occurs when the Position property changes.
        /// </summary>
        public event EventHandler<PositionChangedEventArgs> PositionChanged;

        /// <summary>
        /// Occurs when the Volume property changes.
        /// </summary>
        public event RoutedEventHandler VolumeChanged;

        /// <summary>
        /// Occurs when the IsMuted property changes.
        /// </summary>
        public event EventHandler<IsMutedChangedEventArgs> IsMutedChanged;

        /// <summary>
        /// Occurs when the IsLive property changes.
        /// </summary>
        public event EventHandler<IsLiveChangedEventArgs> IsLiveChanged;

        /// <summary>
        /// Occurs when the AudioStreamIndex property changes.
        /// </summary>
        public event EventHandler<AudioStreamIndexChangedEventArgs> AudioStreamIndexChanged;

        /// <summary>
        /// Occurs when the IsFullScreen property changes.
        /// </summary>
        public event EventHandler<IsFullScreenChangedEventArgs> IsFullScreenChanged;

        /// <summary>
        /// Occurs when the AdvertisingState property changes.
        /// </summary>
        public event EventHandler<AdvertisingStateChangedEventArgs> AdvertisingStateChanged;

        /// <summary>
        /// Occurs when the IsCaptionsActive property changes.
        /// </summary>
        public event EventHandler<IsCaptionsActiveChangedEventArgs> IsCaptionsActiveChanged;

        /// <summary>
        /// Occurs while the user seeks. This is mutually exclusive from scrubbing.
        /// </summary>
        public event EventHandler<SeekingEventArgs> Seeking;

        /// <summary>
        /// Occurs while the user skips forward.
        /// </summary>
        public event EventHandler<SkippingEventArgs> SkippingAhead;

        /// <summary>
        /// Occurs while the user skips backward.
        /// </summary>
        public event EventHandler<SkippingEventArgs> SkippingBack;

        /// <summary>
        /// Occurs while the user is scrubbing. Raised for each new position.
        /// </summary>
        public event EventHandler<ScrubbingEventArgs> Scrubbing;

        /// <summary>
        /// Occurs when the user has completed scrubbing.
        /// </summary>
        public event EventHandler<CompletingScrubEventArgs> CompletingScrub;

        /// <summary>
        /// Occurs when the user starts scrubbing.
        /// </summary>
        public event EventHandler<StartingScrubEventArgs> StartingScrub;

        /// <summary>
        /// Occurs when the SignalStrength property changes.
        /// </summary>
        public event EventHandler<SignalStrengthChangedEventArgs> SignalStrengthChanged;

        /// <summary>
        /// Occurs when the HighDefinition property changes.
        /// </summary>
        public event EventHandler<MediaQualityChangedEventArgs> MediaQualityChanged;

        /// <summary>
        /// Occurs when the IsSlowMotion property changes.
        /// </summary>
        public event EventHandler<IsSlowMotionChangedEventArgs> IsSlowMotionChanged;

        /// <summary>
        /// Occurs when the Duration property changes.
        /// </summary>
        public event EventHandler<DurationChangedEventArgs> DurationChanged;

        /// <summary>
        /// Occurs when the StartTime property changes.
        /// </summary>
        public event EventHandler<StartTimeChangedEventArgs> StartTimeChanged;

        /// <summary>
        /// Occurs when the EndTime property changes.
        /// </summary>
        public event EventHandler<EndTimeChangedEventArgs> EndTimeChanged;

        /// <summary>
        /// Occurs when the TimeRemaining property changes.
        /// </summary>
        public event EventHandler<TimeRemainingChangedEventArgs> TimeRemainingChanged;

        /// <summary>
        /// Occurs when the MaxPosition property changes.
        /// </summary>
        public event EventHandler<LivePositionChangedEventArgs> LivePositionChanged;

        /// <summary>
        /// Occurs when the TimeFormatConverter property changes.
        /// </summary>
        public event EventHandler<TimeFormatConverterChangedEventArgs> TimeFormatConverterChanged;

        /// <summary>
        /// Occurs when the SkipBackInterval property changes.
        /// </summary>
        public event EventHandler<SkipBackIntervalChangedEventArgs> SkipBackIntervalChanged;

        /// <summary>
        /// Occurs when the SkipAheadInterval property changes.
        /// </summary>
        public event EventHandler<SkipAheadIntervalChangedEventArgs> SkipAheadIntervalChanged;

        /// <summary>
        /// Occurs when the SeekToLive method is called.
        /// </summary>
        public event EventHandler<GoLiveInvokedEventArgs> GoLiveInvoked;

        /// <summary>
        /// Occurs when the InvokeCaptionSelection method is called.
        /// </summary>
        public event EventHandler<CaptionsInvokedEventArgs> CaptionsInvoked;

        /// <summary>
        /// Occurs when the InvokeAudioSelection method is called.
        /// </summary>
        public event EventHandler<AudioSelectionInvokedEventArgs> AudioSelectionInvoked;

        #endregion

        #region Properties

        #region Enabled

        #region IsCaptionSelectionEnabled

        /// <summary>
        /// Occurs when the IsCaptionSelectionEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsCaptionSelectionEnabledChanged;
#else
        public event EventHandler<object> IsCaptionSelectionEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsCaptionSelectionEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsCaptionSelectionEnabledProperty { get { return isCaptionSelectionEnabledProperty; } }
        static readonly DependencyProperty isCaptionSelectionEnabledProperty = RegisterDependencyProperty<bool>("IsCaptionSelectionEnabled", (t, o, n) => t.OnIsCaptionSelectionEnabledChanged(), true);

        void OnIsCaptionSelectionEnabledChanged()
        {
            if (IsCaptionSelectionEnabledChanged != null) IsCaptionSelectionEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether GoLive can occur.
        /// </summary>
        public bool IsCaptionSelectionEnabled
        {
            get { return (bool)GetValue(IsCaptionSelectionEnabledProperty); }
            set { SetValue(IsCaptionSelectionEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsCaptionSelectionAllowed property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsCaptionSelectionAllowedChanged;
#else
        public event EventHandler<object> IsCaptionSelectionAllowedChanged;
#endif

        /// <summary>
        /// Gets whether go live is allowed based on the state of the player.
        /// </summary>
        public bool IsCaptionSelectionAllowed
        {
            get
            {
                return CurrentState != MediaElementState.Closed && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading;
            }
        }

        /// <summary>
        /// Indicates that the go live enabled state may have changed.
        /// </summary>
        void NotifyIsCaptionSelectionAllowedChanged()
        {
            if (IsCaptionSelectionAllowedChanged != null) IsCaptionSelectionAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsGoLiveEnabled

        /// <summary>
        /// Occurs when the IsGoLiveEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsGoLiveEnabledChanged;
#else
        public event EventHandler<object> IsGoLiveEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsGoLiveEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsGoLiveEnabledProperty { get { return isGoLiveEnabledProperty; } }
        static readonly DependencyProperty isGoLiveEnabledProperty = RegisterDependencyProperty<bool>("IsGoLiveEnabled", (t, o, n) => t.OnIsGoLiveEnabledChanged(), true);

        void OnIsGoLiveEnabledChanged()
        {
            if (IsGoLiveEnabledChanged != null) IsGoLiveEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether GoLive can occur.
        /// </summary>
        public bool IsGoLiveEnabled
        {
            get { return (bool)GetValue(IsGoLiveEnabledProperty); }
            set { SetValue(IsGoLiveEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsGoLiveAllowed property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsGoLiveAllowedChanged;
#else
        public event EventHandler<object> IsGoLiveAllowedChanged;
#endif

        /// <summary>
        /// Gets whether go live is allowed based on the state of the player.
        /// </summary>
        public bool IsGoLiveAllowed
        {
            get
            {
                return CanSeek && IsLive && !IsPositionLive && CurrentState != MediaElementState.Closed && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading;
            }
        }

        /// <summary>
        /// Indicates that the go live enabled state may have changed.
        /// </summary>
        void NotifyIsGoLiveAllowedChanged()
        {
            if (IsGoLiveAllowedChanged != null) IsGoLiveAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsPlayResumeEnabled

        /// <summary>
        /// Occurs when the IsPlayResumeEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsPlayResumeEnabledChanged;
#else
        public event EventHandler<object> IsPlayResumeEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsPlayResumeEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsPlayResumeEnabledProperty { get { return isPlayResumeEnabledProperty; } }
        static readonly DependencyProperty isPlayResumeEnabledProperty = RegisterDependencyProperty<bool>("IsPlayResumeEnabled", (t, o, n) => t.OnIsPlayResumeEnabledChanged(), true);

        void OnIsPlayResumeEnabledChanged()
        {
            if (IsPlayResumeEnabledChanged != null) IsPlayResumeEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether PlayResume can occur.
        /// </summary>
        public bool IsPlayResumeEnabled
        {
            get { return (bool)GetValue(IsPlayResumeEnabledProperty); }
            set { SetValue(IsPlayResumeEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsPlayResumeAllowed property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsPlayResumeAllowedChanged;
#else
        public event EventHandler<object> IsPlayResumeAllowedChanged;
#endif

        /// <summary>
        /// Indicates that play is preferred over pause. Useful for binding to the toggle state of a Play/Pause button.
        /// </summary>
        public bool IsPlayResumeAllowed
        {
            get { return CurrentState != MediaElementState.Closed && (CurrentState != MediaElementState.Playing || (PlaybackRate != DefaultPlaybackRate && PlaybackRate != 0)) && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the play resume enabled state may have changed.
        /// </summary>
        void NotifyIsPlayResumeAllowedChanged()
        {
            if (IsPlayResumeAllowedChanged != null) IsPlayResumeAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsPauseEnabled

        /// <summary>
        /// Occurs when the IsPauseEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsPauseEnabledChanged;
#else
        public event EventHandler<object> IsPauseEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsPauseEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsPauseEnabledProperty { get { return isPauseEnabledProperty; } }
        static readonly DependencyProperty isPauseEnabledProperty = RegisterDependencyProperty<bool>("IsPauseEnabled", (t, o, n) => t.OnIsPauseEnabledChanged(), true);

        void OnIsPauseEnabledChanged()
        {
            if (IsPauseEnabledChanged != null) IsPauseEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether Pause can occur.
        /// </summary>
        public bool IsPauseEnabled
        {
            get { return (bool)GetValue(IsPauseEnabledProperty); }
            set { SetValue(IsPauseEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsPauseEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsPauseAllowedChanged;
#else
        public event EventHandler<object> IsPauseAllowedChanged;
#endif

        /// <summary>
        /// Gets whether pause is allowed based on the state of the player.
        /// </summary>
        public bool IsPauseAllowed
        {
            get { return CanPause && CurrentState == MediaElementState.Playing && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the pause enabled state may have changed.
        /// </summary>
        void NotifyIsPauseAllowedChanged()
        {
            if (IsPauseAllowedChanged != null) IsPauseAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsStopEnabled

        /// <summary>
        /// Occurs when the IsStopEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsStopEnabledChanged;
#else
        public event EventHandler<object> IsStopEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsStopEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsStopEnabledProperty { get { return isStopEnabledProperty; } }
        static readonly DependencyProperty isStopEnabledProperty = RegisterDependencyProperty<bool>("IsStopEnabled", (t, o, n) => t.OnIsStopEnabledChanged(), true);

        void OnIsStopEnabledChanged()
        {
            if (IsStopEnabledChanged != null) IsStopEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether Stop can occur.
        /// </summary>
        public bool IsStopEnabled
        {
            get { return (bool)GetValue(IsStopEnabledProperty); }
            set { SetValue(IsStopEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsStopEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsStopAllowedChanged;
#else
        public event EventHandler<object> IsStopAllowedChanged;
#endif

        /// <summary>
        /// Gets whether stop is allowed based on the state of the player.
        /// </summary>
        public bool IsStopAllowed
        {
            get { return CurrentState != MediaElementState.Closed || CurrentState != MediaElementState.Stopped; }
        }

        /// <summary>
        /// Indicates that the stop enabled state may have changed.
        /// </summary>
        void NotifyIsStopAllowedChanged()
        {
            if (IsStopAllowedChanged != null) IsStopAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsReplayEnabled

        /// <summary>
        /// Occurs when the IsReplayEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsReplayEnabledChanged;
#else
        public event EventHandler<object> IsReplayEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsReplayEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsReplayEnabledProperty { get { return isReplayEnabledProperty; } }
        static readonly DependencyProperty isReplayEnabledProperty = RegisterDependencyProperty<bool>("IsReplayEnabled", (t, o, n) => t.OnIsReplayEnabledChanged(), true);

        void OnIsReplayEnabledChanged()
        {
            if (IsReplayEnabledChanged != null) IsReplayEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether Replay can occur.
        /// </summary>
        public bool IsReplayEnabled
        {
            get { return (bool)GetValue(IsReplayEnabledProperty); }
            set { SetValue(IsReplayEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsReplayEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsReplayAllowedChanged;
#else
        public event EventHandler<object> IsReplayAllowedChanged;
#endif

        /// <summary>
        /// Gets whether replay is allowed based on the state of the player.
        /// </summary>
        public bool IsReplayAllowed
        {
            get { return CanSeek && CurrentState == MediaElementState.Paused || CurrentState == MediaElementState.Playing && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the replay enabled state may have changed.
        /// </summary>
        void NotifyIsReplayAllowedChanged()
        {
            if (IsReplayAllowedChanged != null) IsReplayAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsAudioSelectionEnabled

        /// <summary>
        /// Occurs when the IsAudioSelectionEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsAudioSelectionEnabledChanged;
#else
        public event EventHandler<object> IsAudioSelectionEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsAudioSelectionEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsAudioSelectionEnabledProperty { get { return isAudioSelectionEnabledProperty; } }
        static readonly DependencyProperty isAudioSelectionEnabledProperty = RegisterDependencyProperty<bool>("IsAudioSelectionEnabled", (t, o, n) => t.OnIsAudioSelectionEnabledChanged(), true);

        void OnIsAudioSelectionEnabledChanged()
        {
            if (IsAudioSelectionEnabledChanged != null) IsAudioSelectionEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether AudioStreamSelection can occur.
        /// </summary>
        public bool IsAudioSelectionEnabled
        {
            get { return (bool)GetValue(IsAudioSelectionEnabledProperty); }
            set { SetValue(IsAudioSelectionEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsAudioSelectionEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsAudioSelectionAllowedChanged;
#else
        public event EventHandler<object> IsAudioSelectionAllowedChanged;
#endif

        /// <summary>
        /// Gets whether audio stream selection is allowed based on the state of the player.
        /// </summary>
        public bool IsAudioSelectionAllowed
        {
            get { return AvailableAudioStreams.Any() && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the audio stream selection enabled state may have changed.
        /// </summary>
        void NotifyIsAudioSelectionAllowedChanged()
        {
            if (IsAudioSelectionAllowedChanged != null) IsAudioSelectionAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsRewindEnabled

        /// <summary>
        /// Occurs when the IsRewindEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsRewindEnabledChanged;
#else
        public event EventHandler<object> IsRewindEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsRewindEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsRewindEnabledProperty { get { return isRewindEnabledProperty; } }
        static readonly DependencyProperty isRewindEnabledProperty = RegisterDependencyProperty<bool>("IsRewindEnabled", (t, o, n) => t.OnIsRewindEnabledChanged(), true);

        void OnIsRewindEnabledChanged()
        {
            if (IsRewindEnabledChanged != null) IsRewindEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether Rewind can occur.
        /// </summary>
        public bool IsRewindEnabled
        {
            get { return (bool)GetValue(IsRewindEnabledProperty); }
            set { SetValue(IsRewindEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsRewindEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsRewindAllowedChanged;
#else
        public event EventHandler<object> IsRewindAllowedChanged;
#endif

        /// <summary>
        /// Gets whether rewind is allowed based on the state of the player.
        /// </summary>
        public bool IsRewindAllowed
        {
            get { return CurrentState == MediaElementState.Playing && PlaybackRate > SupportedPlaybackRates.Min() && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the rewind enabled state may have changed.
        /// </summary>
        void NotifyIsRewindAllowedChanged()
        {
            if (IsRewindAllowedChanged != null) IsRewindAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsFastForwardEnabled

        /// <summary>
        /// Occurs when the IsFastForwardEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsFastForwardEnabledChanged;
#else
        public event EventHandler<object> IsFastForwardEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsFastForwardEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsFastForwardEnabledProperty { get { return isFastForwardEnabledProperty; } }
        static readonly DependencyProperty isFastForwardEnabledProperty = RegisterDependencyProperty<bool>("IsFastForwardEnabled", (t, o, n) => t.OnIsFastForwardEnabledChanged(), true);

        void OnIsFastForwardEnabledChanged()
        {
            if (IsFastForwardEnabledChanged != null) IsFastForwardEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether FastForward can occur.
        /// </summary>
        public bool IsFastForwardEnabled
        {
            get { return (bool)GetValue(IsFastForwardEnabledProperty); }
            set { SetValue(IsFastForwardEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsFastForwardEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsFastForwardAllowedChanged;
#else
        public event EventHandler<object> IsFastForwardAllowedChanged;
#endif

        /// <summary>
        /// Gets whether fast forward is allowed based on the state of the player.
        /// </summary>
        public bool IsFastForwardAllowed
        {
            get { return CurrentState == MediaElementState.Playing && PlaybackRate < SupportedPlaybackRates.Max() && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the fast forward enabled state may have changed.
        /// </summary>
        void NotifyIsFastForwardAllowedChanged()
        {
            if (IsFastForwardAllowedChanged != null) IsFastForwardAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsSlowMotionEnabled

        /// <summary>
        /// Occurs when the IsSlowMotionEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsSlowMotionEnabledChanged;
#else
        public event EventHandler<object> IsSlowMotionEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsSlowMotionEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsSlowMotionEnabledProperty { get { return isSlowMotionEnabledProperty; } }
        static readonly DependencyProperty isSlowMotionEnabledProperty = RegisterDependencyProperty<bool>("IsSlowMotionEnabled", (t, o, n) => t.OnIsSlowMotionEnabledChanged(), true);

        void OnIsSlowMotionEnabledChanged()
        {
            if (IsSlowMotionEnabledChanged != null) IsSlowMotionEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether SlowMotion can occur.
        /// </summary>
        public bool IsSlowMotionEnabled
        {
            get { return (bool)GetValue(IsSlowMotionEnabledProperty); }
            set { SetValue(IsSlowMotionEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsSlowMotionEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsSlowMotionAllowedChanged;
#else
        public event EventHandler<object> IsSlowMotionAllowedChanged;
#endif

        /// <summary>
        /// Gets whether slow motion is allowed based on the state of the player.
        /// </summary>
        public bool IsSlowMotionAllowed
        {
            get { return CurrentState == MediaElementState.Playing && SupportedPlaybackRates.Where(r => r > 0 && r < 1).Any() && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the slow motion enabled state may have changed.
        /// </summary>
        void NotifyIsSlowMotionAllowedChanged()
        {
            if (IsSlowMotionAllowedChanged != null) IsSlowMotionAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsSeekEnabled

        /// <summary>
        /// Occurs when the IsSeekEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsSeekEnabledChanged;
#else
        public event EventHandler<object> IsSeekEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsSeekEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsSeekEnabledProperty { get { return isSeekEnabledProperty; } }
        static readonly DependencyProperty isSeekEnabledProperty = RegisterDependencyProperty<bool>("IsSeekEnabled", (t, o, n) => t.OnIsSeekEnabledChanged(), true);

        void OnIsSeekEnabledChanged()
        {
            if (IsSeekEnabledChanged != null) IsSeekEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether Seek can occur.
        /// </summary>
        public bool IsSeekEnabled
        {
            get { return (bool)GetValue(IsSeekEnabledProperty); }
            set { SetValue(IsSeekEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsSeekEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsSeekAllowedChanged;
#else
        public event EventHandler<object> IsSeekAllowedChanged;
#endif

        /// <summary>
        /// Gets whether seek is allowed based on the state of the player.
        /// </summary>
        public bool IsSeekAllowed
        {
            get { return CanSeek && CurrentState != MediaElementState.Closed && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the seek enabled state may have changed.
        /// </summary>
        void NotifyIsSeekAllowedChanged()
        {
            if (IsSeekAllowedChanged != null) IsSeekAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsSkipPreviousEnabled

        /// <summary>
        /// Occurs when the IsSkipPreviousEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsSkipPreviousEnabledChanged;
#else
        public event EventHandler<object> IsSkipPreviousEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsSkipPreviousEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsSkipPreviousEnabledProperty { get { return isSkipPreviousEnabledProperty; } }
        static readonly DependencyProperty isSkipPreviousEnabledProperty = RegisterDependencyProperty<bool>("IsSkipPreviousEnabled", (t, o, n) => t.OnIsSkipPreviousEnabledChanged(), true);

        void OnIsSkipPreviousEnabledChanged()
        {
            if (IsSkipPreviousEnabledChanged != null) IsSkipPreviousEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether the SkipPrevious method can be called.
        /// </summary>
        public bool IsSkipPreviousEnabled
        {
            get { return (bool)GetValue(IsSkipPreviousEnabledProperty); }
            set { SetValue(IsSkipPreviousEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsSkipPreviousEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsSkipPreviousAllowedChanged;
#else
        public event EventHandler<object> IsSkipPreviousAllowedChanged;
#endif

        /// <summary>
        /// Gets whether skipping previous is allowed based on the state of the player.
        /// </summary>
        public bool IsSkipPreviousAllowed
        {
            get { return CanSeek && CurrentState != MediaElementState.Closed && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the skip previous enabled state may have changed.
        /// </summary>
        void NotifyIsSkipPreviousAllowedChanged()
        {
            if (IsSkipPreviousAllowedChanged != null) IsSkipPreviousAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsSkipNextEnabled

        /// <summary>
        /// Occurs when the IsSkipNextEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsSkipNextEnabledChanged;
#else
        public event EventHandler<object> IsSkipNextEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsSkipNextEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsSkipNextEnabledProperty { get { return isSkipNextEnabledProperty; } }
        static readonly DependencyProperty isSkipNextEnabledProperty = RegisterDependencyProperty<bool>("IsSkipNextEnabled", (t, o, n) => t.OnIsSkipNextEnabledChanged(), true);

        void OnIsSkipNextEnabledChanged()
        {
            if (IsSkipNextEnabledChanged != null) IsSkipNextEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether the SkipNext method can be called.
        /// </summary>
        public bool IsSkipNextEnabled
        {
            get { return (bool)GetValue(IsSkipNextEnabledProperty); }
            set { SetValue(IsSkipNextEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsSkipNextEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsSkipNextAllowedChanged;
#else
        public event EventHandler<object> IsSkipNextAllowedChanged;
#endif

        /// <summary>
        /// Gets whether skipping next is allowed based on the state of the player.
        /// </summary>
        public bool IsSkipNextAllowed
        {
            get { return CanSeek && CurrentState != MediaElementState.Closed && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the skip next enabled state may have changed.
        /// </summary>
        void NotifyIsSkipNextAllowedChanged()
        {
            if (IsSkipNextAllowedChanged != null) IsSkipNextAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsSkipBackEnabled

        /// <summary>
        /// Occurs when the IsSkipBackEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsSkipBackEnabledChanged;
#else
        public event EventHandler<object> IsSkipBackEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsSkipBackEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsSkipBackEnabledProperty { get { return isSkipBackEnabledProperty; } }
        static readonly DependencyProperty isSkipBackEnabledProperty = RegisterDependencyProperty<bool>("IsSkipBackEnabled", (t, o, n) => t.OnIsSkipBackEnabledChanged(), true);

        void OnIsSkipBackEnabledChanged()
        {
            if (IsSkipBackEnabledChanged != null) IsSkipBackEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether the SkipBack method can be called.
        /// </summary>
        public bool IsSkipBackEnabled
        {
            get { return (bool)GetValue(IsSkipBackEnabledProperty); }
            set { SetValue(IsSkipBackEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsSkipBackEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsSkipBackAllowedChanged;
#else
        public event EventHandler<object> IsSkipBackAllowedChanged;
#endif

        /// <summary>
        /// Gets whether skipping back is allowed based on the state of the player.
        /// </summary>
        public bool IsSkipBackAllowed
        {
            get { return CanSeek && CurrentState != MediaElementState.Closed && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the skip back enabled state may have changed.
        /// </summary>
        void NotifyIsSkipBackAllowedChanged()
        {
            if (IsSkipBackAllowedChanged != null) IsSkipBackAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsSkipAheadEnabled

        /// <summary>
        /// Occurs when the IsSkipAheadEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsSkipAheadEnabledChanged;
#else
        public event EventHandler<object> IsSkipAheadEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsSkipAheadEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsSkipAheadEnabledProperty { get { return isSkipAheadEnabledProperty; } }
        static readonly DependencyProperty isSkipAheadEnabledProperty = RegisterDependencyProperty<bool>("IsSkipAheadEnabled", (t, o, n) => t.OnIsSkipAheadEnabledChanged(), true);

        void OnIsSkipAheadEnabledChanged()
        {
            if (IsSkipAheadEnabledChanged != null) IsSkipAheadEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether the SkipAhead method can be called.
        /// </summary>
        public bool IsSkipAheadEnabled
        {
            get { return (bool)GetValue(IsSkipAheadEnabledProperty); }
            set { SetValue(IsSkipAheadEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsSkipAheadEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsSkipAheadAllowedChanged;
#else
        public event EventHandler<object> IsSkipAheadAllowedChanged;
#endif

        /// <summary>
        /// Gets whether skipping ahead is allowed based on the state of the player.
        /// </summary>
        public bool IsSkipAheadAllowed
        {
            get { return CanSeek && CurrentState != MediaElementState.Closed && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the skip ahead enabled state may have changed.
        /// </summary>
        void NotifyIsSkipAheadAllowedChanged()
        {
            if (IsSkipAheadAllowedChanged != null) IsSkipAheadAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region IsScrubbingEnabled

        /// <summary>
        /// Occurs when the IsScrubbingEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsScrubbingEnabledChanged;
#else
        public event EventHandler<object> IsScrubbingEnabledChanged;
#endif

        /// <summary>
        /// Identifies the IsScrubbingEnabled dependency property.
        /// </summary>
        public static DependencyProperty IsScrubbingEnabledProperty { get { return isScrubbingEnabledProperty; } }
        static readonly DependencyProperty isScrubbingEnabledProperty = RegisterDependencyProperty<bool>("IsScrubbingEnabled", (t, o, n) => t.OnIsScrubbingEnabledChanged(), true);

        void OnIsScrubbingEnabledChanged()
        {
            if (IsScrubbingEnabledChanged != null) IsScrubbingEnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets based on the current state whether scrubbing can occur.
        /// </summary>
        public bool IsScrubbingEnabled
        {
            get { return (bool)GetValue(IsScrubbingEnabledProperty); }
            set { SetValue(IsScrubbingEnabledProperty, value); }
        }

        /// <summary>
        /// Occurs when the IsScrubbingEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler IsScrubbingAllowedChanged;
#else
        public event EventHandler<object> IsScrubbingAllowedChanged;
#endif

        /// <summary>
        /// Gets whether scrubbing is allowed based on the state of the player.
        /// </summary>
        public bool IsScrubbingAllowed
        {
            get { return CanSeek && CurrentState != MediaElementState.Closed && (PlayerState == PlayerState.Started || PlayerState == PlayerState.Opened) && AdvertisingState != AdvertisingState.Linear && AdvertisingState != AdvertisingState.Loading; }
        }

        /// <summary>
        /// Indicates that the scrubbing enabled state may have changed.
        /// </summary>
        void NotifyIsScrubbingAllowedChanged()
        {
            if (IsScrubbingAllowedChanged != null) IsScrubbingAllowedChanged(this, EventArgs.Empty);
        }
        #endregion

        #endregion

        #region Visibility

#if SILVERLIGHT
        #region IsDisplayModeVisible
        /// <summary>
        /// Identifies the IsDisplayModeVisible dependency property.
        /// </summary>
        public static DependencyProperty IsDisplayModeVisibleProperty { get { return isDisplayModeVisibleProperty; } }
        static readonly DependencyProperty isDisplayModeVisibleProperty = RegisterDependencyProperty<bool>("IsDisplayModeVisible", (t, o, n) => t.OnIsDisplayModeVisibleChanged(), DefaultIsDisplayModeVisible);

        static bool DefaultIsDisplayModeVisible
        {
            get
            {
#if WINDOWS_PHONE
                return true;
#else
                return false;
#endif
            }
        }

        void OnIsDisplayModeVisibleChanged()
        {
            if (IsDisplayModeVisibleChanged != null) IsDisplayModeVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsDisplayModeVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsDisplayModeVisibleChanged;
#else
        public event EventHandler<object> IsDisplayModeVisibleChanged;
#endif
#else
        public event EventHandler<object> IsDisplayModeVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive DisplayMode feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsDisplayModeVisible
        {
            get { return (bool)GetValue(IsDisplayModeVisibleProperty); }
            set { SetValue(IsDisplayModeVisibleProperty, value); }
        }
        #endregion
#endif

        #region IsAudioSelectionVisible
        /// <summary>
        /// Identifies the IsAudioSelectionVisible dependency property.
        /// </summary>
        public static DependencyProperty IsAudioSelectionVisibleProperty { get { return isAudioSelectionVisibleProperty; } }
        static readonly DependencyProperty isAudioSelectionVisibleProperty = RegisterDependencyProperty<bool>("IsAudioSelectionVisible", (t, o, n) => t.OnIsAudioSelectionVisibleChanged(), false);

        void OnIsAudioSelectionVisibleChanged()
        {
            if (IsAudioSelectionVisibleChanged != null) IsAudioSelectionVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsAudioSelectionVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsAudioSelectionVisibleChanged;
#else
        public event EventHandler<object> IsAudioSelectionVisibleChanged;
#endif
#else
        public event EventHandler<object> IsAudioSelectionVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive AudioStreamSelection feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsAudioSelectionVisible
        {
            get { return (bool)GetValue(IsAudioSelectionVisibleProperty); }
            set { SetValue(IsAudioSelectionVisibleProperty, value); }
        }
        #endregion

        #region IsCaptionSelectionVisible
        /// <summary>
        /// Identifies the IsCaptionSelectionVisible dependency property.
        /// </summary>
        public static DependencyProperty IsCaptionSelectionVisibleProperty { get { return isCaptionSelectionVisibleProperty; } }
        static readonly DependencyProperty isCaptionSelectionVisibleProperty = RegisterDependencyProperty<bool>("IsCaptionSelectionVisible", (t, o, n) => t.OnIsCaptionSelectionVisibleChanged(), false);

        void OnIsCaptionSelectionVisibleChanged()
        {
            if (IsCaptionSelectionVisibleChanged != null) IsCaptionSelectionVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsCaptionSelectionVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsCaptionSelectionVisibleChanged;
#else
        public event EventHandler<object> IsCaptionSelectionVisibleChanged;
#endif
#else
        public event EventHandler<object> IsCaptionSelectionVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive Captions feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsCaptionSelectionVisible
        {
            get { return (bool)GetValue(IsCaptionSelectionVisibleProperty); }
            set { SetValue(IsCaptionSelectionVisibleProperty, value); }
        }
        #endregion

        #region IsDurationVisible
        /// <summary>
        /// Identifies the IsDurationVisible dependency property.
        /// </summary>
        public static DependencyProperty IsDurationVisibleProperty { get { return isDurationVisibleProperty; } }
        static readonly DependencyProperty isDurationVisibleProperty = RegisterDependencyProperty<bool>("IsDurationVisible", (t, o, n) => t.OnIsDurationVisibleChanged(), false);

        void OnIsDurationVisibleChanged()
        {
            if (IsDurationVisibleChanged != null) IsDurationVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsDurationVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsDurationVisibleChanged;
#else
        public event EventHandler<object> IsDurationVisibleChanged;
#endif
#else
        public event EventHandler<object> IsDurationVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive Duration feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsDurationVisible
        {
            get { return (bool)GetValue(IsDurationVisibleProperty); }
            set { SetValue(IsDurationVisibleProperty, value); }
        }
        #endregion

        #region IsTimeRemainingVisible
        /// <summary>
        /// Identifies the IsTimeRemainingVisible dependency property.
        /// </summary>
        public static DependencyProperty IsTimeRemainingVisibleProperty { get { return isTimeRemainingVisibleProperty; } }
        static readonly DependencyProperty isTimeRemainingVisibleProperty = RegisterDependencyProperty<bool>("IsTimeRemainingVisible", (t, o, n) => t.OnIsTimeRemainingVisibleChanged(), true);

        void OnIsTimeRemainingVisibleChanged()
        {
            if (IsTimeRemainingVisibleChanged != null) IsTimeRemainingVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsTimeRemainingVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsTimeRemainingVisibleChanged;
#else
        public event EventHandler<object> IsTimeRemainingVisibleChanged;
#endif
#else
        public event EventHandler<object> IsTimeRemainingVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive TimeRemaining feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsTimeRemainingVisible
        {
            get { return (bool)GetValue(IsTimeRemainingVisibleProperty); }
            set { SetValue(IsTimeRemainingVisibleProperty, value); }
        }
        #endregion

        #region IsFastForwardVisible
        /// <summary>
        /// Identifies the IsFastForwardVisible dependency property.
        /// </summary>
        public static DependencyProperty IsFastForwardVisibleProperty { get { return isFastForwardVisibleProperty; } }
        static readonly DependencyProperty isFastForwardVisibleProperty = RegisterDependencyProperty<bool>("IsFastForwardVisible", (t, o, n) => t.OnIsFastForwardVisibleChanged(), false);

        void OnIsFastForwardVisibleChanged()
        {
            if (IsFastForwardVisibleChanged != null) IsFastForwardVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsFastForwardVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsFastForwardVisibleChanged;
#else
        public event EventHandler<object> IsFastForwardVisibleChanged;
#endif
#else
        public event EventHandler<object> IsFastForwardVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive FastForward feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsFastForwardVisible
        {
            get { return (bool)GetValue(IsFastForwardVisibleProperty); }
            set { SetValue(IsFastForwardVisibleProperty, value); }
        }
        #endregion

        #region IsFullScreenVisible
        /// <summary>
        /// Identifies the IsFullScreenVisible dependency property.
        /// </summary>
        public static DependencyProperty IsFullScreenVisibleProperty { get { return isFullScreenVisibleProperty; } }
        static readonly DependencyProperty isFullScreenVisibleProperty = RegisterDependencyProperty<bool>("IsFullScreenVisible", (t, o, n) => t.OnIsFullScreenVisibleChanged(), false);

        void OnIsFullScreenVisibleChanged()
        {
            if (IsFullScreenVisibleChanged != null) IsFullScreenVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsFullScreenVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsFullScreenVisibleChanged;
#else
        public event EventHandler<object> IsFullScreenVisibleChanged;
#endif
#else
        public event EventHandler<object> IsFullScreenVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive FullScreen feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsFullScreenVisible
        {
            get { return (bool)GetValue(IsFullScreenVisibleProperty); }
            set { SetValue(IsFullScreenVisibleProperty, value); }
        }
        #endregion

        #region IsGoLiveVisible
        /// <summary>
        /// Identifies the IsGoLiveVisible dependency property.
        /// </summary>
        public static DependencyProperty IsGoLiveVisibleProperty { get { return isGoLiveVisibleProperty; } }
        static readonly DependencyProperty isGoLiveVisibleProperty = RegisterDependencyProperty<bool>("IsGoLiveVisible", (t, o, n) => t.OnIsGoLiveVisibleChanged(), false);

        void OnIsGoLiveVisibleChanged()
        {
            if (IsGoLiveVisibleChanged != null) IsGoLiveVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsGoLiveVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsGoLiveVisibleChanged;
#else
        public event EventHandler<object> IsGoLiveVisibleChanged;
#endif
#else
        public event EventHandler<object> IsGoLiveVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive GoLive feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsGoLiveVisible
        {
            get { return (bool)GetValue(IsGoLiveVisibleProperty); }
            set { SetValue(IsGoLiveVisibleProperty, value); }
        }
        #endregion

        #region IsPlayPauseVisible
        /// <summary>
        /// Identifies the IsPlayPauseVisible dependency property.
        /// </summary>
        public static DependencyProperty IsPlayPauseVisibleProperty { get { return isPlayPauseVisibleProperty; } }
        static readonly DependencyProperty isPlayPauseVisibleProperty = RegisterDependencyProperty<bool>("IsPlayPauseVisible", (t, o, n) => t.OnIsPlayPauseVisibleChanged(), true);

        void OnIsPlayPauseVisibleChanged()
        {
            if (IsPlayPauseVisibleChanged != null) IsPlayPauseVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsPlayPauseVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsPlayPauseVisibleChanged;
#else
        public event EventHandler<object> IsPlayPauseVisibleChanged;
#endif
#else
        public event EventHandler<object> IsPlayPauseVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive PlayPause feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsPlayPauseVisible
        {
            get { return (bool)GetValue(IsPlayPauseVisibleProperty); }
            set { SetValue(IsPlayPauseVisibleProperty, value); }
        }
        #endregion

        #region IsTimeElapsedVisible
        /// <summary>
        /// Identifies the IsTimeElapsedVisible dependency property.
        /// </summary>
        public static DependencyProperty IsTimeElapsedVisibleProperty { get { return isTimeElapsedVisibleProperty; } }
        static readonly DependencyProperty isTimeElapsedVisibleProperty = RegisterDependencyProperty<bool>("IsTimeElapsedVisible", (t, o, n) => t.OnIsTimeElapsedVisibleChanged(), true);

        void OnIsTimeElapsedVisibleChanged()
        {
            if (IsTimeElapsedVisibleChanged != null) IsTimeElapsedVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsTimeElapsedVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsTimeElapsedVisibleChanged;
#else
        public event EventHandler<object> IsTimeElapsedVisibleChanged;
#endif
#else
        public event EventHandler<object> IsTimeElapsedVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive Position feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsTimeElapsedVisible
        {
            get { return (bool)GetValue(IsTimeElapsedVisibleProperty); }
            set { SetValue(IsTimeElapsedVisibleProperty, value); }
        }
        #endregion

        #region IsSkipBackVisible
        /// <summary>
        /// Identifies the IsSkipBackVisible dependency property.
        /// </summary>
        public static DependencyProperty IsSkipBackVisibleProperty { get { return isSkipBackVisibleProperty; } }
        static readonly DependencyProperty isSkipBackVisibleProperty = RegisterDependencyProperty<bool>("IsSkipBackVisible", (t, o, n) => t.OnIsSkipBackVisibleChanged(), DefaultIsSkipBackVisible);

        static bool DefaultIsSkipBackVisible
        {
            get
            {
#if WINDOWS_PHONE
                return true;
#else
                return false;
#endif
            }
        }

        void OnIsSkipBackVisibleChanged()
        {
            if (IsSkipBackVisibleChanged != null) IsSkipBackVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsSkipBackVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsSkipBackVisibleChanged;
#else
        public event EventHandler<object> IsSkipBackVisibleChanged;
#endif
#else
        public event EventHandler<object> IsSkipBackVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive SkipBack feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsSkipBackVisible
        {
            get { return (bool)GetValue(IsSkipBackVisibleProperty); }
            set { SetValue(IsSkipBackVisibleProperty, value); }
        }
        #endregion

        #region IsSkipAheadVisible
        /// <summary>
        /// Identifies the IsSkipAheadVisible dependency property.
        /// </summary>
        public static DependencyProperty IsSkipAheadVisibleProperty { get { return isSkipAheadVisibleProperty; } }
        static readonly DependencyProperty isSkipAheadVisibleProperty = RegisterDependencyProperty<bool>("IsSkipAheadVisible", (t, o, n) => t.OnIsSkipAheadVisibleChanged(), DefaultIsSkipAheadVisible);

        static bool DefaultIsSkipAheadVisible
        {
            get
            {
#if WINDOWS_PHONE
                return true;
#else
                return false;
#endif
            }
        }

        void OnIsSkipAheadVisibleChanged()
        {
            if (IsSkipAheadVisibleChanged != null) IsSkipAheadVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsSkipAheadVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsSkipAheadVisibleChanged;
#else
        public event EventHandler<object> IsSkipAheadVisibleChanged;
#endif
#else
        public event EventHandler<object> IsSkipAheadVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive SkipAhead feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsSkipAheadVisible
        {
            get { return (bool)GetValue(IsSkipAheadVisibleProperty); }
            set { SetValue(IsSkipAheadVisibleProperty, value); }
        }
        #endregion

        #region IsReplayVisible
        /// <summary>
        /// Identifies the IsReplayVisible dependency property.
        /// </summary>
        public static DependencyProperty IsReplayVisibleProperty { get { return isReplayVisibleProperty; } }
        static readonly DependencyProperty isReplayVisibleProperty = RegisterDependencyProperty<bool>("IsReplayVisible", (t, o, n) => t.OnIsReplayVisibleChanged(), false);

        void OnIsReplayVisibleChanged()
        {
            if (IsReplayVisibleChanged != null) IsReplayVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsReplayVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsReplayVisibleChanged;
#else
        public event EventHandler<object> IsReplayVisibleChanged;
#endif
#else
        public event EventHandler<object> IsReplayVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive Replay feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsReplayVisible
        {
            get { return (bool)GetValue(IsReplayVisibleProperty); }
            set { SetValue(IsReplayVisibleProperty, value); }
        }
        #endregion

        #region IsRewindVisible
        /// <summary>
        /// Identifies the IsRewindVisible dependency property.
        /// </summary>
        public static DependencyProperty IsRewindVisibleProperty { get { return isRewindVisibleProperty; } }
        static readonly DependencyProperty isRewindVisibleProperty = RegisterDependencyProperty<bool>("IsRewindVisible", (t, o, n) => t.OnIsRewindVisibleChanged(), false);

        void OnIsRewindVisibleChanged()
        {
            if (IsRewindVisibleChanged != null) IsRewindVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsRewindVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsRewindVisibleChanged;
#else
        public event EventHandler<object> IsRewindVisibleChanged;
#endif
#else
        public event EventHandler<object> IsRewindVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive Rewind feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsRewindVisible
        {
            get { return (bool)GetValue(IsRewindVisibleProperty); }
            set { SetValue(IsRewindVisibleProperty, value); }
        }
        #endregion

        #region IsSkipPreviousVisible
        /// <summary>
        /// Identifies the IsSkipPreviousVisible dependency property.
        /// </summary>
        public static DependencyProperty IsSkipPreviousVisibleProperty { get { return isSkipPreviousVisibleProperty; } }
        static readonly DependencyProperty isSkipPreviousVisibleProperty = RegisterDependencyProperty<bool>("IsSkipPreviousVisible", (t, o, n) => t.OnIsSkipPreviousVisibleChanged(), false);

        void OnIsSkipPreviousVisibleChanged()
        {
            if (IsSkipPreviousVisibleChanged != null) IsSkipPreviousVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsSkipPreviousVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsSkipPreviousVisibleChanged;
#else
        public event EventHandler<object> IsSkipPreviousVisibleChanged;
#endif
#else
        public event EventHandler<object> IsSkipPreviousVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive SkipPrevious feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsSkipPreviousVisible
        {
            get { return (bool)GetValue(IsSkipPreviousVisibleProperty); }
            set { SetValue(IsSkipPreviousVisibleProperty, value); }
        }
        #endregion

        #region IsSkipNextVisible
        /// <summary>
        /// Identifies the IsSkipNextVisible dependency property.
        /// </summary>
        public static DependencyProperty IsSkipNextVisibleProperty { get { return isSkipNextVisibleProperty; } }
        static readonly DependencyProperty isSkipNextVisibleProperty = RegisterDependencyProperty<bool>("IsSkipNextVisible", (t, o, n) => t.OnIsSkipNextVisibleChanged(), false);

        void OnIsSkipNextVisibleChanged()
        {
            if (IsSkipNextVisibleChanged != null) IsSkipNextVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsSkipNextVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsSkipNextVisibleChanged;
#else
        public event EventHandler<object> IsSkipNextVisibleChanged;
#endif
#else
        public event EventHandler<object> IsSkipNextVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive SkipNext feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsSkipNextVisible
        {
            get { return (bool)GetValue(IsSkipNextVisibleProperty); }
            set { SetValue(IsSkipNextVisibleProperty, value); }
        }
        #endregion

        #region IsSlowMotionVisible
        /// <summary>
        /// Identifies the IsSlowMotionVisible dependency property.
        /// </summary>
        public static DependencyProperty IsSlowMotionVisibleProperty { get { return isSlowMotionVisibleProperty; } }
        static readonly DependencyProperty isSlowMotionVisibleProperty = RegisterDependencyProperty<bool>("IsSlowMotionVisible", (t, o, n) => t.OnIsSlowMotionVisibleChanged(), false);

        void OnIsSlowMotionVisibleChanged()
        {
            if (IsSlowMotionVisibleChanged != null) IsSlowMotionVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsSlowMotionVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsSlowMotionVisibleChanged;
#else
        public event EventHandler<object> IsSlowMotionVisibleChanged;
#endif
#else
        public event EventHandler<object> IsSlowMotionVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive SlowMotion feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsSlowMotionVisible
        {
            get { return (bool)GetValue(IsSlowMotionVisibleProperty); }
            set { SetValue(IsSlowMotionVisibleProperty, value); }
        }
        #endregion

        #region IsStopVisible
        /// <summary>
        /// Identifies the IsStopVisible dependency property.
        /// </summary>
        public static DependencyProperty IsStopVisibleProperty { get { return isStopVisibleProperty; } }
        static readonly DependencyProperty isStopVisibleProperty = RegisterDependencyProperty<bool>("IsStopVisible", (t, o, n) => t.OnIsStopVisibleChanged(), false);

        void OnIsStopVisibleChanged()
        {
            if (IsStopVisibleChanged != null) IsStopVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsStopVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsStopVisibleChanged;
#else
        public event EventHandler<object> IsStopVisibleChanged;
#endif
#else
        public event EventHandler<object> IsStopVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive Stop feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsStopVisible
        {
            get { return (bool)GetValue(IsStopVisibleProperty); }
            set { SetValue(IsStopVisibleProperty, value); }
        }
        #endregion

        #region IsTimelineVisible
        /// <summary>
        /// Identifies the IsTimelineVisible dependency property.
        /// </summary>
        public static DependencyProperty IsTimelineVisibleProperty { get { return isTimelineVisibleProperty; } }
        static readonly DependencyProperty isTimelineVisibleProperty = RegisterDependencyProperty<bool>("IsTimelineVisible", (t, o, n) => t.OnIsTimelineVisibleChanged(), true);

        void OnIsTimelineVisibleChanged()
        {
            if (IsTimelineVisibleChanged != null) IsTimelineVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsTimelineVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsTimelineVisibleChanged;
#else
        public event EventHandler<object> IsTimelineVisibleChanged;
#endif
#else
        public event EventHandler<object> IsTimelineVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive Timeline feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsTimelineVisible
        {
            get { return (bool)GetValue(IsTimelineVisibleProperty); }
            set { SetValue(IsTimelineVisibleProperty, value); }
        }
        #endregion

        #region IsVolumeVisible
        /// <summary>
        /// Identifies the IsVolumeVisible dependency property.
        /// </summary>
        public static DependencyProperty IsVolumeVisibleProperty { get { return isVolumeVisibleProperty; } }
        static readonly DependencyProperty isVolumeVisibleProperty = RegisterDependencyProperty<bool>("IsVolumeVisible", (t, o, n) => t.OnIsVolumeVisibleChanged(), true);

        void OnIsVolumeVisibleChanged()
        {
            if (IsVolumeVisibleChanged != null) IsVolumeVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsVolumeVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsVolumeVisibleChanged;
#else
        public event EventHandler<object> IsVolumeVisibleChanged;
#endif
#else
        public event EventHandler<object> IsVolumeVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive Volume feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsVolumeVisible
        {
            get { return (bool)GetValue(IsVolumeVisibleProperty); }
            set { SetValue(IsVolumeVisibleProperty, value); }
        }
        #endregion

        #region IsSignalStrengthVisible
        /// <summary>
        /// Identifies the IsSignalStrengthVisible dependency property.
        /// </summary>
        public static DependencyProperty IsSignalStrengthVisibleProperty { get { return isSignalStrengthVisibleProperty; } }
        static readonly DependencyProperty isSignalStrengthVisibleProperty = RegisterDependencyProperty<bool>("IsSignalStrengthVisible", (t, o, n) => t.OnIsSignalStrengthVisibleChanged(), false);

        void OnIsSignalStrengthVisibleChanged()
        {
            if (IsSignalStrengthVisibleChanged != null) IsSignalStrengthVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsSignalStrengthVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsSignalStrengthVisibleChanged;
#else
        public event EventHandler<object> IsSignalStrengthVisibleChanged;
#endif
#else
        public event EventHandler<object> IsSignalStrengthVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive SignalStrength feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsSignalStrengthVisible
        {
            get { return (bool)GetValue(IsSignalStrengthVisibleProperty); }
            set { SetValue(IsSignalStrengthVisibleProperty, value); }
        }
        #endregion

        #region IsResolutionIndicatorVisible
        /// <summary>
        /// Identifies the IsResolutionIndicatorVisible dependency property.
        /// </summary>
        public static DependencyProperty IsResolutionIndicatorVisibleProperty { get { return isResolutionIndicatorVisibleProperty; } }
        static readonly DependencyProperty isResolutionIndicatorVisibleProperty = RegisterDependencyProperty<bool>("IsResolutionIndicatorVisible", (t, o, n) => t.OnIsResolutionIndicatorVisibleChanged(), false);

        void OnIsResolutionIndicatorVisibleChanged()
        {
            if (IsResolutionIndicatorVisibleChanged != null) IsResolutionIndicatorVisibleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the IsResolutionIndicatorVisible property changes.
        /// </summary>
#if SILVERLIGHT
#if SILVERLIGHT
        public event EventHandler IsResolutionIndicatorVisibleChanged;
#else
        public event EventHandler<object> IsResolutionIndicatorVisibleChanged;
#endif
#else
        public event EventHandler<object> IsResolutionIndicatorVisibleChanged;
#endif

        /// <summary>
        /// Gets or sets if the interactive SignalStrength feature should be visible and therefore available for the user to control.
        /// </summary>
        public bool IsResolutionIndicatorVisible
        {
            get { return (bool)GetValue(IsResolutionIndicatorVisibleProperty); }
            set { SetValue(IsResolutionIndicatorVisibleProperty, value); }
        }
        #endregion

        #endregion

        /// <inheritdoc /> 
        MediaPlayer IMediaSource.Player { get { return this; } }

        #region TimeFormatConverter
        /// <summary>
        /// Identifies the TimeFormatConverter dependency property.
        /// </summary>
        public static DependencyProperty TimeFormatConverterProperty { get { return timeFormatConverterProperty; } }
        static readonly DependencyProperty timeFormatConverterProperty = RegisterDependencyProperty<IValueConverter>("TimeFormatConverter", (t, o, n) => t.OnTimeFormatConverterChanged(o, n));

        void OnTimeFormatConverterChanged(IValueConverter oldValue, IValueConverter newValue)
        {
            OnTimeFormatConverterChanged(new TimeFormatConverterChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets or sets a an IValueConverter that is used to display the time to the user such as the position, duration, and time remaining.
        /// The default value applies the string format of "h\\:mm\\:ss".
        /// </summary>
        public IValueConverter TimeFormatConverter
        {
            get { return (IValueConverter)GetValue(TimeFormatConverterProperty); }
            set { SetValue(TimeFormatConverterProperty, value); }
        }

        /// <summary>
        /// The default TimeFormat string to use to display position, time remaining and duration.
        /// </summary>
        public static string DefaultTimeFormat
        {
            get
            {
#if SILVERLIGHT
                return PlayerFramework.Resources.TimeSpanReadableFormat;
#else
                if (!IsInDesignMode)
                {
                    return GetResourceString("TimeSpanReadableFormat");
                }
                else
                {
                    return @"h\:mm\:ss";
                }
#endif
            }
        }
        #endregion

        #region SkipBackInterval
        /// <summary>
        /// Identifies the SkipBackInterval dependency property.
        /// </summary>
        public static DependencyProperty SkipBackIntervalProperty { get { return skipBackIntervalProperty; } }
        static readonly DependencyProperty skipBackIntervalProperty = RegisterDependencyProperty<TimeSpan?>("SkipBackInterval", (t, o, n) => t.OnSkipBackIntervalChanged(o, n), DefaultSkipBackInterval);

        void OnSkipBackIntervalChanged(TimeSpan? oldValue, TimeSpan? newValue)
        {
            OnSkipBackIntervalChanged(new SkipBackIntervalChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets or sets the amount of time in the video to skip back when the user selects skip back.
        /// This can be set to null to cause the skip back action to go back to the beginning.
        /// The default is 30 seconds although it will never go back past the beginning.
        /// </summary>
        public TimeSpan? SkipBackInterval
        {
            get { return (TimeSpan?)GetValue(SkipBackIntervalProperty); }
            set { SetValue(SkipBackIntervalProperty, value); }
        }

        /// <summary>
        /// The default SkipBackInterval value.
        /// </summary>
        public static TimeSpan DefaultSkipBackInterval
        {
            get
            {
#if WINDOWS_PHONE
                return TimeSpan.FromSeconds(7);
#else
                return TimeSpan.FromSeconds(30);
#endif
            }
        }
        #endregion

        #region SkipAheadInterval
        /// <summary>
        /// Identifies the SkipAheadInterval dependency property.
        /// </summary>
        public static DependencyProperty SkipAheadIntervalProperty { get { return skipAheadIntervalProperty; } }
        static readonly DependencyProperty skipAheadIntervalProperty = RegisterDependencyProperty<TimeSpan?>("SkipAheadInterval", (t, o, n) => t.OnSkipAheadIntervalChanged(o, n), DefaultSkipAheadInterval);

        void OnSkipAheadIntervalChanged(TimeSpan? oldValue, TimeSpan? newValue)
        {
            OnSkipAheadIntervalChanged(new SkipAheadIntervalChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets or sets the amount of time in the video to skip ahead when the user selects skip ahead.
        /// This can be set to null to cause the skip ahead action to go directly to the end.
        /// The default is 30 seconds although it will never go past the end (or MaxPosition if set).
        /// </summary>
        public TimeSpan? SkipAheadInterval
        {
            get { return (TimeSpan?)GetValue(SkipAheadIntervalProperty); }
            set { SetValue(SkipAheadIntervalProperty, value); }
        }

        /// <summary>
        /// The default SkipAheadInterval value.
        /// </summary>
        public static TimeSpan DefaultSkipAheadInterval
        {
            get { return TimeSpan.FromSeconds(30); }
        }
        #endregion

        #region VisualMarkers
        /// <summary>
        /// Identifies the TimelineMarkers dependency property.
        /// </summary>
        public static DependencyProperty VisualMarkersProperty { get { return visualMarkersProperty; } }
        static readonly DependencyProperty visualMarkersProperty = RegisterDependencyProperty<IList<VisualMarker>>("VisualMarkers");

        /// <summary>
        /// Gets or sets the collection of markers to be displayed in the timeline.
        /// </summary>
        public IList<VisualMarker> VisualMarkers
        {
            get { return GetValue(VisualMarkersProperty) as IList<VisualMarker>; }
        }
        #endregion

        #region Markers

        /// <summary>
        /// Gets the collection of timeline markers associated with the currently loaded media file.
        /// </summary>
        public TimelineMarkerCollection Markers { get { return _Markers; } }

        #endregion

        #region AutoLoad
        /// <summary>
        /// Identifies the AutoLoad dependency property.
        /// </summary>
        public static DependencyProperty AutoLoadProperty { get { return autoLoadProperty; } }
        static readonly DependencyProperty autoLoadProperty = RegisterDependencyProperty<bool>("AutoLoad", (t, o, n) => t.OnAutoLoadChanged(n), true);

        void OnAutoLoadChanged(bool newValue)
        {
            if (newValue && PendingLoadAction != null)
            {
                PendingLoadAction();
                PendingLoadAction = null;
            }
        }

        /// <summary>
        /// Gets or sets a gate for loading the source. Setting this to false postpones any subsequent calls to the Source property and SetSource method.
        /// Once the source is set on the underlying MediaElement, the media begins to download.
        /// Note: There is another opportunity to block setting the source by using the awaitable BeforeMediaLoaded event.
        /// </summary>
        public bool AutoLoad
        {
            get { return (bool)GetValue(AutoLoadProperty); }
            set { SetValue(AutoLoadProperty, value); }
        }
        #endregion

        #region SignalStrength
        /// <summary>
        /// Identifies the SignalStrength dependency property.
        /// </summary>
        public static DependencyProperty SignalStrengthProperty { get { return signalStrengthProperty; } }
        static readonly DependencyProperty signalStrengthProperty = RegisterDependencyProperty<double>("SignalStrength", (t, o, n) => t.OnSignalStrengthChanged(o, n), 0.0);

        void OnSignalStrengthChanged(double oldValue, double newValue)
        {
            OnSignalStrengthChanged(new SignalStrengthChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets or sets the signal strength used to indicate visually to the user the quality of the bitrate.
        /// This is only useful for adaptive streaming and is only displayed when IsSignalStrengthVisible = true
        /// </summary>
        public double SignalStrength
        {
            get { return (double)GetValue(SignalStrengthProperty); }
            set { SetValue(SignalStrengthProperty, value); }
        }
        #endregion

        #region MediaQuality
        /// <summary>
        /// Identifies the MediaQuality dependency property.
        /// </summary>
        public static DependencyProperty MediaQualityProperty { get { return mediaQualityProperty; } }
        static readonly DependencyProperty mediaQualityProperty = RegisterDependencyProperty<MediaQuality>("MediaQuality", (t, o, n) => t.OnMediaQualityChanged(o, n), MediaQuality.StandardDefinition);

        void OnMediaQualityChanged(MediaQuality oldValue, MediaQuality newValue)
        {
            OnMediaQualityChanged(new MediaQualityChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets or sets an enum indicating the quality or resolution of the media. This does not affect the actual quality and only offers visual indication to the end-user when IsResolutionIndicatorVisible is true.
        /// </summary>
        public MediaQuality MediaQuality
        {
            get { return (MediaQuality)GetValue(MediaQualityProperty); }
            set { SetValue(MediaQualityProperty, value); }
        }
        #endregion

        #region LivePositionBuffer
        /// <summary>
        /// Identifies the LivePositionBuffer dependency property.
        /// </summary>
        public static DependencyProperty LivePositionBufferProperty { get { return livePositionBufferProperty; } }
        static readonly DependencyProperty livePositionBufferProperty = RegisterDependencyProperty<TimeSpan>("LivePositionBuffer", (t, o, n) => t.OnLivePositionBufferChanged(o, n), TimeSpan.FromSeconds(10));

        void OnLivePositionBufferChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            UpdateIsPositionLive();
        }

        void UpdateIsPositionLive()
        {
            var liveThreshold = IsPositionLive
                           ? TimeSpan.FromSeconds(LivePositionBuffer.TotalSeconds + (LivePositionBuffer.TotalSeconds * .1))
                           : TimeSpan.FromSeconds(LivePositionBuffer.TotalSeconds - (LivePositionBuffer.TotalSeconds * .1));

            IsPositionLive = LivePosition.HasValue && LivePosition.Value.Subtract(Position) < liveThreshold;
        }

        /// <summary>
        /// Gets or sets a value indicating what the tollerance is for determining whether or not the current position is live. IsPositionLive is affected by this property.
        /// </summary>
        public TimeSpan LivePositionBuffer
        {
            get { return (TimeSpan)GetValue(LivePositionBufferProperty); }
            set { SetValue(LivePositionBufferProperty, value); }
        }
        #endregion

        #region IsPositionLive
        /// <summary>
        /// Identifies the IsPositionLive dependency property.
        /// </summary>
        public static DependencyProperty IsPositionLiveProperty { get { return isPositionLiveProperty; } }
        static readonly DependencyProperty isPositionLiveProperty = RegisterDependencyProperty<bool>("IsPositionLive", (t, o, n) => t.OnIsPositionLiveChanged(o, n), false);

        void OnIsPositionLiveChanged(bool oldValue, bool newValue)
        {
            NotifyIsGoLiveAllowedChanged();
        }

        /// <summary>
        /// Gets or sets a value indicating what the tollerance is for determining whether or not the current position is live. IsPositionLive is affected by this property.
        /// </summary>
        public bool IsPositionLive
        {
            get { return (bool)GetValue(IsPositionLiveProperty); }
            private set { SetValue(IsPositionLiveProperty, value); }
        }
        #endregion

        #region LivePosition
        /// <summary>
        /// Identifies the LivePosition dependency property.
        /// </summary>
        public static DependencyProperty LivePositionProperty { get { return livePositionProperty; } }
        static readonly DependencyProperty livePositionProperty = RegisterDependencyProperty<TimeSpan?>("LivePosition", (t, o, n) => t.OnLivePositionChanged(o, n), (TimeSpan?)null);

        void OnLivePositionChanged(TimeSpan? oldValue, TimeSpan? newValue)
        {
            UpdateIsPositionLive();
            OnLivePositionChanged(new LivePositionChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets or sets the Live position for realtime/live playback.
        /// </summary>
        public TimeSpan? LivePosition
        {
            get { return (TimeSpan?)GetValue(LivePositionProperty); }
            set { SetValue(LivePositionProperty, value); }
        }
        #endregion

        #region Duration
        /// <summary>
        /// Identifies the Duration dependency property.
        /// </summary>
        public static DependencyProperty DurationProperty { get { return durationProperty; } }
        static readonly DependencyProperty durationProperty = RegisterDependencyProperty<TimeSpan>("Duration", (t, o, n) => t.OnDurationChanged(o, n), TimeSpan.Zero);

        void OnDurationChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            OnDurationChanged(new DurationChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets the duration of the current video or audio. For VOD, this is automatically set from the NaturalDuration property.
        /// </summary>
        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
        }
        #endregion

        #region IsStartTimeOffset
        /// <summary>
        /// Identifies the IsStartTimeOffset dependency property.
        /// </summary>
        public static DependencyProperty IsStartTimeOffsetProperty { get { return isStartTimeOffsetProperty; } }
        static readonly DependencyProperty isStartTimeOffsetProperty = RegisterDependencyProperty<bool>("IsStartTimeOffset", false);

        /// <summary>
        /// Gets or sets the IsStartTimeOffset of the current video or audio. For VOD, this is automatically set from the NaturalIsStartTimeOffset property.
        /// </summary>
        public bool IsStartTimeOffset
        {
            get { return (bool)GetValue(IsStartTimeOffsetProperty); }
            set { SetValue(IsStartTimeOffsetProperty, value); }
        }
        #endregion

        #region StartTime
        /// <summary>
        /// Identifies the StartTime dependency property.
        /// </summary>
        public static DependencyProperty StartTimeProperty { get { return startTimeProperty; } }
        static readonly DependencyProperty startTimeProperty = RegisterDependencyProperty<TimeSpan>("StartTime", (t, o, n) => t.OnStartTimeChanged(o, n), TimeSpan.Zero);

        void OnStartTimeChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            SetValue(DurationProperty, EndTime.Subtract(newValue));
            OnStartTimeChanged(new StartTimeChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets or sets the StartTime of the current video or audio. For VOD, this is automatically set from the NaturalStartTime property.
        /// </summary>
        public TimeSpan StartTime
        {
            get { return (TimeSpan)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }
        #endregion

        #region EndTime
        /// <summary>
        /// Identifies the EndTime dependency property.
        /// </summary>
        public static DependencyProperty EndTimeProperty { get { return endTimeProperty; } }
        static readonly DependencyProperty endTimeProperty = RegisterDependencyProperty<TimeSpan>("EndTime", (t, o, n) => t.OnEndTimeChanged(o, n), TimeSpan.Zero);

        void OnEndTimeChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            SetValue(DurationProperty, newValue.Subtract(StartTime));
            SetValue(TimeRemainingProperty, TimeSpanExtensions.Max(newValue.Subtract(Position), TimeSpan.Zero));
            OnEndTimeChanged(new EndTimeChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets or sets the EndTime of the current media. For progressive video, this is automatically set from Duration - StartTime.
        /// </summary>
        public TimeSpan EndTime
        {
            get { return (TimeSpan)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }
        #endregion

        #region TimeRemaining
        /// <summary>
        /// Identifies the TimeRemaining dependency property.
        /// </summary>
        public static DependencyProperty TimeRemainingProperty { get { return timeRemainingProperty; } }
        static readonly DependencyProperty timeRemainingProperty = RegisterDependencyProperty<TimeSpan>("TimeRemaining", (t, o, n) => t.OnTimeRemainingChanged(o, n), TimeSpan.Zero);

        void OnTimeRemainingChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            OnTimeRemainingChanged(new TimeRemainingChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets the time remaining before the media will finish. This is calculated automatically whenever the Position or Duration properties change.
        /// </summary>
        public TimeSpan TimeRemaining
        {
            get { return (TimeSpan)GetValue(TimeRemainingProperty); }
        }
        #endregion

        #region SeekWhileScrubbing
        /// <summary>
        /// Identifies the SeekWhileScrubbing dependency property.
        /// </summary>
        public static DependencyProperty SeekWhileScrubbingProperty { get { return seekWhileScrubbingProperty; } }
        static readonly DependencyProperty seekWhileScrubbingProperty = RegisterDependencyProperty<bool>("SeekWhileScrubbing", true);

        /// <summary>
        /// Gets or sets whether or not the position should change while the user is actively scrubbing. If false, media will be paused until the user has finished scrubbing.
        /// </summary>
        public bool SeekWhileScrubbing
        {
            get { return (bool)GetValue(SeekWhileScrubbingProperty); }
            set { SetValue(SeekWhileScrubbingProperty, value); }
        }
        #endregion

        #region ReplayOffset
        /// <summary>
        /// Identifies the ReplayOffset dependency property.
        /// </summary>
        public static DependencyProperty ReplayOffsetProperty { get { return replayOffsetProperty; } }
        static readonly DependencyProperty replayOffsetProperty = RegisterDependencyProperty<TimeSpan>("ReplayOffset", TimeSpan.FromSeconds(5));

        /// <summary>
        /// Gets or sets the amount of time to reset the current play position back for an instant replay. Default 5 seconds.
        /// </summary>
        public TimeSpan ReplayOffset
        {
            get { return (TimeSpan)GetValue(ReplayOffsetProperty); }
            set { SetValue(ReplayOffsetProperty, value); }
        }
        #endregion

        #region SlowMotionPlaybackRate
        /// <summary>
        /// Identifies the SlowMotionPlaybackRate dependency property.
        /// </summary>
        public static DependencyProperty SlowMotionPlaybackRateProperty { get { return slowMotionPlaybackRateProperty; } }
        static readonly DependencyProperty slowMotionPlaybackRateProperty = RegisterDependencyProperty<double>("SlowMotionPlaybackRate", .25);

        /// <summary>
        /// Gets or sets the playback rate when operating in slow motion (IsSlowMotion). Default .25
        /// </summary>
        public double SlowMotionPlaybackRate
        {
            get { return (double)GetValue(SlowMotionPlaybackRateProperty); }
            set { SetValue(SlowMotionPlaybackRateProperty, value); }
        }
        #endregion

        #region IsSlowMotion
        /// <summary>
        /// Identifies the IsSlowMotion dependency property.
        /// </summary>
        public static DependencyProperty IsSlowMotionProperty { get { return isSlowMotionProperty; } }
        static readonly DependencyProperty isSlowMotionProperty = RegisterDependencyProperty<bool>("IsSlowMotion", (t, o, n) => t.OnIsSlowMotionChanged(o, n), false);

        void OnIsSlowMotionChanged(bool oldValue, bool newValue)
        {
            if (newValue) PlaybackRate = SlowMotionPlaybackRate;
            else PlaybackRate = DefaultPlaybackRate;
            OnIsSlowMotionChanged(new IsSlowMotionChangedEventArgs());
        }

        /// <summary>
        /// Gets or sets whether or not the media is playing in slow motion.
        /// The slow motion playback rate is defined by the SlowMotionPlaybackRate property.
        /// </summary>
        public bool IsSlowMotion
        {
            get { return (bool)GetValue(IsSlowMotionProperty); }
            set { SetValue(IsSlowMotionProperty, value); }
        }
        #endregion

        #region IsCaptionsActive
        /// <summary>
        /// Identifies the IsCaptionsActive dependency property.
        /// </summary>
        public static DependencyProperty IsCaptionsActiveProperty { get { return isCaptionsActiveProperty; } }
        static readonly DependencyProperty isCaptionsActiveProperty = RegisterDependencyProperty<bool>("IsCaptionsActive", (t, o, n) => t.OnIsCaptionsActiveChanged(o, n), false);

        void OnIsCaptionsActiveChanged(bool oldValue, bool newValue)
        {
            _IsCaptionsActive = newValue;
            OnIsCaptionsActiveChanged(new IsCaptionsActiveChangedEventArgs());
        }

        /// <summary>
        /// Gets or sets if the player should show the captions configuration window.
        /// </summary>
        public bool IsCaptionsActive
        {
            get { return (bool)GetValue(IsCaptionsActiveProperty); }
            set { SetValue(IsCaptionsActiveProperty, value); }
        }
        #endregion

        #region IsFullScreen
        /// <summary>
        /// Identifies the IsFullScreen dependency property.
        /// </summary>
        public static DependencyProperty IsFullScreenProperty { get { return isFullScreenProperty; } }
        static readonly DependencyProperty isFullScreenProperty = RegisterDependencyProperty<bool>("IsFullScreen", (t, o, n) => t.OnIsFullScreenChanged(o, n), false);

        void OnIsFullScreenChanged(bool oldValue, bool newValue)
        {
            if (!newValue && oldValue)
            {
#if SILVERLIGHT
                this.Cursor = IsInteractive ? Cursors.Arrow : Cursors.None;
#endif
            }
            _IsFullScreen = newValue;
            OnIsFullScreenChanged(new IsFullScreenChangedEventArgs());
        }

        /// <summary>
        /// Gets or sets if the player should indicate it is in fullscreen mode.
        /// </summary>
        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }
        #endregion

        #region AdvertisingState
        /// <summary>
        /// Identifies the AdvertisingState dependency property.
        /// </summary>
        public static DependencyProperty AdvertisingStateProperty { get { return advertisingStateProperty; } }
        static readonly DependencyProperty advertisingStateProperty = RegisterDependencyProperty<AdvertisingState>("AdvertisingState", (t, o, n) => t.OnAdvertisingStateChanged(o, n), AdvertisingState.None);

        void OnAdvertisingStateChanged(AdvertisingState oldValue, AdvertisingState newValue)
        {
            _AdvertisingState = newValue;
            OnAdvertisingStateChanged(new AdvertisingStateChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets or sets if the player should indicate it is in Advertising mode.
        /// </summary>
        public AdvertisingState AdvertisingState
        {
            get { return (AdvertisingState)GetValue(AdvertisingStateProperty); }
            set { SetValue(AdvertisingStateProperty, value); }
        }
        #endregion

        #region IsScrubbing
        /// <summary>
        /// Identifies the IsScrubbing dependency property.
        /// </summary>
        public static DependencyProperty IsScrubbingProperty { get { return isScrubbingProperty; } }
        static readonly DependencyProperty isScrubbingProperty = RegisterDependencyProperty<bool>("IsScrubbing", false);

        /// <summary>
        /// Gets whether or not the user is actively scrubbing.
        /// </summary>
        public bool IsScrubbing
        {
            get { return (bool)GetValue(IsScrubbingProperty); }
        }
        #endregion

        #region StartupPosition
        /// <summary>
        /// Identifies the StartupPosition dependency property.
        /// </summary>
        public static DependencyProperty StartupPositionProperty { get { return startupPositionProperty; } }
        static readonly DependencyProperty startupPositionProperty = RegisterDependencyProperty<TimeSpan?>("StartupPosition", (TimeSpan?)null);

        /// <summary>
        /// Gets or sets the position at which to start the video at. This is useful for resuming videos at the place they were left off at.
        /// </summary>
        public TimeSpan? StartupPosition
        {
            get { return (TimeSpan?)GetValue(StartupPositionProperty); }
            set { SetValue(StartupPositionProperty, value); }
        }
        #endregion

        #region MediaEndedBehavior
        /// <summary>
        /// Identifies the MediaEndedBehavior dependency property.
        /// </summary>
        public static DependencyProperty MediaEndedBehaviorProperty { get { return mediaEndedBehaviorProperty; } }
        static readonly DependencyProperty mediaEndedBehaviorProperty = RegisterDependencyProperty<MediaEndedBehavior>("MediaEndedBehavior", MediaEndedBehavior.Stop);

        /// <summary>
        /// Gets or sets the desired behavior when the media reaches the end.
        /// Note: This will be ignored if IsLooping = true.
        /// </summary>
        public MediaEndedBehavior MediaEndedBehavior
        {
            get { return (MediaEndedBehavior)GetValue(MediaEndedBehaviorProperty); }
            set { SetValue(MediaEndedBehaviorProperty, value); }
        }
        #endregion

        #region UpdateInterval

        /// <summary>
        /// Identifies the UpdateInterval dependency property.
        /// </summary>
        public static DependencyProperty UpdateIntervalProperty { get { return updateIntervalProperty; } }
        static readonly DependencyProperty updateIntervalProperty = RegisterDependencyProperty<TimeSpan>("UpdateInterval", (t, o, n) => t.OnUpdateIntervalChanged(n), TimeSpan.FromMilliseconds(250));

        void OnUpdateIntervalChanged(TimeSpan newValue)
        {
            UpdateTimer.Interval = newValue;
        }

        /// <summary>
        /// Gets or sets the interval that the timeline and other properties affected by the position will change.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get { return (TimeSpan)GetValue(UpdateIntervalProperty); }
            set { SetValue(UpdateIntervalProperty, value); }
        }

        void UpdateTimer_Tick(object sender, object e)
        {
            OnUpdate();
        }

        #endregion

        #region AvailableCaptions
        /// <summary>
        /// Identifies the AvailableCaptions dependency property.
        /// </summary>
        public static DependencyProperty AvailableCaptionsProperty { get { return availableCaptionsProperty; } }
        static readonly DependencyProperty availableCaptionsProperty = RegisterDependencyProperty<IList<ICaption>>("AvailableCaptions");

        /// <summary>
        /// Gets or sets the list of captions that can be chosen by the user.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Can be set from xaml")]
        public IList<ICaption> AvailableCaptions
        {
            get { return GetValue(AvailableCaptionsProperty) as IList<ICaption>; }
        }
        #endregion

        #region SelectedCaption
        /// <summary>
        /// Identifies the SelectedCaption dependency property.
        /// </summary>
        public static DependencyProperty SelectedCaptionProperty { get { return selectedCaptionProperty; } }
        static readonly DependencyProperty selectedCaptionProperty = RegisterDependencyProperty<ICaption>("SelectedCaption", (t, o, n) => t.OnSelectedCaptionChanged(o, n));

        void OnSelectedCaptionChanged(ICaption oldValue, ICaption newValue)
        {
            OnSelectedCaptionChanged(new SelectedCaptionChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets or sets the selected caption stream.
        /// </summary>
        public ICaption SelectedCaption
        {
            get { return GetValue(SelectedCaptionProperty) as ICaption; }
            set { SetValue(SelectedCaptionProperty, value); }
        }
        #endregion

        #region AvailableAudioStreams
        /// <summary>
        /// Identifies the AvailableAudioStreams dependency property.
        /// </summary>
        public static DependencyProperty AvailableAudioStreamsProperty { get { return availableAudioStreamsProperty; } }
        static readonly DependencyProperty availableAudioStreamsProperty = RegisterDependencyProperty<IList<IAudioStream>>("AvailableAudioStreams");

        /// <summary>
        /// Gets or sets the list of AudioStreams that can be chosen by the user.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Can be set from xaml")]
        public IList<IAudioStream> AvailableAudioStreams
        {
            get { return GetValue(AvailableAudioStreamsProperty) as IList<IAudioStream>; }
        }
        #endregion

        #region SelectedAudioStream
        /// <summary>
        /// Identifies the SelectedAudioStream dependency property.
        /// </summary>
        public static DependencyProperty SelectedAudioStreamProperty { get { return selectedAudioStreamProperty; } }
        static readonly DependencyProperty selectedAudioStreamProperty = RegisterDependencyProperty<IAudioStream>("SelectedAudioStream", (t, o, n) => t.OnSelectedAudioStreamChanged(o, n));

        void OnSelectedAudioStreamChanged(IAudioStream oldValue, IAudioStream newValue)
        {
            var eventArgs = new SelectedAudioStreamChangedEventArgs(oldValue, newValue);
            OnSelectedAudioStreamChanged(eventArgs);
            if (!eventArgs.Handled)
            {
                AudioStreamIndex = AvailableAudioStreams.IndexOf(newValue);
            }
        }

        /// <summary>
        /// Gets or sets the selected AudioStream stream.
        /// </summary>
        public IAudioStream SelectedAudioStream
        {
            get { return GetValue(SelectedAudioStreamProperty) as IAudioStream; }
            set { SetValue(SelectedAudioStreamProperty, value); }
        }
        #endregion

        #region IsLive
        /// <summary>
        /// Identifies the IsLive dependency property.
        /// </summary>
        public static DependencyProperty IsLiveProperty { get { return isLiveProperty; } }
        static readonly DependencyProperty isLiveProperty = RegisterDependencyProperty<bool>("IsLive", (t, o, n) => t.OnIsLiveChanged(o, n), false);

        void OnIsLiveChanged(bool oldValue, bool newValue)
        {
            OnIsLiveChanged(new IsLiveChangedEventArgs());
            NotifyIsGoLiveAllowedChanged();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the media is Live (vs. VOD).
        /// </summary>
        public bool IsLive
        {
            get { return (bool)GetValue(IsLiveProperty); }
            set { SetValue(IsLiveProperty, value); }
        }
        #endregion

#if SILVERLIGHT

        #region BufferingTime
        /// <summary>
        /// Identifies the BufferingTime dependency property.
        /// </summary>
        public static DependencyProperty BufferingTimeProperty { get { return bufferingTimeProperty; } }
        static readonly DependencyProperty bufferingTimeProperty = RegisterDependencyProperty<TimeSpan>("BufferingTime", (t, o, n) => t.OnBufferingTimeChanged(n), DefaultBufferingTime);

        void OnBufferingTimeChanged(TimeSpan newValue)
        {
            _BufferingTime = newValue;
        }

        /// <summary>
        /// Gets or sets the amount of time to buffer. The default value is the recommended value for optimal performance.
        /// </summary>
        public TimeSpan BufferingTime
        {
            get { return (TimeSpan)GetValue(BufferingTimeProperty); }
            set { SetValue(BufferingTimeProperty, value); }
        }
        #endregion

        #region LicenseAcquirer

        /// <summary>
        /// Gets or sets the System.Windows.Media.LicenseAcquirer associated with the MediaElement. The LicenseAcquirer handles acquiring licenses for DRM encrypted content.
        /// </summary>
        public LicenseAcquirer LicenseAcquirer
        {
            get { return _LicenseAcquirer; }
            set { _LicenseAcquirer = value; }
        }

        #endregion

#if !WINDOWS_PHONE
        #region Attributes
        /// <summary>
        /// Identifies the Attributes dependency property.
        /// </summary>
        public static DependencyProperty AttributesProperty { get { return attributesProperty; } }
        static readonly DependencyProperty attributesProperty = RegisterDependencyProperty<Dictionary<string, string>>("Attributes", DefaultAttributes);

        /// <summary>
        /// Gets the collection of attributes that corresponds to the current entry in the ASX file that Source is set to.
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get { return (Dictionary<string, string>)GetValue(AttributesProperty); }
        }
        #endregion

        #region IsDecodingOnGPU
        /// <summary>
        /// Identifies the IsDecodingOnGPU dependency property.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "MediaElement compatibility")]
        public static DependencyProperty IsDecodingOnGPUProperty { get { return isDecodingOnGPUProperty; } }
        static readonly DependencyProperty isDecodingOnGPUProperty = RegisterDependencyProperty<bool>("IsDecodingOnGPU", DefaultIsDecodingOnGPU);


        /// <summary>
        /// Gets a value that indicates whether the media is being decoded in hardware.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "MediaElement compatibility")]
        public bool IsDecodingOnGPU
        {
            get { return (bool)GetValue(IsDecodingOnGPUProperty); }
        }
        #endregion
#endif

        #region DroppedFramesPerSecond
        /// <summary>
        /// Identifies the DroppedFramesPerSecond dependency property.
        /// </summary>
        public static DependencyProperty DroppedFramesPerSecondProperty { get { return droppedFramesPerSecondProperty; } }
        static readonly DependencyProperty droppedFramesPerSecondProperty = RegisterDependencyProperty<double>("DroppedFramesPerSecond", DefaultDroppedFramesPerSecond);

        /// <summary>
        /// Gets the number of frames per second being dropped by the media.
        /// </summary>
        public double DroppedFramesPerSecond
        {
            get { return (double)GetValue(DroppedFramesPerSecondProperty); }
        }
        #endregion

        #region RenderedFramesPerSecond

        /// <summary>
        /// Identifies the RenderedFramesPerSecond dependency property.
        /// </summary>
        public static DependencyProperty RenderedFramesPerSecondProperty { get { return renderedFramesPerSecondProperty; } }
        static readonly DependencyProperty renderedFramesPerSecondProperty = RegisterDependencyProperty<double>("RenderedFramesPerSecond", DefaultRenderedFramesPerSecond);

        /// <summary>
        /// Gets the number of frames per second being rendered by the media.
        /// </summary>
        public double RenderedFramesPerSecond
        {
            get { return (double)GetValue(RenderedFramesPerSecondProperty); }
        }

        #endregion

        #region DefaultPlaybackRate
        /// <summary>
        /// Identifies the DefaultPlaybackRate dependency property.
        /// </summary>
        public static DependencyProperty DefaultPlaybackRateProperty { get { return defaultPlaybackRateProperty; } }
        static readonly DependencyProperty defaultPlaybackRateProperty = RegisterDependencyProperty<double>("DefaultPlaybackRate", 1.0);

        /// <summary>
        /// Gets or sets the default playback rate for the media. The playback rate applies when the user is not using fast forward or reverse.
        /// </summary>
        public double DefaultPlaybackRate
        {
            get { return (double)GetValue(DefaultPlaybackRateProperty); }
            set { SetValue(DefaultPlaybackRateProperty, value); }
        }
        #endregion

        #region IsLooping
        /// <summary>
        /// Identifies the IsLooping dependency property.
        /// </summary>
        public static DependencyProperty IsLoopingProperty { get { return isLoopingProperty; } }
        static readonly DependencyProperty isLoopingProperty = RegisterDependencyProperty<bool>("IsLooping", false);

        /// <summary>
        /// Gets or sets a value that describes whether the media source should seek to the start after reaching its end. Set to true to loop the media and play continuously.
        /// If set to true, MediaEndedBehavior will have no effect.
        /// </summary>
        public bool IsLooping
        {
            get { return (bool)GetValue(IsLoopingProperty); }
            set { SetValue(IsLoopingProperty, value); }
        }

        #endregion

        #region PosterSource
        /// <summary>
        /// Identifies the PosterSource dependency property.
        /// </summary>
        public static DependencyProperty PosterSourceProperty { get { return posterSourceProperty; } }
        static readonly DependencyProperty posterSourceProperty = RegisterDependencyProperty<ImageSource>("PosterSource");

        /// <summary>
        /// Gets or sets an ImageSource to be displayed before the content is loaded. Only shows until MediaOpened fires and is hidden when the first frame of the video is available.
        /// Note: This will not show when waiting for AutoPlay to be set to true since MediaOpened still fires.
        /// </summary>
        public ImageSource PosterSource
        {
            get { return (ImageSource)GetValue(PosterSourceProperty); }
            set { SetValue(PosterSourceProperty, value); }
        }

        #endregion

        #region Stretch
        /// <summary>
        /// Identifies the Stretch dependency property.
        /// </summary>
        public static DependencyProperty StretchProperty { get { return stretchProperty; } }
        static readonly DependencyProperty stretchProperty = RegisterDependencyProperty<Stretch>("Stretch", (t, o, n) => t.OnStretchChanged(o, n), DefaultStretch);

        void OnStretchChanged(Stretch oldValue, Stretch newValue)
        {
            _Stretch = newValue;
            OnStretchChanged(new RoutedPropertyChangedEventArgs<Stretch>(oldValue, newValue));
        }

        /// <summary>
        /// Gets or sets a Stretch value that describes how to fill the destination rectangle. The default is Uniform.
        /// You can also cycle through the enumerations by calling the CycleDisplayMode method.
        /// </summary>
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }
        #endregion
#else

        #region TestForMediaPack

        /// <summary>
        /// Identifies the AspectRatioWidth�dependency property.
        /// </summary>
        public static DependencyProperty TestForMediaPackProperty { get { return testForMediaPackProperty; } }
        static readonly DependencyProperty testForMediaPackProperty = RegisterDependencyProperty<bool>("TestForMediaPack", true);

        /// <summary>
        /// Gets or sets whether a test for the media feature pack should be performed prior to allowing content to be laoded. This is useful to enable if Windows 8 N/KN users will be using this app.
        /// </summary>
        public bool TestForMediaPack
        {
            get { return (bool)GetValue(TestForMediaPackProperty); }
            set { SetValue(TestForMediaPackProperty, value); }
        }

        #endregion

        #region MediaExtensionManager
        /// <summary>
        /// Gets or sets the MediaExtensionManager to be used by PlayerFramework plugins.
        /// One will be created on first use if it is not set.
        /// </summary>
        public MediaExtensionManager MediaExtensionManager
        {
            get
            {
                if (mediaExtensionManager == null)
                {
                    mediaExtensionManager = new Windows.Media.MediaExtensionManager();
                }
                return mediaExtensionManager;
            }
            set
            {
                mediaExtensionManager = value;
            }
        }
        #endregion

        #region AspectRatioWidth

        /// <summary>
        /// Identifies the AspectRatioWidth�dependency property.
        /// </summary>
        public static DependencyProperty AspectRatioWidthProperty { get { return aspectRatioWidthProperty; } }
        static readonly DependencyProperty aspectRatioWidthProperty = RegisterDependencyProperty<int>("AspectRatioWidth", DefaultAspectRatioWidth);

        /// <summary>
        /// Gets the width portion of the media's native aspect ratio.
        /// </summary>
        public int AspectRatioWidth
        {
            get { return (int)GetValue(AspectRatioWidthProperty); }
        }

        #endregion

        #region AspectRatioHeight

        /// <summary>
        /// Identifies the AspectRatioHeight�dependency property.
        /// </summary>
        public static DependencyProperty AspectRatioHeightProperty { get { return aspectRatioHeightProperty; } }
        static readonly DependencyProperty aspectRatioHeightProperty = RegisterDependencyProperty<int>("AspectRatioHeight", DefaultAspectRatioHeight);

        /// <summary>
        /// Gets the height portion of the media's native aspect ratio.
        /// </summary>
        public int AspectRatioHeight
        {
            get { return (int)GetValue(AspectRatioHeightProperty); }
        }

        #endregion

        #region AudioCategory

        /// <summary>
        /// Identifies the AudioCategory dependency property.
        /// </summary>
        public static DependencyProperty AudioCategoryProperty { get { return audioCategoryProperty; } }
        static readonly DependencyProperty audioCategoryProperty = RegisterDependencyProperty<AudioCategory>("AudioCategory", (t, o, n) => t.OnAudioCategoryChanged(n), DefaultAudioCategory);

        void OnAudioCategoryChanged(AudioCategory newValue)
        {
            _AudioCategory = newValue;
        }

        /// <summary>
        /// Gets or sets a value that describes the purpose of the audio information in an audio stream.
        /// </summary>
        public AudioCategory AudioCategory
        {
            get { return (AudioCategory)GetValue(AudioCategoryProperty); }
            set { SetValue(AudioCategoryProperty, value); }
        }

        #endregion

        #region AudioDeviceType

        /// <summary>
        /// Identifies the AudioDeviceType dependency property.
        /// </summary>
        public static DependencyProperty AudioDeviceTypeProperty { get { return audioDeviceTypeProperty; } }
        static readonly DependencyProperty audioDeviceTypeProperty = RegisterDependencyProperty<AudioDeviceType>("AudioDeviceType", (t, o, n) => t.OnAudioDeviceTypeChanged(n), DefaultAudioDeviceType);

        void OnAudioDeviceTypeChanged(AudioDeviceType newValue)
        {
            _AudioDeviceType = newValue;
        }

        /// <summary>
        /// Gets or sets a value that describes the primary usage of the device that is being used to play back audio.
        /// </summary>
        public AudioDeviceType AudioDeviceType
        {
            get { return (AudioDeviceType)GetValue(AudioDeviceTypeProperty); }
            set { SetValue(AudioDeviceTypeProperty, value); }
        }

        #endregion

        #region PlayToSource

        /// <summary>
        /// Identifies the PlayToSource dependency property.
        /// </summary>
        public static DependencyProperty PlayToSourceProperty { get { return playToSourceProperty; } }
        static readonly DependencyProperty playToSourceProperty = RegisterDependencyProperty<PlayToSource>("PlayToSource", (t, o, n) => t.OnPlayToSourceChanged(o, n), DefaultPlayToSource);

        partial void OnPlayToSourceChanged(PlayToSource oldValue, PlayToSource newValue);

        /// <summary>
        /// Gets the information that is transmitted if the MediaElement is used for a "PlayTo" scenario.
        /// </summary>
        public PlayToSource PlayToSource
        {
            get { return (PlayToSource)GetValue(PlayToSourceProperty); }
        }

        #endregion

        #region DefaultPlaybackRate

        /// <summary>
        /// Identifies the DefaultPlaybackRate�dependency property.
        /// </summary>
        public static DependencyProperty DefaultPlaybackRateProperty { get { return defaultPlaybackRateProperty; } }
        static readonly DependencyProperty defaultPlaybackRateProperty = RegisterDependencyProperty<double>("DefaultPlaybackRate", (t, o, n) => t.OnDefaultPlaybackRateChanged(n), DefaultDefaultPlaybackRate);

        void OnDefaultPlaybackRateChanged(double newValue)
        {
            _DefaultPlaybackRate = newValue;
        }

        /// <summary>
        /// Gets or sets the default playback rate for the media engine. The playback rate applies when the user is not using fast foward or reverse.
        /// </summary>
        public double DefaultPlaybackRate
        {
            get { return (double)GetValue(DefaultPlaybackRateProperty); }
            set { SetValue(DefaultPlaybackRateProperty, value); }
        }

        #endregion

        #region IsAudioOnly

        /// <summary>
        /// Identifies the IsAudioOnly dependency property.
        /// </summary>
        public static DependencyProperty IsAudioOnlyProperty { get { return isAudioOnlyProperty; } }
        static readonly DependencyProperty isAudioOnlyProperty = RegisterDependencyProperty<bool>("IsAudioOnly", DefaultIsAudioOnly);

        /// <summary>
        /// Gets or sets a value that describes whether the media source loaded in the media engine should seek to the start after reaching its end.
        /// </summary>
        public bool IsAudioOnly
        {
            get { return (bool)GetValue(IsAudioOnlyProperty); }
        }

        #endregion

        #region IsLooping

        /// <summary>
        /// Identifies the IsLooping dependency property.
        /// </summary>
        public static DependencyProperty IsLoopingProperty { get { return isLoopingProperty; } }
        static readonly DependencyProperty isLoopingProperty = RegisterDependencyProperty<bool>("IsLooping", (t, o, n) => t.OnIsLoopingChanged(n), DefaultIsLooping);

        void OnIsLoopingChanged(bool newValue)
        {
            _IsLooping = newValue;
        }

        /// <summary>
        /// Gets or sets a value that describes whether the media source loaded in the media engine should seek to the start after reaching its end.
        /// </summary>
        public bool IsLooping
        {
            get { return (bool)GetValue(IsLoopingProperty); }
            set { SetValue(IsLoopingProperty, value); }
        }

        #endregion

        #region PosterSource

        /// <summary>
        /// Identifies the PosterSource dependency property.
        /// </summary>
        public static DependencyProperty PosterSourceProperty { get { return posterSourceProperty; } }
        static readonly DependencyProperty posterSourceProperty = RegisterDependencyProperty<ImageSource>("PosterSource", (t, o, n) => t.OnPosterSourceChanged(n), DefaultPosterSource);

        void OnPosterSourceChanged(ImageSource newValue)
        {
            _PosterSource = newValue;
        }

        /// <summary>
        /// Gets or sets the source used for a default poster effect that is used as background in the default template for MediaPlayer.
        /// </summary>
        public ImageSource PosterSource
        {
            get { return (ImageSource)GetValue(PosterSourceProperty); }
            set { SetValue(PosterSourceProperty, value); }
        }

        #endregion

        #region ActualStereo3DVideoPackingMode

        /// <summary>
        /// Identifies the ActualStereo3DVideoPackingMode dependency property.
        /// </summary>
        public static DependencyProperty ActualStereo3DVideoPackingModeProperty { get { return actualStereo3DVideoPackingModeProperty; } }
        static readonly DependencyProperty actualStereo3DVideoPackingModeProperty = RegisterDependencyProperty<Stereo3DVideoPackingMode>("ActualStereo3DVideoPackingMode", DefaultActualStereo3DVideoPackingMode);

        /// <summary>
        /// Gets a value that reports whether the current source media is a stereo 3-D video media file.
        /// </summary>
        public Stereo3DVideoPackingMode ActualStereo3DVideoPackingMode
        {
            get { return (Stereo3DVideoPackingMode)GetValue(ActualStereo3DVideoPackingModeProperty); }
        }

        #endregion

        #region Stereo3DVideoPackingMode

        /// <summary>
        /// Identifies the Stereo3DVideoPackingMode dependency property.
        /// </summary>
        public static DependencyProperty Stereo3DVideoPackingModeProperty { get { return stereo3DVideoPackingModeProperty; } }
        static readonly DependencyProperty stereo3DVideoPackingModeProperty = RegisterDependencyProperty<Stereo3DVideoPackingMode>("Stereo3DVideoPackingMode", (t, o, n) => t.OnStereo3DVideoPackingModeChanged(n), DefaultStereo3DVideoPackingMode);

        void OnStereo3DVideoPackingModeChanged(Stereo3DVideoPackingMode newValue)
        {
            _Stereo3DVideoPackingMode = newValue;
        }

        /// <summary>
        /// Gets or sets an enumeration value that determines the stereo 3-D video frame-packing mode for the current media source.
        /// </summary>
        public Stereo3DVideoPackingMode Stereo3DVideoPackingMode
        {
            get { return (Stereo3DVideoPackingMode)GetValue(Stereo3DVideoPackingModeProperty); }
            set { SetValue(Stereo3DVideoPackingModeProperty, value); }
        }
        #endregion

        #region Stereo3DVideoRenderMode

        /// <summary>
        /// Identifies the Stereo3DVideoRenderMode dependency property.
        /// </summary>
        public static DependencyProperty Stereo3DVideoRenderModeProperty { get { return stereo3DVideoRenderModeProperty; } }
        static readonly DependencyProperty stereo3DVideoRenderModeProperty = RegisterDependencyProperty<Stereo3DVideoRenderMode>("Stereo3DVideoRenderMode", (t, o, n) => t.OnStereo3DVideoRenderModeChanged(n), DefaultStereo3DVideoRenderMode);

        void OnStereo3DVideoRenderModeChanged(Stereo3DVideoRenderMode newValue)
        {
            _Stereo3DVideoRenderMode = newValue;
        }

        /// <summary>
        /// Gets or sets an enumeration value that determines the stereo 3-D video render mode for the current media source.
        /// </summary>
        public Stereo3DVideoRenderMode Stereo3DVideoRenderMode
        {
            get { return (Stereo3DVideoRenderMode)GetValue(Stereo3DVideoRenderModeProperty); }
            set { SetValue(Stereo3DVideoRenderModeProperty, value); }
        }
        #endregion

        #region IsStereo3DVideo

        /// <summary>
        /// Identifies the IsStereo3DVideo dependency property.
        /// </summary>
        public static DependencyProperty IsStereo3DVideoProperty { get { return isStereo3DVideoProperty; } }
        static readonly DependencyProperty isStereo3DVideoProperty = RegisterDependencyProperty<bool>("IsStereo3DVideo", DefaultIsStereo3DVideo);

        /// <summary>
        /// Gets a value that reports whether the current source media is a stereo 3-D video media file.
        /// </summary>
        public bool IsStereo3DVideo
        {
            get { return (bool)GetValue(IsStereo3DVideoProperty); }
        }

        #endregion

        #region RealTimePlayback

        /// <summary>
        /// Identifies the RealTimePlayback dependency property.
        /// </summary>
        public static DependencyProperty RealTimePlaybackProperty { get { return realTimePlaybackProperty; } }
        static readonly DependencyProperty realTimePlaybackProperty = RegisterDependencyProperty<bool>("RealTimePlayback", (t, o, n) => t.OnRealTimePlaybackChanged(n), DefaultRealTimePlayback);

        void OnRealTimePlaybackChanged(bool newValue)
        {
            _RealTimePlayback = newValue;
        }

        /// <summary>
        /// Gets or sets a value that configures the MediaElement for real-time communications scenarios.
        /// </summary>
        public bool RealTimePlayback
        {
            get { return (bool)GetValue(RealTimePlaybackProperty); }
            set { SetValue(RealTimePlaybackProperty, value); }
        }

        #endregion

        #region ProtectionManager

        /// <summary>
        /// Identifies the ProtectionManager dependency property.
        /// </summary>
        public static DependencyProperty ProtectionManagerProperty { get { return protectionManagerProperty; } }
        static readonly DependencyProperty protectionManagerProperty = RegisterDependencyProperty<MediaProtectionManager>("ProtectionManager", (t, o, n) => t.OnProtectionManagerChanged(n), DefaultProtectionManager);

        void OnProtectionManagerChanged(MediaProtectionManager newValue)
        {
            _ProtectionManager = newValue;
        }

        /// <summary>
        /// Gets or sets the dedicated object for media content protection that is associated with this MediaElement.
        /// </summary>
        public MediaProtectionManager ProtectionManager
        {
            get { return (MediaProtectionManager)GetValue(ProtectionManagerProperty); }
            set { SetValue(ProtectionManagerProperty, value); }
        }

        #endregion

#endif

        #region AudioStreamCount
        /// <summary>
        /// Identifies the AudioStreamCount dependency property.
        /// </summary>
        public static DependencyProperty AudioStreamCountProperty { get { return audioStreamCountProperty; } }
        static readonly DependencyProperty audioStreamCountProperty = RegisterDependencyProperty<int>("AudioStreamCount", (t, o, n) => t.OnAudioStreamCountChanged(), DefaultAudioStreamCount);

        void OnAudioStreamCountChanged()
        {
            NotifyIsAudioSelectionAllowedChanged();
        }

        /// <summary>
        /// Gets the number of audio streams available in the current media file. The default is 0.
        /// </summary>
        public int AudioStreamCount
        {
            get { return (int)GetValue(AudioStreamCountProperty); }
        }
        #endregion

        #region AudioStreamIndex
        /// <summary>
        /// Identifies the AudioStreamIndex dependency property.
        /// </summary>
        public static DependencyProperty AudioStreamIndexProperty { get { return audioStreamIndexProperty; } }
        static readonly DependencyProperty audioStreamIndexProperty = RegisterDependencyProperty<int?>("AudioStreamIndex", (t, o, n) => t.OnAudioStreamIndexChanged(o, n), DefaultAudioStreamIndex);

        void OnAudioStreamIndexChanged(int? oldValue, int? newValue)
        {
            _AudioStreamIndex = newValue;
            OnAudioStreamIndexChanged(new AudioStreamIndexChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Gets or sets the index of the audio stream that plays along with the video component.
        /// The collection of audio streams is composed at run time and represents all audio streams available within the media file.
        /// The index can be unspecified, in which case the value is null.
        /// </summary>
        public int? AudioStreamIndex
        {
            get { return (int?)GetValue(AudioStreamIndexProperty); }
            set { SetValue(AudioStreamIndexProperty, value); }
        }
        #endregion

        #region AutoPlay
        /// <summary>
        /// Identifies the AutoPlay dependency property.
        /// </summary>
        public static DependencyProperty AutoPlayProperty { get { return autoPlayProperty; } }
        static readonly DependencyProperty autoPlayProperty = RegisterDependencyProperty<bool>("AutoPlay", (t, o, n) => t.OnAutoPlayChanged(n), DefaultAutoPlay);

        void OnAutoPlayChanged(bool newValue)
        {
            // wait until after the template is applied in order to give the dev a chance to set AllowMediaStartingDeferrals
            RegisterApplyTemplateAction(() =>
            {
                // Note: by default we do not set autoplay on the mediaelement. We need to control this ourselves in order to support pre-roll ads
                if (!AllowMediaStartingDeferrals)
                {
                    _AutoPlay = newValue;
                }
            });
        }

        /// <summary>
        /// Gets or sets a value that indicates whether media will begin playback automatically when the Source property is set.
        /// Setting to false will still open, download and buffer the media but will pause on the first frame.
        /// </summary>
        public bool AutoPlay
        {
            get { return (bool)GetValue(AutoPlayProperty); }
            set { SetValue(AutoPlayProperty, value); }
        }
        #endregion

        #region BufferingProgress
        /// <summary>
        /// Identifies the BufferingProgress dependency property.
        /// </summary>
        public static DependencyProperty BufferingProgressProperty { get { return bufferingProgressProperty; } }
        static readonly DependencyProperty bufferingProgressProperty = RegisterDependencyProperty<double>("BufferingProgress", DefaultBufferingProgress);

        /// <summary>
        /// Gets a value that indicates the current buffering progress.
        /// The amount of buffering that is completed for media content. The value ranges from 0 to 1. Multiply by 100 to obtain a percentage.
        /// </summary>
        public double BufferingProgress
        {
            get { return (double)GetValue(BufferingProgressProperty); }
        }
        #endregion

        #region CanPause
        /// <summary>
        /// Identifies the CanPause dependency property.
        /// </summary>
        public static DependencyProperty CanPauseProperty { get { return canPauseProperty; } }
        static readonly DependencyProperty canPauseProperty = RegisterDependencyProperty<bool>("CanPause", DefaultCanPause);

        /// <summary>
        /// Gets a value indicating if media can be paused if the Pause() method is called.
        /// </summary>
        public bool CanPause
        {
            get { return (bool)GetValue(CanPauseProperty); }
        }
        #endregion

        #region CanSeek
        /// <summary>
        /// Identifies the CanSeek dependency property.
        /// </summary>
        public static DependencyProperty CanSeekProperty { get { return canSeekProperty; } }
        static readonly DependencyProperty canSeekProperty = RegisterDependencyProperty<bool>("CanSeek", DefaultCanSeek);

        /// <summary>
        /// Gets a value indicating if media can be repositioned by setting the value of the Position property.
        /// </summary>
        public bool CanSeek
        {
            get { return (bool)GetValue(CanSeekProperty); }
        }
        #endregion

        #region Balance
        /// <summary>
        /// Identifies the Balance dependency property.
        /// </summary>
        public static DependencyProperty BalanceProperty { get { return balanceProperty; } }
        static readonly DependencyProperty balanceProperty = RegisterDependencyProperty<double>("Balance", (t, o, n) => t.OnBalanceChanged(n), DefaultBalance);

        void OnBalanceChanged(double newValue)
        {
            _Balance = newValue;
        }

        /// <summary>
        /// Gets or sets a ratio of volume across stereo speakers. The ratio of volume across speakers in the range between -1 and 1.
        /// </summary>
        public double Balance
        {
            get { return (double)GetValue(BalanceProperty); }
            set { SetValue(BalanceProperty, value); }
        }
        #endregion

        #region DownloadProgress
        /// <summary>
        /// Identifies the DownloadProgress dependency property.
        /// </summary>
        public static DependencyProperty DownloadProgressProperty { get { return downloadProgressProperty; } }
        static readonly DependencyProperty downloadProgressProperty = RegisterDependencyProperty<double>("DownloadProgress", DefaultDownloadProgress);

        /// <summary>
        /// Gets a percentage value indicating the amount of download completed for content located on a remote server.
        /// The value ranges from 0 to 1. Multiply by 100 to obtain a percentage.
        /// </summary>
        public double DownloadProgress
        {
            get { return (double)GetValue(DownloadProgressProperty); }
        }
        #endregion

        #region DownloadProgressOffset
        /// <summary>
        /// Identifies the DownloadProgressOffset dependency property.
        /// </summary>
        public static DependencyProperty DownloadProgressOffsetProperty { get { return downloadProgressOffsetProperty; } }
        static readonly DependencyProperty downloadProgressOffsetProperty = RegisterDependencyProperty<double>("DownloadProgressOffset", DefaultDownloadProgressOffset);

        /// <summary>
        /// Gets the offset of the download progress.
        /// </summary>
        public double DownloadProgressOffset
        {
            get { return (double)GetValue(DownloadProgressOffsetProperty); }
        }
        #endregion

        #region IsMuted
        /// <summary>
        /// Identifies the IsMuted dependency property.
        /// </summary>
        public static DependencyProperty IsMutedProperty { get { return isMutedProperty; } }
        static readonly DependencyProperty isMutedProperty = RegisterDependencyProperty<bool>("IsMuted", (t, o, n) => t.OnIsMutedChanged(o, n), DefaultIsMuted);

        void OnIsMutedChanged(bool oldValue, bool newValue)
        {
            _IsMuted = newValue;
            OnIsMutedChanged(new IsMutedChangedEventArgs());
        }

        /// <summary>
        /// Gets or sets a value indicating whether the audio is muted.
        /// </summary>
        public bool IsMuted
        {
            get { return (bool)GetValue(IsMutedProperty); }
            set { SetValue(IsMutedProperty, value); }
        }
        #endregion

        #region NaturalDuration
        /// <summary>
        /// Identifies the NaturalDuration dependency property.
        /// </summary>
        public static DependencyProperty NaturalDurationProperty { get { return naturalDurationProperty; } }
        static readonly DependencyProperty naturalDurationProperty = RegisterDependencyProperty<Duration>("NaturalDuration", (t, o, n) => t.OnNaturalDurationChanged(n), DefaultNaturalDuration);

        void OnNaturalDurationChanged(Duration newValue)
        {
            if (newValue.HasTimeSpan && newValue != TimeSpan.MaxValue)
            {
                EndTime = StartTime.Add(newValue.TimeSpan); // this will trigger Duration to get set.
            }
        }

        /// <summary>
        /// The natural duration of the media. The default value is Duration.Automatic, which is the value held if you query this property before MediaOpened.
        /// </summary>
        public Duration NaturalDuration
        {
            get { return (Duration)GetValue(NaturalDurationProperty); }
        }
        #endregion

        #region NaturalVideoHeight
        /// <summary>
        /// Identifies the NaturalVideoHeight dependency property.
        /// </summary>
        public static DependencyProperty NaturalVideoHeightProperty { get { return naturalVideoHeightProperty; } }
        static readonly DependencyProperty naturalVideoHeightProperty = RegisterDependencyProperty<int>("NaturalVideoHeight", DefaultNaturalVideoHeight);

        /// <summary>
        /// Gets the height of the video associated with the media.
        /// The height of the video that is associated with the media, in pixels. Audio files will return 0.
        /// </summary>
        public int NaturalVideoHeight
        {
            get { return (int)GetValue(NaturalVideoHeightProperty); }
        }
        #endregion

        #region NaturalVideoWidth
        /// <summary>
        /// Identifies the NaturalVideoWidth dependency property.
        /// </summary>
        public static DependencyProperty NaturalVideoWidthProperty { get { return naturalVideoWidthProperty; } }
        static readonly DependencyProperty naturalVideoWidthProperty = RegisterDependencyProperty<int>("NaturalVideoWidth", DefaultNaturalVideoWidth);

        /// <summary>
        /// Gets the width of the video associated with the media.
        /// The width of the video associated with the media, in pixels. Audio files will return 0.
        /// </summary>
        public int NaturalVideoWidth
        {
            get { return (int)GetValue(NaturalVideoWidthProperty); }
        }
        #endregion

        #region PlaybackRate
        /// <summary>
        /// Identifies the PlaybackRate dependency property.
        /// </summary>
        public static DependencyProperty PlaybackRateProperty { get { return playbackRateProperty; } }
        static readonly DependencyProperty playbackRateProperty = RegisterDependencyProperty<double>("PlaybackRate", (t, o, n) => t.OnPlaybackRateChanged(n), DefaultRate);

        void OnPlaybackRateChanged(double newValue)
        {
            _PlaybackRate = newValue;
        }

        /// <summary>
        /// Gets or sets the playback rate of the media. Use this property to directly control features like fast forward, reverse, and slow motion.
        /// IncreasePlaybackRate, DecreasePlaybackRate, and IsSlowMotion are also available to help control this.
        /// </summary>
        public double PlaybackRate
        {
            get { return (double)GetValue(PlaybackRateProperty); }
            set { SetValue(PlaybackRateProperty, value); }
        }
        #endregion

        #region Position
        /// <summary>
        /// Identifies the Position dependency property.
        /// </summary>
        public static DependencyProperty PositionProperty { get { return positionProperty; } }
        static readonly DependencyProperty positionProperty = RegisterDependencyProperty<TimeSpan>("Position", (t, o, n) => t.OnPositionChanged(n, o), DefaultPosition);

        void OnPositionChanged(TimeSpan newValue, TimeSpan oldValue)
        {
            SetValueWithoutCallback(PositionProperty, oldValue); // reset the value temporarily while we're seeking. This will get updated once the seek completes and ensure the integrity of PositionChangedEventArgs.PreviousValue
            var t = SeekAsync(newValue);
        }

        /// <summary>
        /// Gets or sets the current position of progress through the media's playback time (or the amount of time since the beginning of the media).
        /// </summary>
        public TimeSpan Position
        {
            get { return (TimeSpan)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }
        #endregion

        #region CurrentState
        /// <summary>
        /// Identifies the CurrentState dependency property.
        /// </summary>
        public static DependencyProperty CurrentStateProperty { get { return currentStateProperty; } }
        static readonly DependencyProperty currentStateProperty = RegisterDependencyProperty<MediaElementState>("CurrentState", DefaultCurrentState);

        /// <summary>
        /// Gets the status of the MediaElement.
        /// The state can be one of the following (as defined in the MediaElementState enumeration):
        /// Buffering, Closed, Opening, Paused, Playing, Stopped.
        /// </summary>
        public MediaElementState CurrentState
        {
            get { return (MediaElementState)GetValue(CurrentStateProperty); }
        }
        #endregion

        #region Source
        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static DependencyProperty SourceProperty { get { return sourceProperty; } }
        static readonly DependencyProperty sourceProperty = RegisterDependencyProperty<Uri>("Source", (t, o, n) => t.OnSourceChanged(n), DefaultSource);

        /// <summary>
        /// Gets or sets a media source on the MediaElement.
        /// A string that specifies the source of the element, as a Uniform Resource Identifier (URI).
        /// </summary>
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        void OnSourceChanged(Uri newValue)
        {
            if (!IsInDesignMode)
            {
                SetSource(newValue);
            }
        }

        void SetSource(Uri source)
        {
            RegisterApplyTemplateAction(async () =>
            {
                if (AutoLoad || source == null)
                {
                    bool proceed = true;
                    MediaLoadingInstruction loadingResult = null;
                    if (source != null)
                    {
                        loadingResult = await OnMediaLoadingAsync(source);
                        proceed = loadingResult != null;
                    }
                    if (proceed)
                    {
                        LoadSource(loadingResult);
                    }
                    else
                    {
                        OnMediaFailure(null);
                    }
                }
                else
                {
                    PendingLoadAction = () => SetSource(source);
                }
            });
        }

        private void LoadSource(MediaLoadingInstruction loadingInstruction)
        {
            if (IsMediaLoaded)
            {
                OnMediaClosed(EventArgs.Empty);
            }
            if (loadingInstruction == null)
            {
                _Source = null;
            }
            else if (loadingInstruction.Source != null)
            {
                _Source = loadingInstruction.Source;
            }
#if NETFX_CORE
            else if (loadingInstruction.SourceStream != null)
            {
                _SetSource(loadingInstruction.SourceStream, loadingInstruction.MimeType);
            }
#else
            else if (loadingInstruction.SourceStream != null)
            {
                _SetSource(loadingInstruction.SourceStream);
            }
            else if (loadingInstruction.MediaStreamSource != null)
            {
                _SetSource(loadingInstruction.MediaStreamSource);
            }
#endif
            IsMediaLoaded = loadingInstruction != null;
        }

#if NETFX_CORE
        private async Task<MediaLoadingInstruction> OnMediaLoadingAsync(IRandomAccessStream stream, string mimeType)
        {
            var deferrableOperation = new MediaPlayerDeferrableOperation(cts);
            var args = new MediaLoadingEventArgs(deferrableOperation, stream, mimeType);
            return await OnMediaLoadingAsync(args);
        }

#else
        private async Task<MediaLoadingInstruction> OnMediaLoadingAsync(MediaStreamSource mediaStreamSource)
        {
            var deferrableOperation = new MediaPlayerDeferrableOperation(cts);
            var args = new MediaLoadingEventArgs(deferrableOperation, mediaStreamSource);
            return await OnMediaLoadingAsync(args);
        }

        private async Task<MediaLoadingInstruction> OnMediaLoadingAsync(Stream stream)
        {
            var deferrableOperation = new MediaPlayerDeferrableOperation(cts);
            var args = new MediaLoadingEventArgs(deferrableOperation, stream);
            return await OnMediaLoadingAsync(args);
        }

#endif
        private async Task<MediaLoadingInstruction> OnMediaLoadingAsync(Uri source)
        {
            var deferrableOperation = new MediaPlayerDeferrableOperation(cts);
            var args = new MediaLoadingEventArgs(deferrableOperation, source);
            return await OnMediaLoadingAsync(args);
        }

        private async Task<MediaLoadingInstruction> OnMediaLoadingAsync(MediaLoadingEventArgs args)
        {
            SetValue(PlayerStateProperty, PlayerState.Loading);
#if NETFX_CORE
            if (TestForMediaPack)
            {
                if (!await MediaPackHelper.TestForMediaPack())
                {
                    return null;
                }
            }
#endif
            if (MediaLoading != null) MediaLoading(this, args);
            bool[] result;
            try
            {
                result = await args.DeferrableOperation.Task;
            }
            catch (OperationCanceledException) { return null; }
            if (!result.Any() || result.All(r => r))
            {
                if (AllowMediaStartingDeferrals)
                {
                    _AutoPlay = false;
                }
                return new MediaLoadingInstruction(args);
            }
            else return null;
        }
        #endregion

        #region Volume
        /// <summary>
        /// Identifies the Volume dependency property.
        /// </summary>
        public static DependencyProperty VolumeProperty { get { return volumeProperty; } }
        static readonly DependencyProperty volumeProperty = RegisterDependencyProperty<double>("Volume", (t, o, n) => t.OnVolumeChanged(o, n), DefaultVolume);

        void OnVolumeChanged(double oldValue, double newValue)
        {
            _Volume = newValue;
#if SILVERLIGHT
            OnVolumeChanged(new RoutedPropertyChangedEventArgs<double>(oldValue, newValue));
#else
            if (PlayerState == PlayerState.Unloaded)
            {
                // raise the volume changed event if the player is unloaded because VolumeChanged does not fire on the MediaElement.
                // this enables the scenario of playing ads without loading a source. Without it, ads would not receive volume change notifications and the volume slider will not work.
                OnVolumeChanged(null);
            }
#endif
        }

        /// <summary>
        /// Gets or sets the media's volume.
        /// The media's volume represented on a linear scale between 0 and 1. The default is 0.5.
        /// </summary>
        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        #endregion

        #region SupportedPlaybackRates
        /// <summary>
        /// Identifies the SupportedPlaybackRates dependency property.
        /// </summary>
        public static DependencyProperty SupportedPlaybackRatesProperty { get { return supportedPlaybackRatesProperty; } }
        static readonly DependencyProperty supportedPlaybackRatesProperty = RegisterDependencyProperty<IList<double>>("SupportedPlaybackRates", (t, o, n) => t.OnSupportedPlaybackRatesChanged(), null);

        void OnSupportedPlaybackRatesChanged()
        {
            var eligibleSlowMotionRates = SupportedPlaybackRates.Where(r => r > 0 && r < 1);
            if (eligibleSlowMotionRates.Any())
            {
                SlowMotionPlaybackRate = eligibleSlowMotionRates.First();
            }

            NotifyIsFastForwardAllowedChanged();
            NotifyIsRewindAllowedChanged();
            NotifyIsSlowMotionAllowedChanged();
        }

        static IList<double> DefaultSupportedPlaybackRates
        {
            get
            {
                return new List<double>(new[] { -32, -16, -8, -4, -2, -1, 0, .25, .5, 1, 2, 4, 8, 16, 32 });
            }
        }

        /// <summary>
        /// Gets or sets the supported playback rates. This impacts when slow motion, fast forward and rewind are available.
        /// </summary>
        public IList<double> SupportedPlaybackRates
        {
            get { return GetValue(SupportedPlaybackRatesProperty) as IList<double>; }
            set { SetValue(SupportedPlaybackRatesProperty, value); }
        }
        #endregion

        /// <summary>
        /// Gets or sets whether the MediaStarting event supports deferrals before playback begins. Note: without this, pre-roll ads will not work. 
        /// Interally, this causes MediaElement.AutoPlay to be set to false and Play to be called automatically from the MediaOpened event (if AutoPlay is true).
        /// </summary>
        public bool AllowMediaStartingDeferrals { get; set; }

        void OnInvokeCaptionSelection(CaptionsInvokedEventArgs e)
        {
            if (CaptionsInvoked != null) CaptionsInvoked(this, e);
        }

        void OnInvokeAudioSelection(AudioSelectionInvokedEventArgs e)
        {
            if (AudioSelectionInvoked != null) AudioSelectionInvoked(this, e);
        }

        void OnSeekToLive(GoLiveInvokedEventArgs e)
        {
            if (GoLiveInvoked != null) GoLiveInvoked(this, e);
        }

        void OnMediaFailed(ExceptionRoutedEventArgs e)
        {
            if (MediaFailed != null) MediaFailed(this, e);

            SetValue(PlayerStateProperty, PlayerState.Failed);
        }

        async void OnMediaEnded(MediaEndedEventArgs e)
        {
            SetValue(PlayerStateProperty, PlayerState.Ending);
            var deferrableOperation = new MediaPlayerDeferrableOperation(cts);
            if (MediaEnding != null)
            {
                MediaEnding(this, new MediaEndingEventArgs(deferrableOperation));
                if (deferrableOperation.DeferralsExist)
                {
                    try
                    {
                        await deferrableOperation.Task;
                    }
                    catch (OperationCanceledException) { return; }
                    // HACK: this lets all other operations awaiting this task finish first. Important for postrolls. Might be a better way to do this.
#if SILVERLIGHT
                    await Dispatcher.InvokeAsync(() => { });
#else
                    await Dispatcher.BeginInvoke(() => { });
#endif
                }
            }

            if (MediaEnded != null) MediaEnded(this, e);

            if (!e.Handled)
            {
                SetValue(PlayerStateProperty, PlayerState.Started);
                OnProcessMediaEndedBehavior();
            }
        }

        /// <summary>
        /// Gives a subclass the opportunity to perform custom behaviors when the media ends. Called after MediaEnded fires.
        /// </summary>
        void OnProcessMediaEndedBehavior()
        {
#if SILVERLIGHT
            if (IsLooping)
            {
                Position = TimeSpan.Zero;
                Play();
            }
#endif
            if (!IsLooping)
            {
                switch (MediaEndedBehavior)
                {
                    case MediaEndedBehavior.Manual:
                        // do nothing
                        break;
                    case MediaEndedBehavior.Stop:
                        Stop();
                        break;
                    case MediaEndedBehavior.Reset:
                        Position = StartTime;
                        break;
                }
            }
        }

        void OnMarkerReached(TimelineMarkerRoutedEventArgs e)
        {
            if (MarkerReached != null) MarkerReached(this, e);
        }

        void OnDownloadProgressChanged(RoutedEventArgs e)
        {
            if (DownloadProgressChanged != null) DownloadProgressChanged(this, e);
        }

        void OnBufferingProgressChanged(RoutedEventArgs e)
        {
            if (BufferingProgressChanged != null) BufferingProgressChanged(this, e);
        }

        MediaElementState stateWithoutBufferg = MediaElementState.Closed;
        void OnCurrentStateChanged(RoutedEventArgs e)
        {
#if WINDOWS_PHONE
            // WP7 doesn't update these in the MediaOpened event
            SetValue(CanPauseProperty, _CanPause);
            SetValue(CanSeekProperty, _CanSeek);
#endif

#if NETFX_CORE
            if (CurrentState == MediaElementState.Opening)
            {
                SetValue(PlayToSourceProperty, _PlayToSource);
            }
            else if (_CurrentState == MediaElementState.Closed)
            {
                SetValue(PlayToSourceProperty, DefaultPlayToSource);
            }
#endif

            if (CurrentState != MediaElementState.Buffering)
            {
                stateWithoutBufferg = CurrentState;
                NotifyIsPlayResumeAllowedChanged();
                NotifyIsPauseAllowedChanged();
                NotifyIsStopAllowedChanged();
                NotifyIsReplayAllowedChanged();
                NotifyIsRewindAllowedChanged();
                NotifyIsFastForwardAllowedChanged();
                NotifyIsSlowMotionAllowedChanged();
                if (CurrentStateChangedBufferingIgnored != null) CurrentStateChangedBufferingIgnored(this, e);
            }
            NotifyIsSeekAllowedChanged();
            NotifyIsSkipPreviousAllowedChanged();
            NotifyIsSkipNextAllowedChanged();
            NotifyIsSkipBackAllowedChanged();
            NotifyIsSkipAheadAllowedChanged();
            NotifyIsScrubbingAllowedChanged();
            if (CurrentStateChanged != null) CurrentStateChanged(this, e);
        }

        void OnMediaElementUnititialized()
        {
#if SILVERLIGHT
#else
            if (currentSeek != null) currentSeek.TrySetCanceled();
#endif
        }

#if SILVERLIGHT
        void OnLogReady(LogReadyRoutedEventArgs e)
        {
            if (LogReady != null) LogReady(this, e);
        }

        void OnStretchChanged(RoutedPropertyChangedEventArgs<Stretch> e)
        {
            if (StretchChanged != null) StretchChanged(this, e);
        }
        
        /// <summary>
        /// Performs an async Seek. This can also be accomplished by setting the Position property and waiting for SeekCompleted to fire.
        /// </summary>
        /// <param name="position">The new position to seek to</param>
        /// <returns>The task to await on. If true, the seek was successful, if false, the seek was trumped by a new value while it was waiting for a pending seek to complete.</returns>
        private Task<bool> SeekAsync(TimeSpan position)
        {
            _Position = position;
#if SILVERLIGHT && !WINDOWS_PHONE || WINDOWS_PHONE7
            return TaskEx.FromResult(true); // There is no SeekCompleted event in Silverlight's MediaElement, therefore we just assume true and rely on the MediaElement to buffer this for us.
#else
            return Task.FromResult(true); // There is no SeekCompleted event in Silverlight's MediaElement, therefore we just assume true and rely on the MediaElement to buffer this for us.
#endif
        }
#else

        /// <summary>
        /// Performs an async Seek. This can also be accomplished by setting the Position property and waiting for SeekCompleted to fire.
        /// </summary>
        /// <param name="position">The new position to seek to</param>
        /// <returns>The task to await on. If true, the seek was successful, if false, the seek was trumped by a new value while it was waiting for a pending seek to complete.</returns>
#if NETFX_CORE
        public IAsyncOperation<bool> SeekAsync(TimeSpan position)
        {
            return AsyncInfo.Run(c => seekAsync(position));
        }

        async Task<bool> seekAsync(TimeSpan position)
#else
        public async Task<bool> SeekAsync(TimeSpan position)
#endif
        {
            if (currentSeek != null)
            {
                var thisOperation = new object();
                pendingSeekOperation = thisOperation;
                try
                {
                    await currentSeek.Task;
                    await Task.Yield(); // let the original task await finish first to ensure currentSeek is set to null.
                }
                catch (OperationCanceledException) { /* ignore */ }
                if (IsMediaOpened) // check to make sure we are still open.
                {
                    if (object.ReferenceEquals(pendingSeekOperation, thisOperation))
                    {
                        try
                        {
                            return await SeekAsync(position);
                        }
                        catch (OperationCanceledException) { /* ignore */ }
                    }
                }
                return false;
            }
            else
            {
                try
                {
                    await SingleSeekAsync(position);
                    return true;
                }
                catch (OperationCanceledException) { return false; }
            }
        }

        void OnSeekCompleted(RoutedEventArgs e)
        {
            if (currentSeek != null) currentSeek.TrySetResult(e);
            if (SeekCompleted != null) SeekCompleted(this, e);
        }

        // WARNING: this should never be called directly and is intended to only process a single operation at a time.
        private async Task SingleSeekAsync(TimeSpan position)
        {
            currentSeek = new TaskCompletionSource<RoutedEventArgs>();
            try
            {
                _Position = position;
                await currentSeek.Task;
                OnUpdate();
            }
            finally
            {
                currentSeek = null;
            }
        }
#endif

        /// <summary>
        /// Occurs when the timer updates
        /// </summary>
        void OnUpdate()
        {
#if !POSITIONBINDING
            previousPosition = this.Position;
            var newPosition = _Position;
            if (newPosition != previousPosition)
            {
                SetValueWithoutCallback(PositionProperty, newPosition);
                UpdateIsPositionLive();
                OnPositionChanged(new PositionChangedEventArgs(previousPosition, newPosition));
            }
            else
            {
                UpdateIsPositionLive();
            }
#endif
#if SILVERLIGHT
            SetValue(RenderedFramesPerSecondProperty, _RenderedFramesPerSecond);
            SetValue(DroppedFramesPerSecondProperty, _DroppedFramesPerSecond);
#endif
            if (Updated != null) Updated(this, new UpdatedEventArgs());
        }

        async void OnMediaOpened(RoutedEventArgs e)
        {
            OnMediaOpened();

            SetValue(PlayerStateProperty, PlayerState.Opened);

            if (MediaOpened != null) MediaOpened(this, e);

            if ((AutoPlay || StartupPosition.HasValue) && await OnMediaStartingAsync())
            {
                if (StartupPosition.HasValue)
                {
#if WINDOWS_PHONE
                    // HACK: sometimes this is ignored on the phone if we don't set the position on the dispatcher
                    Dispatcher.BeginInvoke(() =>
                    {
                        Position = StartupPosition.Value;
                    });
#else
                    Position = StartupPosition.Value;
#endif
                }

                if (AutoPlay && AllowMediaStartingDeferrals)
                {
                    _Play();
                }

                OnUpdate();   // simulate the timer tick ASAP so everyone can update things.
            }
        }

        private async Task<bool> OnMediaStartingAsync()
        {
            if (PlayerState != PlayerState.Starting)
            {
                SetValue(PlayerStateProperty, PlayerState.Starting);
                var deferrableOperation = new MediaPlayerDeferrableOperation(cts);
                if (MediaStarting != null) MediaStarting(this, new MediaStartingEventArgs(deferrableOperation));
                bool[] result;
                try
                {
                    result = await deferrableOperation.Task;
                }
                catch (OperationCanceledException) { return false; }
                if (!result.Any() || result.All(r => r))
                {
                    SetValue(PlayerStateProperty, PlayerState.Started);
                    if (MediaStarted != null) MediaStarted(this, new MediaStartedEventArgs());
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Initializes the MediaPlayer once the media has opened but immediately before the MediaOpened event fires.
        /// </summary>
        void OnMediaOpened()
        {
#if SILVERLIGHT && !WINDOWS_PHONE
            SetValue(AttributesProperty, _Attributes);
            SetValue(IsDecodingOnGPUProperty, _IsDecodingOnGPU);
#elif NETFX_CORE
            SetValue(AspectRatioHeightProperty, _AspectRatioHeight);
            SetValue(AspectRatioWidthProperty, _AspectRatioWidth);
            SetValue(IsStereo3DVideoProperty, _IsStereo3DVideo);
            SetValue(ActualStereo3DVideoPackingModeProperty, _ActualStereo3DVideoPackingMode);
            SetValue(IsAudioOnlyProperty, _IsAudioOnly);
#endif
            SetValue(AudioStreamIndexProperty, _AudioStreamIndex);
            SetValue(AudioStreamCountProperty, _AudioStreamCount);
            SetValue(CanPauseProperty, _CanPause);
            SetValue(CanSeekProperty, _CanSeek);
            SetValue(NaturalDurationProperty, _NaturalDuration);
            SetValue(NaturalVideoHeightProperty, _NaturalVideoHeight);
            SetValue(NaturalVideoWidthProperty, _NaturalVideoWidth);
            SetValue(MediaQualityProperty, NaturalVideoHeight >= 720 ? MediaQuality.HighDefinition : MediaQuality.StandardDefinition);

            PopulateAvailableAudioStreams();
        }

        /// <summary>
        /// Populates the available audio streams from the MediaElement.
        /// </summary>
        void PopulateAvailableAudioStreams()
        {
            // only add audio streams if it is empty. Otherwise, it implies the app or a plugin is taking care of this for us.
            if (!AvailableAudioStreams.Any())
            {
                for (int i = 0; i < AudioStreamCount; i++)
                {
#if !SILVERLIGHT
                    var language = GetAudioStreamLanguage(i);
                    string name;
                    if (!string.IsNullOrEmpty(language))
                    {
                        if (Windows.Globalization.Language.IsWellFormed(language))
                        {
                            name = new Windows.Globalization.Language(language).DisplayName;
                        }
                        else
                        {
                            name = language;
                        }
                    }
                    else
                    {
                        name = DefaultAudioStreamName;
                    }
                    var audioStream = new AudioStream(name, language);
#else
                    var audioStream = new AudioStream(DefaultAudioStreamName);
#endif
                    AvailableAudioStreams.Add(audioStream);
                    if (i == AudioStreamIndex.GetValueOrDefault(0))
                    {
                        SelectedAudioStream = audioStream;
                    }
                }
            }
        }

        void OnMediaClosed(object e)
        {
            cts.Cancel();
            cts = new CancellationTokenSource(); // reset the cancellation token
            OnMediaClosed();

            if (MediaClosed != null) MediaClosed(this, new MediaClosedEventArgs());
        }

        /// <summary>
        /// Cleans up the MediaPlayer when the media has closed but immediately before the MediaClosed event fires.
        /// </summary>
        void OnMediaClosed()
        {
            IsMediaLoaded = false;
#if SILVERLIGHT && !WINDOWS_PHONE
            SetValue(AttributesProperty, DefaultAttributes);
            SetValue(IsDecodingOnGPUProperty, DefaultIsDecodingOnGPU);
#elif NETFX_CORE
            SetValue(AspectRatioHeightProperty, DefaultAspectRatioHeight);
            SetValue(AspectRatioWidthProperty, DefaultAspectRatioWidth);
            SetValue(PlayToSourceProperty, DefaultPlayToSource);
#endif
            SetValue(AudioStreamCountProperty, DefaultAudioStreamCount);
            // do not actually push value into MediaElement or it will throw since media is closed.
            SetValueWithoutCallback(AudioStreamIndexProperty, DefaultAudioStreamIndex);
            SetValue(CanPauseProperty, DefaultCanPause);
            SetValue(CanSeekProperty, DefaultCanSeek);
            SetValue(StartTimeProperty, TimeSpan.Zero);
            SetValue(LivePositionProperty, (TimeSpan?)null);
            SetValue(IsLiveProperty, false);
            SetValue(NaturalDurationProperty, DefaultNaturalDuration);
            SetValue(NaturalVideoHeightProperty, DefaultNaturalVideoHeight);
            SetValue(NaturalVideoWidthProperty, DefaultNaturalVideoWidth);
            SetValue(MediaQualityProperty, MediaQuality.StandardDefinition);
            SetValue(SignalStrengthProperty, 0.0);
        }

        void OnRateChanged(RateChangedRoutedEventArgs e)
        {
#if SILVERLIGHT
            IsSlowMotion = (e.NewRate == SlowMotionPlaybackRate);
#else
            IsSlowMotion = (_PlaybackRate == SlowMotionPlaybackRate);
#endif
            NotifyIsPlayResumeAllowedChanged();
            NotifyIsRewindAllowedChanged();
            NotifyIsFastForwardAllowedChanged();
            if (RateChanged != null) RateChanged(this, e);
        }

        void OnPositionChanged(PositionChangedEventArgs e)
        {
            SetValue(TimeRemainingProperty, TimeSpanExtensions.Max(EndTime.Subtract(e.NewValue), TimeSpan.Zero));
            if (PositionChanged != null) PositionChanged(this, e);
        }

        void OnVolumeChanged(RoutedEventArgs e)
        {
            if (VolumeChanged != null) VolumeChanged(this, e);
        }

        void OnIsMutedChanged(IsMutedChangedEventArgs e)
        {
            if (IsMutedChanged != null) IsMutedChanged(this, e);
        }

        void OnIsLiveChanged(IsLiveChangedEventArgs e)
        {
            if (IsLiveChanged != null) IsLiveChanged(this, e);
        }

        void OnIsCaptionsActiveChanged(IsCaptionsActiveChangedEventArgs e)
        {
            if (IsCaptionsActiveChanged != null) IsCaptionsActiveChanged(this, e);
        }

        void OnIsFullScreenChanged(IsFullScreenChangedEventArgs e)
        {
            if (IsFullScreenChanged != null) IsFullScreenChanged(this, e);
        }

        void OnAdvertisingStateChanged(AdvertisingStateChangedEventArgs e)
        {
            if (AdvertisingStateChanged != null) AdvertisingStateChanged(this, e);
        }

        void OnAudioStreamIndexChanged(AudioStreamIndexChangedEventArgs e)
        {
            if (AudioStreamIndexChanged != null) AudioStreamIndexChanged(this, e);
        }

        void OnSeeking(SeekingEventArgs e)
        {
            if (Seeking != null) Seeking(this, e);
        }

        void OnScrubbing(ScrubbingEventArgs e)
        {
            if (Scrubbing != null) Scrubbing(this, e);
        }

        void OnCompletingScrub(CompletingScrubEventArgs e)
        {
            if (CompletingScrub != null) CompletingScrub(this, e);
        }

        void OnStartingScrub(StartingScrubEventArgs e)
        {
            if (StartingScrub != null) StartingScrub(this, e);
        }

        void OnPlayerStateChanged(PlayerStateChangedEventArgs e)
        {
            if (PlayerStateChanged != null) PlayerStateChanged(this, e);
        }

        void OnSignalStrengthChanged(SignalStrengthChangedEventArgs e)
        {
            if (SignalStrengthChanged != null) SignalStrengthChanged(this, e);
        }

        void OnMediaQualityChanged(MediaQualityChangedEventArgs e)
        {
            if (MediaQualityChanged != null) MediaQualityChanged(this, e);
        }

        void OnIsSlowMotionChanged(IsSlowMotionChangedEventArgs e)
        {
            if (IsSlowMotionChanged != null) IsSlowMotionChanged(this, e);
        }

        void OnDurationChanged(DurationChangedEventArgs e)
        {
            if (DurationChanged != null) DurationChanged(this, e);
        }

        void OnStartTimeChanged(StartTimeChangedEventArgs e)
        {
            if (StartTimeChanged != null) StartTimeChanged(this, e);
        }

        void OnEndTimeChanged(EndTimeChangedEventArgs e)
        {
            if (EndTimeChanged != null) EndTimeChanged(this, e);
        }

        void OnTimeRemainingChanged(TimeRemainingChangedEventArgs e)
        {
            if (TimeRemainingChanged != null) TimeRemainingChanged(this, e);
        }

        void OnLivePositionChanged(LivePositionChangedEventArgs e)
        {
            if (LivePositionChanged != null) LivePositionChanged(this, e);
        }

        void OnTimeFormatConverterChanged(TimeFormatConverterChangedEventArgs e)
        {
            if (TimeFormatConverterChanged != null) TimeFormatConverterChanged(this, e);
        }

        void OnSelectedCaptionChanged(SelectedCaptionChangedEventArgs e)
        {
            if (SelectedCaptionChanged != null) SelectedCaptionChanged(this, e);
        }

        void OnSelectedAudioStreamChanged(SelectedAudioStreamChangedEventArgs e)
        {
            if (SelectedAudioStreamChanged != null) SelectedAudioStreamChanged(this, e);
        }

        void OnSkipBackIntervalChanged(SkipBackIntervalChangedEventArgs e)
        {
            if (SkipBackIntervalChanged != null) SkipBackIntervalChanged(this, e);
        }

        void OnSkipAheadIntervalChanged(SkipAheadIntervalChangedEventArgs e)
        {
            if (SkipAheadIntervalChanged != null) SkipAheadIntervalChanged(this, e);
        }


        #endregion

        #region Helpers

        /// <summary>
        /// Retrieves a resource string from the ResourceLoader
        /// </summary>
        /// <param name="resourceId">The ID of the resource</param>
        /// <returns>The resource string found.</returns>
        public static string GetResourceString(string resourceId)
#if SILVERLIGHT
        {
            return Microsoft.PlayerFramework.Resources.ResourceManager.GetString(resourceId, Microsoft.PlayerFramework.Resources.Culture);
        }
#else
        {
            string result = null;
            if (ResourceLoader != null)
            {
                result = ResourceLoader.GetString(string.Format("{0}", resourceId));
            }
            if (string.IsNullOrEmpty(result))
            {
                if (!IsInDesignMode)
                {
                    result = DefaultResourceLoader.GetString(string.Format("Resources/{0}", resourceId));
                }
                else result = resourceId;
            }
            return result;
        }

        static ResourceLoader defaultResourceLoader;
        static ResourceLoader DefaultResourceLoader
        {
            get
            {
                if (defaultResourceLoader == null)
                {
                    defaultResourceLoader = new ResourceLoader("Microsoft.PlayerFramework");
                }
                return defaultResourceLoader;
            }
        }

        /// <summary>
        /// Gets or sets the ResourceLoader used to load all string resources.
        /// </summary>
        public static ResourceLoader ResourceLoader { get; set; }
#endif

        private static DependencyProperty RegisterDependencyProperty<T1, T2>(string propertyName, Action<T1, T2, T2> callback, T2 defaultValue) where T1 : DependencyObject
        {
            return DependencyProperty.Register(propertyName, typeof(T2), typeof(T1), new PropertyMetadata(defaultValue, (d, e) => callback((T1)d, (T2)e.OldValue, (T2)e.NewValue)));
        }

        private static DependencyProperty RegisterDependencyProperty<T1, T2>(string propertyName, Action<T1, T2, T2> callback) where T1 : DependencyObject
        {
#if SILVERLIGHT
            return DependencyProperty.Register(propertyName, typeof(T2), typeof(T1), new PropertyMetadata((d, e) => callback((T1)d, (T2)e.OldValue, (T2)e.NewValue)));
#else
            return DependencyProperty.Register(propertyName, typeof(T2), typeof(T1), new PropertyMetadata(default(T2), (d, e) => callback((T1)d, (T2)e.OldValue, (T2)e.NewValue)));
#endif
        }

        private static DependencyProperty RegisterDependencyProperty<T1, T2>(string propertyName, T2 defaultValue = default(T2)) where T1 : DependencyObject
        {
            return DependencyProperty.Register(propertyName, typeof(T2), typeof(T1), new PropertyMetadata(defaultValue));
        }

        private static DependencyProperty RegisterDependencyProperty<T1, T2>(string propertyName) where T1 : DependencyObject
        {
            return DependencyProperty.Register(propertyName, typeof(T2), typeof(T1), null);
        }

        static DependencyProperty RegisterDependencyProperty<T>(string propertyName)
        {
            return RegisterDependencyProperty<MediaPlayer, T>(propertyName);
        }

        static DependencyProperty RegisterDependencyProperty<T>(string propertyName, Action<MediaPlayer, T, T> callback)
        {
            return RegisterDependencyProperty<MediaPlayer, T>(propertyName, (t, o, n) =>
            {
                if (!t.ignoreCallback) callback(t, o, n);
            });
        }

        static DependencyProperty RegisterDependencyProperty<T>(string propertyName, Action<MediaPlayer, T, T> callback, T defaultValue)
        {
            return RegisterDependencyProperty<MediaPlayer, T>(propertyName, (t, o, n) =>
            {
                if (!t.ignoreCallback) callback(t, o, n);
            }, defaultValue);
        }

        static DependencyProperty RegisterDependencyProperty<T>(string propertyName, T defaultValue)
        {
            return RegisterDependencyProperty<MediaPlayer, T>(propertyName, defaultValue);
        }

        bool ignoreCallback;

        /// <summary>
        /// Sets the local value of a dependency property on a DependencyObject without invoking the callback.
        /// </summary>
        /// <param name="dp">The identifier of the dependency property to set.</param>
        /// <param name="value">The new local value.</param>
        void SetValueWithoutCallback(DependencyProperty dp, object value)
        {
            ignoreCallback = true;
            try
            {
                SetValue(dp, value);
            }
            finally
            {
                ignoreCallback = false;
            }
        }
        #endregion

        /// <summary>
        /// Disposes of the active session and frees up all memory associated with this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Use SuppressFinalize in case a subclass of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        /// <summary>
        /// Disposes of the active session and frees up all memory associated with this instance.
        /// </summary>
        /// <param name="disposing">Is called from the Dispose method.</param>
        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Unload();
                    LoadPlugins(null);
                    UninitializeTemplateChildren();
                    DestroyTemplateChildren();
                    UninitializeTemplateDefinitions();
                    UpdateTimer.Tick -= UpdateTimer_Tick;
#if !SILVERLIGHT
                    mediaExtensionManager = null;
#endif
                }

                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }

        private class MediaLoadingInstruction
        {
            internal MediaLoadingInstruction(MediaLoadingEventArgs args)
            {
                Source = args.Source;
                SourceStream = args.SourceStream;
#if NETFX_CORE
                MimeType = args.MimeType;
#else
                MediaStreamSource = args.MediaStreamSource;
#endif
            }

            public Uri Source { get; private set; }
#if NETFX_CORE
            public IRandomAccessStream SourceStream { get; private set; }
            public string MimeType { get; private set; }
#else
            public Stream SourceStream { get; private set; }
            public MediaStreamSource MediaStreamSource { get; private set; }
#endif
        }
    }
}
