using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VideoAdvertising;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;

namespace Microsoft.PlayerFramework.Advertising
{
    /// <summary>
    /// A helper extension class to make it easy to perform advertising related functions.
    /// </summary>
    public static class MediaPlayerExtensions
    {
        /// <summary>
        /// Plays a simple linear clip
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to play the clip in.</param>
        /// <param name="clipSource">The source Uri of the clip.</param>
        /// <returns>An awaitable task that returns true if the clip was played.</returns>
#if NETFX_CORE
        public static IAsyncOperationWithProgress<bool, AdStatus> PlayLinearClip(this MediaPlayer mediaPlayer, Uri clipSource)
        {
            var adSource = new AdSource(clipSource, ClipAdPayloadHandler.AdType);
            return mediaPlayer.PlayAd(adSource);
        }
#else
        /// <summary>
        /// Plays a simple linear clip
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to play the clip in.</param>
        /// <param name="clipSource">The source Uri of the clip.</param>
        /// <param name="progress">An object that allows progress to be reported.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An awaitable task that returns true if the clip was played.</returns>
        public static Task<bool> PlayLinearClip(this MediaPlayer mediaPlayer, Uri clipSource, IProgress<AdStatus> progress, CancellationToken cancellationToken)
        {
            var adSource = new AdSource(clipSource, ClipAdPayloadHandler.AdType);
            return mediaPlayer.PlayAd(adSource, progress, cancellationToken);
        }
#endif

        /// <summary>
        /// Plays an ad.
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to play the ad in.</param>
        /// <param name="adSource">An object that defines the source of the ad.</param>
        /// <returns>An awaitable task that returns true if the ad was played.</returns>
#if NETFX_CORE
        public static IAsyncOperationWithProgress<bool, AdStatus> PlayAd(this MediaPlayer mediaPlayer, IAdSource adSource)
        {
            var adPlugin = mediaPlayer.Plugins.OfType<AdHandlerPlugin>().FirstOrDefault();
            if (adPlugin != null)
            {
                return AsyncInfo.Run<bool, AdStatus>(async (c, p) =>
                {
                    await adPlugin.PlayAd(adSource).AsTask(c, p);
                    return true;
                });
            }
            else
            {
                return AsyncInfo.Run<bool, AdStatus>((c, p) => Task.FromResult(false));
            }
        }
#else
        /// <summary>
        /// Plays an ad.
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to play the ad in.</param>
        /// <param name="adSource">An object that defines the source of the ad.</param>
        /// <param name="progress">An object that allows progress to be reported.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An awaitable task that returns true if the ad was played.</returns>
        public static async Task<bool> PlayAd(this MediaPlayer mediaPlayer, IAdSource adSource, IProgress<AdStatus> progress, CancellationToken cancellationToken)
        {
            var adPlugin = mediaPlayer.Plugins.OfType<AdHandlerPlugin>().FirstOrDefault();
            if (adPlugin != null)
            {
                await adPlugin.PlayAd(adSource, cancellationToken, progress);
                return true;
            }
            else
            {
                return false;
            }
        }
#endif

#if NETFX_CORE
        /// <summary>
        /// Preloads an ad so it is ready to play instantly when PlayAd is called.
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to play the ad in.</param>
        /// <param name="adSource">An object that defines the source of the ad.</param>
        /// <returns>An awaitable task that returns when the ad is done preloading.</returns>
        public static IAsyncAction PreloadAd(this MediaPlayer mediaPlayer, IAdSource adSource)
        {
            return AsyncInfo.Run(c => preloadAd(mediaPlayer, adSource, c));
        }

        internal static Task preloadAd(this MediaPlayer mediaPlayer, IAdSource adSource, CancellationToken cancellationToken)
        {
            var adPlugin = mediaPlayer.Plugins.OfType<AdHandlerPlugin>().FirstOrDefault();
            if (adPlugin != null)
            {
                return adPlugin.preloadAd(adSource, cancellationToken);
            }
            return null;
        }
#else
        /// <summary>
        /// Preloads an ad so it is ready to play instantly when PlayAd is called.
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to play the ad in.</param>
        /// <param name="adSource">An object that defines the source of the ad.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An awaitable task that returns when the ad is done preloading.</returns>
        public static Task PreloadAd(this MediaPlayer mediaPlayer, IAdSource adSource, CancellationToken cancellationToken)
        {
            var adPlugin = mediaPlayer.Plugins.OfType<AdHandlerPlugin>().FirstOrDefault();
            if (adPlugin != null)
            {
                return adPlugin.PreloadAd(adSource, cancellationToken);
            }
            return null;
        }
#endif

        /// <summary>
        /// Shows a companion ad not associated with a linear ad.
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to play the ad in.</param>
        /// <param name="companionSource">An model containing the companion ad to show.</param>
        public static void ShowCompanion(this MediaPlayer mediaPlayer, ICompanionSource companionSource)
        {
            var adPlugin = mediaPlayer.Plugins.OfType<AdHandlerPlugin>().FirstOrDefault();
            if (adPlugin != null)
            {
                Action undoAction = adPlugin.TryLoadCompanion(companionSource);
                if (undoAction == null)
                {
                    throw new Exception("Unable to play companion ad");
                }
            }
        }

        /// <summary>
        /// Unloads all companion ads.
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to play the ad in.</param>
        public static void UnloadAllCompanions(this MediaPlayer mediaPlayer)
        {
            var adPlugin = mediaPlayer.Plugins.OfType<AdHandlerPlugin>().FirstOrDefault();
            if (adPlugin != null)
            {
                adPlugin.UnloadCompanions();
            }
        }

        /// <summary>
        /// Gets a reference to the active AdSchedulerPlugin.
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to get the associated plugin for.</param>
        /// <returns>A reference to the plugin.</returns>
        public static AdSchedulerPlugin GetAdSchedulerPlugin(this MediaPlayer mediaPlayer)
        {
            return mediaPlayer.Plugins.OfType<AdSchedulerPlugin>().FirstOrDefault();
        }

        /// <summary>
        /// Gets a reference to the active MastSchedulerPlugin.
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to get the associated plugin for.</param>
        /// <returns>A reference to the plugin.</returns>
        public static MastSchedulerPlugin GetMastSchedulerPlugin(this MediaPlayer mediaPlayer)
        {
            return mediaPlayer.Plugins.OfType<MastSchedulerPlugin>().FirstOrDefault();
        }

        /// <summary>
        /// Gets a reference to the active VmapSchedulerPlugin.
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to get the associated plugin for.</param>
        /// <returns>A reference to the plugin.</returns>
        public static VmapSchedulerPlugin GetVmapSchedulerPlugin(this MediaPlayer mediaPlayer)
        {
            return mediaPlayer.Plugins.OfType<VmapSchedulerPlugin>().FirstOrDefault();
        }

        /// <summary>
        /// Gets a reference to the active FreeWheelPlugin (used to advertise with FreeWheel).
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to get the associated plugin for.</param>
        /// <returns>A reference to the plugin.</returns>
        public static FreeWheelPlugin GetFreeWheelPlugin(this MediaPlayer mediaPlayer)
        {
            return mediaPlayer.Plugins.OfType<FreeWheelPlugin>().FirstOrDefault();
        }

        /// <summary>
        /// Gets a reference to the active AdHandlerPlugin.
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to get the associated plugin for.</param>
        /// <returns>A reference to the plugin.</returns>
        public static AdHandlerPlugin GetAdHandlerPlugin(this MediaPlayer mediaPlayer)
        {
            return mediaPlayer.Plugins.OfType<AdHandlerPlugin>().FirstOrDefault();
        }

        /// <summary>
        /// Gets a reference to the active AdPlayerFactoryPlugin.
        /// </summary>
        /// <param name="mediaPlayer">The MediaPlayer instance to get the associated plugin for.</param>
        /// <returns>A reference to the plugin.</returns>
        public static AdPlayerFactoryPlugin GetAdPlayerFactoryPlugin(this MediaPlayer mediaPlayer)
        {
            return mediaPlayer.Plugins.OfType<AdPlayerFactoryPlugin>().FirstOrDefault();
        }
    }
}
