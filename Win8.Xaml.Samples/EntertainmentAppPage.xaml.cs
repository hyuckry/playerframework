﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Microsoft.PlayerFramework.Samples
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EntertainmentAppPage : Microsoft.PlayerFramework.Samples.Common.LayoutAwarePage
    {
        public EntertainmentAppPage()
        {
            this.InitializeComponent();
            UpdateViewModel(player.InteractiveViewModel);
            player.InteractiveViewModelChanged += player_InteractiveViewModelChanged;

            ReplayButton.Loaded += StartLayoutUpdates;
            ReplayButton.Unloaded += StopLayoutUpdates;
            CaptionSelectionButton.Loaded += StartLayoutUpdates;
            CaptionSelectionButton.Unloaded += StopLayoutUpdates;
            AudioSelectionButton.Loaded += StartLayoutUpdates;
            AudioSelectionButton.Unloaded += StopLayoutUpdates;

            // register the control panel so it participates in view state changes
            player.Initialized += player_Initialized;
        }

        void player_Initialized(object sender, object e)
        {
            player.ControlPanel.Loaded += StartLayoutUpdates;
            player.ControlPanel.Unloaded += StopLayoutUpdates;

            var audioSelectionPlugin = player.Plugins.OfType<AudioSelectionPlugin>().FirstOrDefault();
            audioSelectionPlugin.AudioSelectionViewStyle = new Style(typeof(AudioSelectionView));
            audioSelectionPlugin.AudioSelectionViewStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(0, 0, 0, 90)));

            var captionSelectionPlugin = player.Plugins.OfType<CaptionSelectionPlugin>().FirstOrDefault();
            captionSelectionPlugin.CaptionSelectionViewStyle = new Style(typeof(CaptionSelectionView));
            captionSelectionPlugin.CaptionSelectionViewStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(0, 0, 0, 90)));
        }

        void player_InteractiveViewModelChanged(object sender, InteractiveViewModelChangedEventArgs e)
        {
            UpdateViewModel(e.NewValue);
        }

        private void UpdateViewModel(IInteractiveViewModel vm)
        {
            ReplayButton.ViewModel = vm;
            CaptionSelectionButton.ViewModel = vm;
            AudioSelectionButton.ViewModel = vm;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            player.Initialized -= player_Initialized;
            player.ControlPanel.Loaded -= StartLayoutUpdates;
            player.ControlPanel.Unloaded -= StopLayoutUpdates;
            player.Dispose();
            base.OnNavigatedFrom(e);
        }
    }
}
