using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VideoAdvertising;
#if SILVERLIGHT
using System.Windows;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace Microsoft.PlayerFramework.Advertising
{
    /// <summary>
    /// A plugin that is capable of downloading a MAST source file, parsing it and using it to schedule when ads should play.
    /// </summary>
    public sealed class MastSchedulerPlugin :
#if SILVERLIGHT
        DependencyObject
#else
 FrameworkElement
#endif
, IPlugin
    {
        readonly Mainsail mainsail;
        readonly Dictionary<Trigger, CancellationTokenSource> activeTriggers = new Dictionary<Trigger, CancellationTokenSource>();
        MastAdapter mastAdapter;
        bool capturetriggerTask = false;
        Task triggerTask = null;
        CancellationTokenSource cts;
        CancellationTokenSource adCts;
        bool isLoaded;

        /// <summary>
        /// Creates a new instance of MastSchedulerPlugin
        /// </summary>
        public MastSchedulerPlugin()
        {
            mainsail = new Mainsail();
        }

        /// <inheritdoc /> 
        public MediaPlayer MediaPlayer { get; set; }

        /// <inheritdoc /> 
        public void Load()
        {
            cts = new CancellationTokenSource();
            WirePlayer();
            mainsail.ActivateTrigger += mainsail_ActivateTrigger;
            mainsail.DeactivateTrigger += mainsail_DeactivateTrigger;
            isLoaded = true;
        }

        /// <inheritdoc /> 
        public void Update(IMediaSource mediaSource)
        {
            CancelActiveTriggers();
            mainsail.Clear();
            Source = MastScheduler.GetSource((DependencyObject)mediaSource);
        }

        /// <inheritdoc /> 
        public void Unload()
        {
            cts.Cancel();
            cts = null;
            CancelActiveTriggers();
            mainsail.ActivateTrigger -= mainsail_ActivateTrigger;
            mainsail.DeactivateTrigger -= mainsail_DeactivateTrigger;
            UnwirePlayer();
            isLoaded = false;
        }
        
        void mainsail_DeactivateTrigger(object sender, TriggerEventArgs e)
        {
            var trigger = e.Trigger;

            if (activeTriggers.ContainsKey(trigger))
            {
                // tell the marker manager about it. This may have come from the marker manager in which case it will ignore
                var cancellationToken = activeTriggers[trigger];
                cancellationToken.Cancel();
                activeTriggers.Remove(trigger);
            }
        }

        void mainsail_ActivateTrigger(object sender, TriggerEventArgs e)
        {
            if (isLoaded)
            {
                var trigger = e.Trigger;
                if (trigger.Sources.Any())
                {
                    var source = trigger.Sources.First();
                    var remoteSource = new RemoteAdSource(new Uri(source.Uri), source.Format);
                    var cancellationToken = new CancellationTokenSource();
                    var progress = new Progress<AdStatus>();
#if NETFX_CORE
                    var task = MediaPlayer.PlayAd(remoteSource).AsTask(cancellationToken.Token, progress);
#else
                    var task = MediaPlayer.PlayAd(remoteSource, progress, cancellationToken.Token);
#endif
                    activeTriggers.Add(trigger, cancellationToken);
                    if (capturetriggerTask)
                    {
                        triggerTask = task;
                    }
                }
            }
        }

        async void mediaPlayer_MediaStarting(object sender, MediaPlayerDeferrableEventArgs e)
        {
            // we need to do a little trickery to find out if there is a preroll.
            capturetriggerTask = true;
            try
            {
                triggerTask = null;
                mastAdapter.InvokeMediaStarting(); // tell the adapter to fire the OnItemStarting event. This will cause the ActivateTrigger event to fire before we return.
                if (MediaPlayer.AllowMediaStartingDeferrals)
                {
                    if (triggerTask != null)
                    {
                        var deferral = e.DeferrableOperation.GetDeferral();
                        try
                        {
                            await triggerTask;
                        }
                        catch { /* ignore */ }
                        finally
                        {
                            deferral.Complete();
                            triggerTask = null;
                        }
                    }
                }
            }
            finally
            {
                capturetriggerTask = false;
            }
        }

        async void mediaPlayer_MediaEnding(object sender, MediaPlayerDeferrableEventArgs e)
        {
            // we need to do a little trickery to get the post roll to play and await its completion before allowing MediaEnded to fire.
            // otherwise MediaEnded will fire during the ad and the next playlistitem will start.
            capturetriggerTask = true;
            try
            {
                triggerTask = null;
                mastAdapter.InvokeMediaEnded(); // tell the adapter to fire the OnItemEnd event. This will cause the ActivateTrigger event to fire before we return.
                if (triggerTask != null)
                {
                    var deferral = e.DeferrableOperation.GetDeferral();
                    try
                    {
                        await triggerTask;
                    }
                    catch { /* ignore */ }
                    finally
                    {
                        deferral.Complete();
                        triggerTask = null;
                    }
                }
            }
            finally
            {
                capturetriggerTask = false;
            }
        }

        private void WirePlayer()
        {
            mastAdapter = new MastAdapter(MediaPlayer);
            mainsail.MastInterface = mastAdapter;
            MediaPlayer.UpdateCompleted += mediaPlayer_UpdateCompleted;
            MediaPlayer.MediaLoading += mediaPlayer_MediaLoading;
            MediaPlayer.MediaEnding += mediaPlayer_MediaEnding;
            MediaPlayer.MediaStarting += mediaPlayer_MediaStarting;
        }

        private void UnwirePlayer()
        {
            MediaPlayer.UpdateCompleted -= mediaPlayer_UpdateCompleted;
            MediaPlayer.MediaLoading -= mediaPlayer_MediaLoading;
            MediaPlayer.MediaEnding -= mediaPlayer_MediaEnding;
            MediaPlayer.MediaStarting -= mediaPlayer_MediaStarting;
            mainsail.MastInterface = null;
        }

        /// <summary>
        /// Cancels all active triggers.
        /// </summary>
        public void CancelActiveTriggers()
        {
            foreach (var cancellationToken in activeTriggers.Values)
            {
                cancellationToken.Cancel();
            }
            activeTriggers.Clear();
        }

        async void mediaPlayer_MediaLoading(object sender, MediaLoadingEventArgs e)
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
        /// <param name="source">The MAST source URI</param>
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
        /// <param name="source">The MAST source URI</param>
        /// <param name="c">A cancellation token that allows you to cancel a pending operation</param>
        /// <returns>A task to await on.</returns>
        public async Task LoadAds(Uri source, CancellationToken c)
#endif
        {
            mainsail.Clear();
            var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(c, cts.Token).Token;
#if SILVERLIGHT
            await mainsail.LoadSource(source, cancellationToken);
#else
            await mainsail.LoadSource(source).AsTask(cancellationToken);
#endif
        }

        void mediaPlayer_UpdateCompleted(object sender, RoutedEventArgs e)
        {
            if (!MediaPlayer.IsScrubbing && mastAdapter.IsPlaying)
            {
                mainsail.EvaluateTriggers();
            }
        }

        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static DependencyProperty SourceProperty { get { return sourceProperty; } }
        static readonly DependencyProperty sourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(MastSchedulerPlugin), null);

        /// <summary>
        /// Gets or sets the source Uri of the MAST file
        /// </summary>
        public Uri Source
        {
            get { return GetValue(SourceProperty) as Uri; }
            set { SetValue(SourceProperty, value); }
        }
    }
}
