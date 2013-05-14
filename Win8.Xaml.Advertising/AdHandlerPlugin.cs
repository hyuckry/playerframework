using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VideoAdvertising;
using System.ComponentModel;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Input;
#if !WINDOWS_PHONE
using System.Windows.Browser;
#else
using Microsoft.Phone.Tasks;
#endif
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.System;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace Microsoft.PlayerFramework.Advertising
{
    /// <summary>
    /// The main player framework plugin to handle ads. Ads can come from various scheduler plugins or be called directly.
    /// </summary>
    public sealed class AdHandlerPlugin : IPlugin
    {
        readonly AdHandlerController controller;
        readonly Dictionary<IVpaid, List<CancellationTokenSource>> activeIcons = new Dictionary<IVpaid, List<CancellationTokenSource>>();
        object previouscreativeConcept = null;
        List<Action> companionUnloadActions = new List<Action>();
#if WINDOWS_PHONE7
        MediaState playerState;
#endif

        /// <inheritdoc /> 
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the current active VPAID ad player. This is null until an ad is actually playing.
        /// </summary>
        public IVpaid ActiveAdPlayer
        {
            get { return controller.ActiveAdPlayer; }
        }

        /// <summary>
        /// Indicates that the active ad player has changed.
        /// </summary>
        public event EventHandler<object> ActiveAdPlayerChanged;

        /// <summary>
        /// Indicates that the advertising state has changed.
        /// </summary>
        public event EventHandler<AdStateEventArgs> AdStateChanged;

        /// <summary>
        /// Indicates that an ad failed.
        /// </summary>
        public event EventHandler<AdFailureEventArgs> AdFailure;

        /// <summary>
        /// Indicates that a new ad unit has started playing.
        /// </summary>
        public event EventHandler<ActivateAdUnitEventArgs> ActivateAdUnit;

        /// <summary>
        /// Indicates that a new ad unit has started playing.
        /// </summary>
        public event EventHandler<DeactivateAdUnitEventArgs> DeactivateAdUnit;

        public AdHandlerPlugin()
        {
            controller = new AdHandlerController();
            controller.NavigationRequest += controller_NavigationRequest;
            controller.LoadPlayer += controller_LoadPlayer;
            controller.UnloadPlayer += controller_UnloadPlayer;
            controller.ActivateAdUnit += controller_ActivateAdUnit;
            controller.DeactivateAdUnit += controller_DeactivateAdUnit;
            controller.AdStateChanged += controller_AdStateChanged;
            controller.ActiveAdPlayerChanged += controller_ActiveAdPlayerChanged;
            controller.AdFailure += controller_AdFailure;
            AutoLoadAdPlayerFactoryPlugin = true;
        }

        int preferredBitrate;
        /// <summary>
        /// the preferred bitrate for ads (in bps NOT kbps).
        /// </summary>
        public int PreferredBitrate
        {
            get { return preferredBitrate; }
            set
            {
                preferredBitrate = value;
                if (Player != null)
                {
                    Player.CurrentBitrate = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the AdPlayerFactoryPlugin should be automatically added to the plugin collection. Set to false if you are providing your own.
        /// </summary>
        public bool AutoLoadAdPlayerFactoryPlugin { get; set; }

        /// <summary>
        /// Called to retrieve a VPAID player from a creative source.
        /// </summary>
        /// <param name="creativeSource">The creative source that needs a VPAID player to play.</param>
        /// <returns>The VPAID ad player</returns>
        IVpaid GetPlayer(ICreativeSource creativeSource)
        {
            IVpaid result = null;
            // look for ad player factories in the plugin collection. Try each one until you find a player
            foreach (var factory in MediaPlayer.Plugins.OfType<IAdPlayerFactoryPlugin>())
            {
                var player = factory.GetPlayer(creativeSource);
                if (player != null)
                {
                    result = player;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Called when the container for a companion ad needs to be displayed.
        /// </summary>
        /// <param name="source">The source info for the companion ad.</param>
        /// <returns>The element in the UI to place the companion ad.</returns>
        FrameworkElement GetCompanionContainer(ICompanionSource source)
        {
            FrameworkElement container = null;
            if (!string.IsNullOrEmpty(source.AdSlotId))
            {
                container = MediaPlayer.Containers.OfType<FrameworkElement>().FirstOrDefault(f => f.Name == source.AdSlotId);
            }
            if (container == null && source.Width.HasValue && source.Height.HasValue)
            {
                container = MediaPlayer.Containers.OfType<FrameworkElement>().FirstOrDefault(f => f.Width == source.Width && f.Height == source.Height);
            }
            return container;
        }

        /// <summary>
        /// Called when the advertising state has changed.
        /// Allows the subclass to change the UI or allowed behaviors appropriately.
        /// </summary>
        /// <param name="adState">The new advertising state</param>
#if WINDOWS_PHONE7
        async void SetAdvertisingState(AdState adState)
#else
        void SetAdvertisingState(AdState adState)
#endif
        {
            var newValue = ConvertAdState(adState);
            var oldValue = MediaPlayer.AdvertisingState;

            if (newValue != oldValue)
            {
                if (MediaPlayer.PlayerState == PlayerState.Started)
                {
                    // pause the MediaPlayer if we're playing a linear ad or loading an ad, resume if the opposite
                    if ((newValue == AdvertisingState.Loading || newValue == AdvertisingState.Linear) && (oldValue == AdvertisingState.None || oldValue == AdvertisingState.NonLinear))
                    {
#if WINDOWS_PHONE7
                        playerState = MediaPlayer.GetMediaState();
                        MediaPlayer.Close();
#else
                        MediaPlayer.Pause();
#endif
                    }
                    else if ((oldValue == AdvertisingState.Loading || oldValue == AdvertisingState.Linear) && (newValue == AdvertisingState.None || newValue == AdvertisingState.NonLinear))
                    {
#if WINDOWS_PHONE7
                        MediaPlayer.RestoreMediaState(playerState);
#else
                        MediaPlayer.Play();
#endif
                    }
                }

                if (newValue == AdvertisingState.Loading)
                {
                    adContainer.Visibility = Visibility.Visible;
                }
                else if (newValue == AdvertisingState.None)
                {
                    adContainer.Visibility = Visibility.Collapsed;
                }

                // let the MediaPlayer update its visualstate
                MediaPlayer.AdvertisingState = newValue;
            }

            switch (newValue)
            {
                case AdvertisingState.Linear:
                    MediaPlayer.InteractiveViewModel = new VpaidLinearAdViewModel(ActiveAdPlayer, MediaPlayer);
                    break;
                case AdvertisingState.NonLinear:
                    MediaPlayer.InteractiveViewModel = new VpaidNonLinearAdViewModel(ActiveAdPlayer, MediaPlayer);
                    break;
                default:
                    MediaPlayer.InteractiveViewModel = MediaPlayer.DefaultInteractiveViewModel;
                    break;
            }
        }

        static AdvertisingState ConvertAdState(AdState adState)
        {
            switch (adState)
            {
                case AdState.Linear: return AdvertisingState.Linear;
                case AdState.NonLinear: return AdvertisingState.NonLinear;
                case AdState.Loading: return AdvertisingState.Loading;
                case AdState.None: return AdvertisingState.None;
                default: throw new NotImplementedException();
            }
        }

        void MediaPlayer_PlayerStateChanged(object sender, PlayerStateChangedEventArgs e)
        {
            // hide the main media container for pre-rolls. Revert back once the media has started
            var mediaContainer = MediaPlayer.Containers.OfType<FrameworkElement>().FirstOrDefault(f => f.Name == MediaPlayerTemplateParts.MediaContainer);
            if (mediaContainer != null)
            {
                if (e.NewValue == PlayerState.Loaded && MediaPlayer.AutoPlay)
                {
                    mediaContainer.Visibility = Visibility.Collapsed;
                }
                else if (e.NewValue != PlayerState.Opened && e.NewValue != PlayerState.Starting)
                {
                    mediaContainer.Visibility = Visibility.Visible;
                }
            }
        }

        void MediaPlayer_MediaClosed(object sender, object e)
        {
            // always close all active ads when the media is closed
            var task = CancelActiveAds();
            UnloadCompanions();
        }

        void IPlugin.Load()
        {
            Player = new MediaPlayerAdapter(MediaPlayer) { CurrentBitrate = PreferredBitrate };
            adContainer = MediaPlayer.Containers.OfType<Panel>().FirstOrDefault(f => f.Name == MediaPlayerTemplateParts.AdvertisingContainer);

            // look for adhandler in the plugin collection first
            foreach (var handler in MediaPlayer.Plugins.OfType<IAdPayloadHandler>())
            {
                AdHandlers.Add(handler);
            }

            MediaPlayer.PlayerStateChanged += MediaPlayer_PlayerStateChanged;
            MediaPlayer.MediaClosed += MediaPlayer_MediaClosed;

            if (AutoLoadAdPlayerFactoryPlugin)
            {
                MediaPlayer.Plugins.Add(new AdPlayerFactoryPlugin());
            }
        }

        void IPlugin.Update(IMediaSource mediaSource)
        {
            // do nothing
        }

        void IPlugin.Unload()
        {
            MediaPlayer.MediaClosed -= MediaPlayer_MediaClosed;
            MediaPlayer.PlayerStateChanged -= MediaPlayer_PlayerStateChanged;
        }

        /// <inheritdoc /> 
        public MediaPlayer MediaPlayer { get; set; }

        /// <summary>
        /// Gets or set the player adapter needed by the VideoAdvertising component.
        /// </summary>
        IPlayer Player
        {
            get { return controller.Player; }
            set { controller.Player = value; }
        }

#if NETFX_CORE
        /// <summary>
        /// Cancels all active ads.
        /// </summary>
        public IAsyncAction CancelActiveAds()
        {
            return controller.CancelActiveAds();
        }
#else
        /// <summary>
        /// Cancels all active ads.
        /// </summary>
        public async Task CancelActiveAds()
        {
            await controller.CancelActiveAds();
        }
#endif

        void controller_AdFailure(object sender, AdFailureEventArgs e)
        {
            if (AdFailure != null) AdFailure(this, e);
        }

#if SILVERLIGHT
        void controller_ActiveAdPlayerChanged(object sender, EventArgs e)
#else
        void controller_ActiveAdPlayerChanged(object sender, object e)
#endif
        {
            if (ActiveAdPlayerChanged != null)
            {
                ActiveAdPlayerChanged(this, EventArgs.Empty);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActiveAdPlayer"));
            }
        }

#if SILVERLIGHT
        void controller_AdStateChanged(object sender, EventArgs e)
#else
        void controller_AdStateChanged(object sender, object e)
#endif
        {
            SetAdvertisingState(controller.AdState);
            if (AdStateChanged != null) AdStateChanged(this, new AdStateEventArgs(controller.AdState));
        }

        void controller_NavigationRequest(object sender, NavigationRequestEventArgs e)
        {
            RequestNavigation(e.Url);
        }

        void controller_UnloadPlayer(object sender, UnloadPlayerEventArgs e)
        {
            UnloadPlayer(e.Player);
        }

        void controller_LoadPlayer(object sender, LoadPlayerEventArgs e)
        {
            e.Player = GetPlayer(e.CreativeSource);
            if (e.Player != null)
            {
                LoadPlayer(e.Player);
            }
        }

        void controller_ActivateAdUnit(object sender, ActivateAdUnitEventArgs e)
        {
            if (ActivateAdUnit != null) ActivateAdUnit(this, e);
            // show companions
            LoadCompanions(e.Companions, e.SuggestedCompanionRules, e.CreativeSource, e.Player, e.CreativeConcept, e.AdSource);
            // show icons
            if (e.CreativeSource.Icons != null)
            {
                var vpaid2 = e.Player as IVpaid2;
                var preventIcons = vpaid2 != null ? vpaid2.AdIcons : false;
                if (!preventIcons)
                {
                    var canellationTokens = new List<CancellationTokenSource>();
                    activeIcons.Add(e.Player, canellationTokens);
                    foreach (var icon in e.CreativeSource.Icons)
                    {
                        var staticResource = icon.Item as StaticResource;
                        if (staticResource != null)
                        {
                            CancellationTokenSource cts = new CancellationTokenSource();
                            canellationTokens.Add(cts);
                            ShowIcon(icon, staticResource, cts);
                        }
                    }
                }
            }
        }

        /// <inheritdoc /> 
        async void ShowIcon(Microsoft.VideoAdvertising.Icon icon, StaticResource staticResource, CancellationTokenSource cts)
        {
            if (icon.Offset.HasValue)
            {
                try
                {
#if SILVERLIGHT && !WINDOWS_PHONE || WINDOWS_PHONE7
                    await TaskEx.Delay((int)icon.Offset.Value.TotalMilliseconds, cts.Token);
#else
                    await Task.Delay((int)icon.Offset.Value.TotalMilliseconds, cts.Token);
#endif
                }
                catch (OperationCanceledException) { /* swallow */ }
            }

            if (!cts.IsCancellationRequested)
            {
                var iconHost = new HyperlinkButton();
                iconHost.NavigateUri = icon.ClickThrough;

                double topMargin = 0;
                double leftMargin = 0;
                if (icon.Width.HasValue)
                {
                    iconHost.Width = icon.Width.Value;
                }
                if (icon.Height.HasValue)
                {
                    iconHost.Height = icon.Height.Value;
                }
                switch (icon.XPosition)
                {
                    case "left":
                        iconHost.HorizontalAlignment = HorizontalAlignment.Left;
                        break;
                    case "right":
                        iconHost.HorizontalAlignment = HorizontalAlignment.Right;
                        break;
                    default:
                        iconHost.HorizontalAlignment = HorizontalAlignment.Left;
                        int xPositionValue;
                        if (int.TryParse(icon.XPosition, out xPositionValue))
                        {
                            leftMargin = xPositionValue;
                        }
                        break;
                }
                switch (icon.YPosition)
                {
                    case "top":
                        iconHost.VerticalAlignment = VerticalAlignment.Top;
                        break;
                    case "bottom":
                        iconHost.VerticalAlignment = VerticalAlignment.Bottom;
                        break;
                    default:
                        iconHost.VerticalAlignment = VerticalAlignment.Top;
                        int yPositionValue;
                        if (int.TryParse(icon.YPosition, out yPositionValue))
                        {
                            topMargin = yPositionValue;
                        }
                        break;
                }
                iconHost.Margin = new Thickness(leftMargin, topMargin, 0, 0);

                var iconElement = new Image();
                iconElement.Stretch = Stretch.Fill;
                iconElement.Source = new BitmapImage(staticResource.Value);
                iconHost.Content = iconElement;

                //TODO: avoid visually overlaping icons

                iconHost.Tag = icon;
                iconElement.Tag = icon;

                iconHost.Click += iconHost_Click;
                iconElement.ImageOpened += iconElement_ImageOpened;
                adContainer.Children.Add(iconHost);

                try
                {
                    if (icon.Duration.HasValue)
                    {
#if SILVERLIGHT && !WINDOWS_PHONE || WINDOWS_PHONE7
                        await TaskEx.Delay((int)icon.Duration.Value.TotalMilliseconds, cts.Token);
#else
                        await Task.Delay((int)icon.Duration.Value.TotalMilliseconds, cts.Token);
#endif
                    }
                    else
                    {
                        await cts.Token.AsTask();
                    }
                }
                catch (OperationCanceledException) { /* swallow */ }
                finally
                {
                    adContainer.Children.Remove(iconHost);
                    iconHost.Click -= iconHost_Click;
                    iconElement.ImageOpened -= iconElement_ImageOpened;
                }
            }
        }

        void iconElement_ImageOpened(object sender, RoutedEventArgs e)
        {
            var icon = (Microsoft.VideoAdvertising.Icon)((FrameworkElement)sender).Tag;
            foreach (var url in icon.ViewTracking)
            {
                VastHelpers.FireTracking(url);
            }
        }

        void iconHost_Click(object sender, RoutedEventArgs e)
        {
            var icon = (Microsoft.VideoAdvertising.Icon)((FrameworkElement)sender).Tag;
            foreach (var url in icon.ClickTracking)
            {
                VastHelpers.FireTracking(url);
            }
        }

        void controller_DeactivateAdUnit(object sender, DeactivateAdUnitEventArgs e)
        {
            if (DeactivateAdUnit != null) DeactivateAdUnit(this, e);
            if (e.Error != null)
            {
                UnloadCompanions();
            }
            // hide all icons
            HideIcons(e.Player);
        }

        private void HideIcons(IVpaid player)
        {
            if (activeIcons.ContainsKey(player))
            {
                foreach (var cts in activeIcons[player])
                {
                    cts.Cancel();
                }
                activeIcons.Remove(player);
            }
        }

        #region Hiding and showing players and companions
        /// <summary>
        /// Gets or sets the container to show the ads in. Note: this does not apply to companion ads.
        /// </summary>
        Panel adContainer;

        /// <summary>
        /// Called when an ad player should be unloaded.
        /// </summary>
        /// <param name="player">The VPAID player to unload.</param>
        void UnloadPlayer(IVpaid player)
        {
            var uiElement = player as UIElement;
            adContainer.Children.Remove(uiElement);
            MediaPlayer.RemoveInteractiveElement(uiElement);
        }

        /// <summary>
        /// Called when a new VPAID player should be loaded.
        /// This does not mean it should be made visibile since it could be pre-loading.
        /// </summary>
        /// <param name="adPlayer">The VPAID player to load.</param>
        void LoadPlayer(IVpaid adPlayer)
        {
            // set visibility to support preloading. MediaElement won't work unless it is contained in a visible parent
            adContainer.Visibility = Visibility.Visible;
            var uiElement = adPlayer as UIElement;
            adContainer.Children.Add(uiElement);
            MediaPlayer.AddInteractiveElement(uiElement, false);
        }

        /// <summary>
        /// Called to help load companion ads.
        /// </summary>
        /// <param name="companions">The companion ads that should show.</param>
        /// <param name="suggestedCompanionRules">The suggested rules for how to show companions.</param>
        /// <param name="creativeSource">The creative source associated with the companions.</param>
        /// <param name="adPlayer">The VPAID ad player associated with the companions.</param>
        /// <param name="creativeConcept">The creative concept for the companions. Can help provide info to assist with companion life cycle business logic.</param>
        /// <param name="adSource">The ad source from which the companion ads came.</param>
        void LoadCompanions(IEnumerable<ICompanionSource> companions, CompanionAdsRequired suggestedCompanionRules, ICreativeSource creativeSource, IVpaid adPlayer, object creativeConcept, IAdSource adSource)
        {
            if (previouscreativeConcept != null && creativeConcept != previouscreativeConcept)
            {
                // remove all old companions
                UnloadCompanions();
            }

            int failureCount = 0;
            int total = 0;

            companionUnloadActions.Clear();
            try
            {
                if (companions != null)
                {
                    foreach (var companion in companions)
                    {
                        Action undoAction = TryLoadCompanion(companion);
                        if (undoAction == null)
                        {
                            failureCount++;
                        }
                        else
                        {
                            companionUnloadActions.Add(undoAction);
                        }
                        total++;
                    }
                }

                if (suggestedCompanionRules == CompanionAdsRequired.Any && total > 0 && failureCount == total) throw new Exception("All companion ads failed");
                if (suggestedCompanionRules == CompanionAdsRequired.All && failureCount > 0) throw new Exception("Not all companion ads succeeded");

                previouscreativeConcept = creativeConcept;
            }
            catch
            {
                UnloadCompanions();
                throw;
            }
        }

        /// <summary>
        /// Loads the companion ad into the UI.
        /// </summary>
        /// <param name="source">Source information for the companion ad</param>
        /// <returns>An action to undo the loaded companion if successful. Null if not.</returns>
        internal Action TryLoadCompanion(ICompanionSource source)
        {
            if ((source.Type == CompanionType.Static))
            {
                FrameworkElement container = GetCompanionContainer(source);
                if (container != null)
                {
                    var companionHost = new HyperlinkButton();
                    var companionElement = new Image()
                    {
                        Source = new BitmapImage(new Uri(source.Content)),
                        Stretch = Stretch.Fill,
                        Tag = source
                    };
                    companionHost.Content = companionElement;
                    companionHost.NavigateUri = source.ClickThrough;
                    companionHost.Tag = source;

                    if (!string.IsNullOrEmpty(source.AltText))
                    {
                        ToolTipService.SetToolTip(companionHost, new ToolTip() { Content = source.AltText });
                    }

                    Action unwireViewTrackingAction = null;
                    if (source.ViewTracking != null && source.ViewTracking.Any())
                    {
                        companionElement.ImageOpened += companionElement_ImageOpened;
                        unwireViewTrackingAction = () => companionElement.ImageOpened -= companionElement_ImageOpened;
                    }

                    Action unwireClickTrackingAction = null;
                    if (source.ClickTracking != null && source.ClickTracking.Any())
                    {
                        companionHost.Click += companionHost_Click;
                        unwireClickTrackingAction = () => companionHost.Click -= companionHost_Click;
                    }

                    if (container is Border)
                    {
                        ((Border)container).Child = companionHost;
                        return () =>
                        {
                            if (unwireClickTrackingAction != null) unwireClickTrackingAction();
                            if (unwireViewTrackingAction != null) unwireViewTrackingAction();
                            ((Border)container).Child = null;
                        };
                    }
                    else if (container is Panel)
                    {
                        ((Panel)container).Children.Add(companionHost);
                        return () =>
                        {
                            if (unwireClickTrackingAction != null) unwireClickTrackingAction();
                            if (unwireViewTrackingAction != null) unwireViewTrackingAction();
                            ((Panel)container).Children.Remove(companionHost);
                        };
                    }
                    else if (container is ContentControl)
                    {
                        ((ContentControl)container).Content = companionHost;
                        return () =>
                        {
                            if (unwireClickTrackingAction != null) unwireClickTrackingAction();
                            if (unwireViewTrackingAction != null) unwireViewTrackingAction();
                            ((ContentControl)container).Content = null;
                        };
                    }
                    if (unwireClickTrackingAction != null) unwireClickTrackingAction();
                    if (unwireViewTrackingAction != null) unwireViewTrackingAction();
                }
            }
            return null;
        }
        
        void companionElement_ImageOpened(object sender, RoutedEventArgs e)
        {
            var source = (ICompanionSource)((FrameworkElement)sender).Tag;
            foreach (var url in source.ViewTracking)
            {
                VastHelpers.FireTracking(url);
            }
        }

        void companionHost_Click(object sender, RoutedEventArgs e)
        {
            var source = (ICompanionSource)((FrameworkElement)sender).Tag;
            foreach (var url in source.ClickTracking)
            {
                VastHelpers.FireTracking(url);
            }
        }

        /// <summary>
        /// Unloads all companion ads.
        /// </summary>
        public void UnloadCompanions()
        {
            foreach (var unloadAction in companionUnloadActions)
            {
                unloadAction();
            }
            companionUnloadActions.Clear();
            previouscreativeConcept = null;
        }

        #endregion

        /// <summary>
        /// Called when navigation is requested from a click on an ad.
        /// </summary>
        /// <param name="url"></param>
#if SILVERLIGHT
        void RequestNavigation(string url)
#else
        async void RequestNavigation(string url)
#endif
        {
            if (!string.IsNullOrEmpty(url))
            {
#if WINDOWS_PHONE 
                WebBrowserTask task = new WebBrowserTask(); 
                task.Uri = new Uri(url); 
                task.Show(); 
#elif SILVERLIGHT
                HtmlPage.Window.Navigate(new Uri(url), "_blank");
#elif NETFX_CORE
                await Launcher.LaunchUriAsync(new Uri(url));
#endif
            }
        }

        /// <summary>
        /// The timeout for an ad to start playing. If this is exceeded, the ad will be skipped
        /// </summary>
        public TimeSpan? StartTimeout
        {
            get { return controller.StartTimeout; }
            set { controller.StartTimeout = value; }
        }

#if NETFX_CORE
        /// <summary>
        /// Preloads an ad so it is ready to play immediately at the right time.
        /// </summary>
        /// <param name="source">The source of the ad.</param>
        /// <returns>An awaitable Task that returns when the ad is done preloading.</returns>
        public IAsyncAction PreloadAd(IAdSource source)
        {
            return AsyncInfo.Run(c => preloadAd(source, c));
        }

        internal Task preloadAd(IAdSource source, CancellationToken cancellationToken)
        {
            return controller.PreloadAdAsync(source).AsTask(cancellationToken);
        }
#else
        /// <summary>
        /// Preloads an ad so it is ready to play immediately at the right time.
        /// </summary>
        /// <param name="source">The source of the ad.</param>
        /// <param name="cancellationToken">A cancellation token that can later be used to abort the ad.</param>
        /// <returns>An awaitable Task that returns when the ad is done preloading.</returns>
        public Task PreloadAd(IAdSource source, CancellationToken cancellationToken)
        {
            return controller.PreloadAdAsync(source, cancellationToken);
        }
#endif

#if NETFX_CORE
        /// <summary>
        /// Plays an ad.
        /// </summary>
        /// <param name="source">The source of the ad.</param>
        /// <param name="cancellationToken">A cancellation token that can later be used to abort the ad.</param>
        /// <param name="progress">An object that allows the progress to be monitored</param>
        /// <returns>An awaitable Task that returns when the ad is over, fails, or turns into a nonlinear ad.</returns>
        public IAsyncActionWithProgress<AdStatus> PlayAd(IAdSource source)
        {
            return controller.PlayAdAsync(source);
        }
#else
        /// <summary>
        /// Plays an ad.
        /// </summary>
        /// <param name="source">The source of the ad.</param>
        /// <param name="cancellationToken">A cancellation token that can later be used to abort the ad.</param>
        /// <param name="progress">An object that allows the progress to be monitored</param>
        /// <returns>An awaitable Task that returns when the ad is over, fails, or turns into a nonlinear ad.</returns>
        public Task PlayAd(IAdSource source, CancellationToken cancellationToken, IProgress<AdStatus> progress)
        {
            return controller.PlayAdAsync(source, cancellationToken, progress);
        }
#endif

        /// <summary>
        /// Gets a list of AdHandlers that are capable of playing different types of ads.
        /// </summary>
        public IList<IAdPayloadHandler> AdHandlers
        {
            get { return controller.AdPayloadHandlers; }
        }
    }

    /// <summary>
    /// Supplies the ad state for the AdStateChangedEventArgs
    /// </summary>
    public sealed class AdStateEventArgs
#if SILVERLIGHT
 : EventArgs
#endif
    {
        internal AdStateEventArgs(AdState adState)
        {
            AdState = adState;
        }

        /// <summary>
        /// The new ad state.
        /// </summary>
        public AdState AdState { get; private set; }
    }
}
