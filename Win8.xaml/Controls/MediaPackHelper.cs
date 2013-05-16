﻿using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Popups;

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Represents a class that can be used to detect if the Media Feature Pack is required for Windows 8 N/KN users.
    /// </summary>
    public static class MediaPackHelper
    {
        static Uri mediaPackUri = new Uri("http://www.microsoft.com/en-ie/download/details.aspx?id=30685");
        
        /// <summary>
        /// Gets or sets the download url for the media feature pack.
        /// </summary>
        public static Uri MediaPackUri
        {
            get { return mediaPackUri; }
            set { mediaPackUri = value; }
        }

        /// <summary>
        /// Determines if the Media Feature Pack is required.
        /// </summary>
        /// <returns>A boolean indicating if it is required.</returns>
        public static bool IsMediaPackRequired()
        {
            try
            {
                var junk = Windows.Media.VideoEffects.VideoStabilization;
            }
            catch (TypeLoadException)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Performs a test and prompts the user about installing the Media Feature Pack if it is required.
        /// </summary>
        /// <returns>An awaitable task that returns true if the media feature pack is installed, false if not.</returns>
#if NETFX_CORE
        public static IAsyncOperation<bool> TestForMediaPack()
        {
            return AsyncInfo.Run(c => testForMediaPack());
        }

        static async Task<bool> testForMediaPack()
#else
        public static async Task<bool> TestForMediaPack()
#endif
        {
            if (IsMediaPackRequired())
            {
                await PromptForMediaPack();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Prompts the user to install the Media Feature Pack.
        /// </summary>
        /// <returns>An awaitable task that returns when the prompt has completed.</returns>
#if NETFX_CORE
        public static IAsyncAction PromptForMediaPack()
        {
            return AsyncInfo.Run(c => promptForMediaPack());
        }

        static async Task promptForMediaPack()
#else
        public static async Task PromptForMediaPack()
#endif
        {
            var messageDialog = new MessageDialog(MediaPlayer.GetResourceString("MediaFeaturePackRequiredLabel"), MediaPlayer.GetResourceString("MediaFeaturePackRequiredText"));
            var cmdDownload = new UICommand(MediaPlayer.GetResourceString("MediaFeaturePackDownloadLabel"));
            var cmdCancel = new UICommand(MediaPlayer.GetResourceString("MediaFeaturePackCancelLabel"));
            messageDialog.Commands.Add(cmdDownload);
            messageDialog.Commands.Add(cmdCancel);
            var cmd = await messageDialog.ShowAsync();
            if (cmd == cmdDownload)
            {
                await Launcher.LaunchUriAsync(MediaPackUri);
            }
        }
    }
}
