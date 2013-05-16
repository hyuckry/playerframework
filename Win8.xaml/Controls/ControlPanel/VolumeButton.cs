using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
#else
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Windows.Input;
#endif

namespace Microsoft.PlayerFramework
{
    internal static class VolumeVisibilityStates
    {
        internal const string Requested = "VolumeRequested";
        internal const string Dismissed = "VolumeDismissed";
        internal const string Hidden = "VolumeHidden";
        internal const string Visible = "VolumeVisible";
    }

    internal static class VolumeGroupNames
    {
        internal const string VolumeVisibilityStates = "VolumeVisibilityStates";
    }

    internal static class VolumeTemplateParts
    {
        public const string VolumeSliderContainer = "VolumeSliderContainer";
        public const string VolumeSlider = "VolumeSlider";
        public const string MuteButton = "MuteButton";
    }

    /// <summary>
    /// Represents a button that will allow the user to both mute and change the volume.
    /// </summary>
    [TemplatePart(Name = VolumeTemplateParts.VolumeSliderContainer, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = VolumeTemplateParts.VolumeSlider, Type = typeof(VolumeSlider))]
    [TemplatePart(Name = VolumeTemplateParts.MuteButton, Type = typeof(Button))]
    [TemplateVisualState(Name = VolumeVisibilityStates.Requested, GroupName = VolumeGroupNames.VolumeVisibilityStates)]
    [TemplateVisualState(Name = VolumeVisibilityStates.Dismissed, GroupName = VolumeGroupNames.VolumeVisibilityStates)]
    [TemplateVisualState(Name = VolumeVisibilityStates.Hidden, GroupName = VolumeGroupNames.VolumeVisibilityStates)]
    [TemplateVisualState(Name = VolumeVisibilityStates.Visible, GroupName = VolumeGroupNames.VolumeVisibilityStates)]
    public sealed class VolumeButton : Control
    {
        FrameworkElement volumeSliderContainerElement;

        VolumeSlider volumeSliderElement;

        MuteButton muteButtonElement;

        /// <summary>
        /// Creates a new instance of VolumeButton.
        /// </summary>
        public VolumeButton()
        {
            DefaultStyleKey = typeof(VolumeButton);
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public override void OnApplyTemplate()
#else
        protected override void OnApplyTemplate()
#endif
        {
            UninitializeTemplateChildren();
            base.OnApplyTemplate();
            GetTemplateChildren();
            InitializeTemplateChildren();
            SetDefaultVisualStates();
        }

        /// <inheritdoc /> 
        void GetTemplateChildren()
        {
            volumeSliderContainerElement = GetTemplateChild(VolumeTemplateParts.VolumeSliderContainer) as FrameworkElement;
            volumeSliderElement = GetTemplateChild(VolumeTemplateParts.VolumeSlider) as VolumeSlider;
            muteButtonElement = GetTemplateChild(VolumeTemplateParts.MuteButton) as MuteButton;
        }

        void SetDefaultVisualStates()
        {
            UpdateVolumeVisualState();
            UpdateVolumeLabel();
        }

        private void UpdateVolumeVisualState()
        {
            if (IsVolumeVisible)
            {
                this.GoToVisualState(VolumeVisibilityStates.Visible);
            }
            else
            {
                this.GoToVisualState(VolumeVisibilityStates.Hidden);
            }
        }

        private void UpdateVolumeLabel()
        {
            if (muteButtonElement != null)
            {
                if (IsVolumeVisible)
                {
                    muteButtonElement.UnmutedName = MediaPlayer.GetResourceString("MuteButtonLabel");
                }
                else
                {
                    muteButtonElement.UnmutedName = MediaPlayer.GetResourceString("VolumeMuteButtonLabel");
                }
            }
        }

        void InitializeTemplateChildren()
        {
            if (muteButtonElement != null)
            {
                muteButtonElement.GotFocus += VolumeButtonElement_GotFocus;
                var vmCommand = muteButtonElement.Command as ViewModelCommand;
                if (vmCommand != null) vmCommand.Executing += vmCommand_Executing;
            }

            if (volumeSliderContainerElement != null)
            {
#if SILVERLIGHT
                volumeSliderContainerElement.MouseEnter += VolumeSliderContainerElement_MouseEnter;
                volumeSliderContainerElement.MouseLeave += VolumeSliderContainerElement_MouseLeave;
#else
                volumeSliderContainerElement.PointerEntered += VolumeSliderContainerElement_PointerEntered;
                volumeSliderContainerElement.PointerExited += VolumeSliderContainerElement_PointerExited;
#endif
            }

            volumeCollapseTimer.Tick += volumeCollapseTimer_Tick;
        }

        void UninitializeTemplateChildren()
        {
            if (muteButtonElement != null)
            {
                muteButtonElement.GotFocus -= VolumeButtonElement_GotFocus;
                var vmCommand = muteButtonElement.Command as ViewModelCommand;
                if (vmCommand != null) vmCommand.Executing -= vmCommand_Executing;
            }

            if (volumeSliderContainerElement != null)
            {
#if SILVERLIGHT
                volumeSliderContainerElement.MouseEnter -= VolumeSliderContainerElement_MouseEnter;
                volumeSliderContainerElement.MouseLeave -= VolumeSliderContainerElement_MouseLeave;
#else
                volumeSliderContainerElement.PointerEntered -= VolumeSliderContainerElement_PointerEntered;
                volumeSliderContainerElement.PointerExited -= VolumeSliderContainerElement_PointerExited;
#endif
            }

            volumeCollapseTimer.Tick -= volumeCollapseTimer_Tick;
        }

#if SILVERLIGHT
        void VolumeSliderContainerElement_MouseLeave(object sender, MouseEventArgs e)
#else
        void VolumeSliderContainerElement_PointerExited(object sender, PointerRoutedEventArgs e)
#endif
        {
            if (!volumeCollapseTimer.IsEnabled) volumeCollapseTimer.Start();
        }

#if SILVERLIGHT
        void VolumeSliderContainerElement_MouseEnter(object sender, MouseEventArgs e)
#else
        void VolumeSliderContainerElement_PointerEntered(object sender, PointerRoutedEventArgs e)
#endif
        {
            if (volumeCollapseTimer.IsEnabled) volumeCollapseTimer.Stop();
        }

        void vmCommand_Executing(object sender, ExecutingEventArgs e)
        {
            if (!IsVolumeVisible)
            {
                e.Cancel = e.Cancel || !ViewModel.IsMuted;
                this.GoToVisualState(VolumeVisibilityStates.Requested);
                IsVolumeVisible = true;
            }
            else
            {
                this.GoToVisualState(VolumeVisibilityStates.Dismissed);
                IsVolumeVisible = false;
            }
        }

        readonly DispatcherTimer volumeCollapseTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(3) };

        bool isVolumeVisible;
        /// <summary>
        /// Gets or sets whether the volume panel is visible.
        /// </summary>
        public bool IsVolumeVisible
        {
            get { return isVolumeVisible; }
            set
            {
                if (isVolumeVisible != value)
                {
                    isVolumeVisible = value;
                    if (isVolumeVisible)
                    {
                        if (!volumeCollapseTimer.IsEnabled) volumeCollapseTimer.Start();
                    }
                    else
                    {
                        if (volumeCollapseTimer.IsEnabled) volumeCollapseTimer.Stop();
                    }
                    UpdateVolumeLabel();
                }
            }
        }

#if SILVERLIGHT
        void volumeCollapseTimer_Tick(object sender, EventArgs e)
        {
            DismissVolume();
        }
#else
        void volumeCollapseTimer_Tick(object sender, object e)
        {
            if (volumeSliderElement == null || volumeSliderElement.InnerFocusState != FocusState.Keyboard)
            {
                DismissVolume();
            }
        }
#endif

        /// <summary>
        /// Forces the volume slider popout to hide.
        /// </summary>
        public void DismissVolume()
        {
            if (IsVolumeVisible)
            {
                IsVolumeVisible = false;
                this.GoToVisualState(VolumeVisibilityStates.Dismissed);
            }
        }

        void VolumeButtonElement_GotFocus(object sender, RoutedEventArgs e)
        {
#if !SILVERLIGHT
            if (muteButtonElement.FocusState == FocusState.Keyboard)
#endif
            {
                RequestVolume();
            }
        }

        /// <summary>
        /// Forces the volume slider popout to show.
        /// </summary>
        public void RequestVolume()
        {
            if (!IsVolumeVisible)
            {
                this.GoToVisualState(VolumeVisibilityStates.Requested);
                IsVolumeVisible = true;
            }
        }
        
        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(VolumeButton), new PropertyMetadata(null, (s, d) => ((VolumeButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            // nothing to do
        }

        /// <summary>
        /// Identifies the MuteButtonStyle dependency property.
        /// </summary>
        public static DependencyProperty MuteButtonStyleProperty { get { return muteButtonStyleProperty; } }
        static readonly DependencyProperty muteButtonStyleProperty = DependencyProperty.Register("MuteButtonStyle", typeof(Style), typeof(VolumeButton), null);

        /// <summary>
        /// Gets or sets the Style used to display the mute button.
        /// </summary>
        public Style MuteButtonStyle
        {
            get { return GetValue(MuteButtonStyleProperty) as Style; }
            set { SetValue(MuteButtonStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the PanelBackground dependency property.
        /// </summary>
        public static DependencyProperty PanelBackgroundProperty { get { return panelBackgroundProperty; } }
        static readonly DependencyProperty panelBackgroundProperty = DependencyProperty.Register("PanelBackground", typeof(Brush), typeof(VolumeButton), null);

        /// <summary>
        /// Gets or sets the Background brush on the volume panel.
        /// </summary>
        public Brush PanelBackground
        {
            get { return GetValue(PanelBackgroundProperty) as Brush; }
            set { SetValue(PanelBackgroundProperty, value); }
        }

        /// <summary>
        /// Identifies the PanelPosition dependency property.
        /// </summary>
        public static DependencyProperty PanelPositionProperty { get { return panelPositionProperty; } }
        static readonly DependencyProperty panelPositionProperty = DependencyProperty.Register("PanelPosition", typeof(Thickness), typeof(VolumeButton), null);

        /// <summary>
        /// Gets or sets the Background position on the volume panel.
        /// </summary>
        public Thickness PanelPosition
        {
            get { return (Thickness)GetValue(PanelPositionProperty); }
            set { SetValue(PanelPositionProperty, value); }
        }
    }
}
