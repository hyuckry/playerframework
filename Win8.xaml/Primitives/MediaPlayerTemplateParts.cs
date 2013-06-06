
namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Helper class that includes string names of template parts for the media player
    /// </summary>
    public static class MediaPlayerTemplateParts
    {
        internal const string mediaElement = "Media";
        internal const string layoutRootElement = "LayoutRoot";
        internal const string mediaContainer = "MediaContainer";
        internal const string loaderViewContainer = "LoaderViewContainer";
        internal const string captionsContainer = "CaptionsContainer";
        internal const string advertisingContainer = "AdvertisingContainer";
        internal const string bufferingContainer = "BufferingContainer";
        internal const string errorsContainer = "ErrorsContainer";
        internal const string interactivityContainer = "InteractivityContainer";
        internal const string settingsContainer = "SettingsContainer";
        internal const string controlPanel = "ControlPanel";
#if SILVERLIGHT
        internal const string posterContainer = "PosterContainer";
#endif

        /// <summary>
        /// The name of the container most appropriate to place the MediaElement in
        /// </summary>
        public static string MediaContainer { get { return mediaContainer; } }
        /// <summary>
        /// The name of the container most appropriate to place an interactive UI before the media loads
        /// </summary>
        public static string LoaderViewContainer { get { return loaderViewContainer; } }
        /// <summary>
        /// The name of the container most appropriate to place closed captioning
        /// </summary>
        public static string CaptionsContainer { get { return captionsContainer; } }
        /// <summary>
        /// The name of the container most appropriate to place linear advertisements
        /// </summary>
        public static string AdvertisingContainer { get { return advertisingContainer; } }
        /// <summary>
        /// The name of the container most appropriate to place a buffering UI
        /// </summary>
        public static string BufferingContainer { get { return bufferingContainer; } }
        /// <summary>
        /// The name of the container most appropriate to place information about errors that prevent the media from playing
        /// </summary>
        public static string ErrorsContainer { get { return errorsContainer; } }
        /// <summary>
        /// The name of the container most appropriate to place interactive UI elements such as the control panel
        /// </summary>
        public static string InteractivityContainer { get { return interactivityContainer; } }
        /// <summary>
        /// The name of the container most appropriate to place settings dialogs
        /// </summary>
        public static string SettingsContainer { get { return settingsContainer; } }
        /// <summary>
        /// The name of the control panel element
        /// </summary>
        public static string ControlPanel { get { return controlPanel; } }
#if SILVERLIGHT
        /// <summary>
        /// The name of the container most appropriate to place a poster image
        /// </summary>
        public static string PosterContainer  { get { return posterContainer; } }
#endif
    }
}
