using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Automation;
#else
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// A control that allows the user to change the volume.
    /// </summary>
    public sealed class VolumeSlider : Control
    {
        //<local:SeekableSlider x:Name="HorizontalVolumeSlider" local:RangeBaseBehavior.Command="{Binding VolumeCommand, Source={StaticResource Commands}}" ActualValue="{Binding Volume}" Maximum="1" HorizontalAlignment="Center" Width="83" Visibility="Collapsed">
        //    <ToolTipService.ToolTip>
        //        <ToolTip x:Uid="/Microsoft.PlayerFramework/Resources/VolumeTooltip"/>
        //    </ToolTipService.ToolTip>
        //</local:SeekableSlider>

        SeekableSlider slider;

        /// <summary>
        /// Creates a new instance of the VolumeSlider control.
        /// </summary>
        public VolumeSlider()
        {
            DefaultStyleKey = typeof(VolumeSlider);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("VolumeLabel"));
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public override void OnApplyTemplate()
#else
        protected override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();

            if (slider != null)
            {
                slider.ValueChanged -= slider_ValueChanged;
                slider.SetBinding(SeekableSlider.ActualValueProperty, null);
            }

            slider = GetTemplateChild("Slider") as SeekableSlider;

            if (slider != null)
            {
                slider.ValueChanged += slider_ValueChanged;
                slider.SetBinding(SeekableSlider.ActualValueProperty, new Binding() { Path = new PropertyPath("Volume"), Source = ViewModel });
            }
        }

#if SILVERLIGHT
        void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
#else
        void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
#endif
        {
            if (ViewModel != null)
            {
                ViewModel.Volume = e.NewValue;
            }
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(VolumeSlider), new PropertyMetadata(null, (d, e) => ((VolumeSlider)d).OnViewModelChanged(e.OldValue as IInteractiveViewModel, e.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Provides notification that the view model has changed.
        /// </summary>
        /// <param name="oldValue">The old view model. Note: this could be null.</param>
        /// <param name="newValue">The new view model. Note: this could be null.</param>
        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            if (slider != null)
            {
                slider.SetBinding(SeekableSlider.ActualValueProperty, new Binding() { Path = new PropertyPath("Volume"), Source = ViewModel });
            }
        }

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }


        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static DependencyProperty OrientationProperty { get { return orientationProperty; } }
        static readonly DependencyProperty orientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(VolumeSlider), new PropertyMetadata(Orientation.Vertical));

        /// <summary>
        /// Gets or sets the InteractiveOrientation object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

#if NETFX_CORE
        /// <summary>
        /// Gets the focus state of the underlying Slider control
        /// </summary>
        public FocusState InnerFocusState
        {
            get { return slider != null ? slider.FocusState : FocusState; }
        }
#endif
    }
}
