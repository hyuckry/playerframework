#define CODE_ANALYSIS

using System.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// A plugin used to display a UI with a play button on it to let the user choose to load the media.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Correctly named architectural pattern")]
    public sealed class LoaderPlugin : IPlugin
    {
        LoaderView loaderViewElement;
        Panel loaderViewContainer;

        /// <summary>
        /// Gets or sets the style to be used for the ErrorView
        /// </summary>
        public Style LoaderViewStyle { get; set; }

        /// <inheritdoc /> 
        public MediaPlayer MediaPlayer { get; set; }

        /// <inheritdoc /> 
        public void Load()
        {
            loaderViewContainer = MediaPlayer.Containers.OfType<Panel>().FirstOrDefault(e => e.Name == MediaPlayerTemplateParts.LoaderViewContainer);
            if (loaderViewContainer != null)
            {
                loaderViewElement = new LoaderView()
                {
                    Style = LoaderViewStyle
                };
                loaderViewElement.Load += loaderViewElement_Load;
                loaderViewContainer.Children.Add(loaderViewElement);
            }
        }

        /// <inheritdoc /> 
        public void Update(IMediaSource mediaSource)
        {
            // nothing to do
        }

        /// <inheritdoc /> 
        public void Unload()
        {
            if (loaderViewContainer != null)
            {
                if (loaderViewElement != null)
                {
                    loaderViewElement.Load -= loaderViewElement_Load;
                    loaderViewContainer.Children.Remove(loaderViewElement);
                    loaderViewElement = null;
                }
                loaderViewContainer = null;
            }
        }

        void loaderViewElement_Load(object sender, RoutedEventArgs e)
        {
            MediaPlayer.AutoLoad = true;
        }
    }
}
