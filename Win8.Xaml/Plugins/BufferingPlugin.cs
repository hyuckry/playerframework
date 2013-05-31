﻿#define CODE_ANALYSIS

using System.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// A plugin used to show the user that the media is buffering.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Correctly named architectural pattern")]
    public sealed class BufferingPlugin : IPlugin
    {
        BufferingView bufferingView;
        Panel bufferingContainer;

        /// <summary>
        /// Gets or sets the style to be used for the BufferingView
        /// </summary>
        public Style BufferingViewStyle { get; set; }

        /// <inheritdoc /> 
        public MediaPlayer MediaPlayer { get; set; }

        /// <inheritdoc /> 
        public void Load()
        {
            bufferingContainer = MediaPlayer.Containers.OfType<Panel>().FirstOrDefault(e => e.Name == MediaPlayerTemplateParts.BufferingContainer);
            if (bufferingContainer != null)
            {
                bufferingView = new BufferingView();
                if (BufferingViewStyle != null) bufferingView.Style = BufferingViewStyle;
                bufferingContainer.Children.Add(bufferingView);
                bufferingView.SetBinding(BufferingView.ViewModelProperty, new Binding() { Path = new PropertyPath("InteractiveViewModel"), Source = MediaPlayer });
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
            if (bufferingView != null)
            {
                bufferingView.ClearValue(BufferingView.ViewModelProperty);
                bufferingContainer.Children.Remove(bufferingView);
                bufferingView = null;
                bufferingContainer = null;
            }
        }
    }
}