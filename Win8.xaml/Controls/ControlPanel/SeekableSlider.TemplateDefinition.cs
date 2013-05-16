using System;
using System.Collections;
using System.Collections.Generic;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.ComponentModel;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#endif

namespace Microsoft.PlayerFramework
{
    [TemplatePart(Name = SeekableSliderTemplateParts.HorizontalTemplate, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = SeekableSliderTemplateParts.HorizontalThumb, Type = typeof(Thumb))]
    [TemplatePart(Name = SeekableSliderTemplateParts.HorizontalAvailableBar, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = SeekableSliderTemplateParts.VerticalTemplate, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = SeekableSliderTemplateParts.VerticalThumb, Type = typeof(Thumb))]
    [TemplatePart(Name = SeekableSliderTemplateParts.VerticalAvailableBar, Type = typeof(FrameworkElement))]
    [TemplateVisualState(Name = TimelineVisualStates.ScrubbingStates.IsScrubbing, GroupName = TimelineVisualStates.GroupNames.ScrubbingStates)]
    [TemplateVisualState(Name = TimelineVisualStates.ScrubbingStates.IsNotScrubbing, GroupName = TimelineVisualStates.GroupNames.ScrubbingStates)]
    public partial class SeekableSlider
    {
        FrameworkElement availableBar;

        FrameworkElement panel;

        Thumb thumb;

        #region Template Children

        /// <inheritdoc /> 
#if SILVERLIGHT
        public override void OnApplyTemplate()
#else
        protected override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            UninitializeTemplateChildren();
            GetTemplateChildren();
            InitializeTemplateChildren();
        }

        private void GetTemplateChildren()
        {
            if (Orientation == Orientation.Horizontal)
            {
                panel = GetTemplateChild(SeekableSliderTemplateParts.HorizontalTemplate) as FrameworkElement;
                thumb = GetTemplateChild(SeekableSliderTemplateParts.HorizontalThumb) as Thumb;
                availableBar = GetTemplateChild(SeekableSliderTemplateParts.HorizontalAvailableBar) as FrameworkElement;
            }
            else
            {
                panel = GetTemplateChild(SeekableSliderTemplateParts.VerticalTemplate) as FrameworkElement;
                thumb = GetTemplateChild(SeekableSliderTemplateParts.VerticalThumb) as Thumb;
                availableBar = GetTemplateChild(SeekableSliderTemplateParts.VerticalAvailableBar) as FrameworkElement;
            }
        }

        private void InitializeTemplateChildren()
        {
            if (availableBar != null)
            {
#if SILVERLIGHT
                availableBar.MouseLeftButtonDown += bar_PointerPressed;
                availableBar.MouseLeftButtonUp += bar_PointerReleased;
                availableBar.MouseMove += bar_PointerMoved;
#else
                availableBar.PointerPressed += bar_PointerPressed;
                availableBar.PointerReleased += bar_PointerReleased;
                availableBar.PointerMoved += bar_PointerMoved;
#endif
            }
            if (panel != null)
            {
                panel.SizeChanged += PanelSizeChanged;
            }

            if (thumb != null)
            {
                thumb.DragStarted += ThumbDragStarted;
                thumb.DragDelta += ThumbDragDelta;
                thumb.DragCompleted += ThumbDragCompleted;
            }
        }

        private void UninitializeTemplateChildren()
        {
            // main container
            if (availableBar != null)
            {
#if SILVERLIGHT
                availableBar.MouseLeftButtonDown -= bar_PointerPressed;
                availableBar.MouseLeftButtonUp -= bar_PointerReleased;
                availableBar.MouseMove -= bar_PointerMoved;
#else
                availableBar.PointerPressed -= bar_PointerPressed;
                availableBar.PointerReleased -= bar_PointerReleased;
                availableBar.PointerMoved -= bar_PointerMoved;
#endif
            }
            if (panel != null)
            {
                panel.SizeChanged -= PanelSizeChanged;
            }

            // thumb
            if (thumb != null)
            {
                thumb.DragStarted -= ThumbDragStarted;
                thumb.DragDelta -= ThumbDragDelta;
                thumb.DragCompleted -= ThumbDragCompleted;
            }
        }

        #endregion

        #region Dependency Properties

        #region ActualValue
        /// <summary>
        /// ActualValue DependencyProperty definition.
        /// </summary>
        public static DependencyProperty ActualValueProperty { get { return actualValueProperty; } }
        static readonly DependencyProperty actualValueProperty = DependencyProperty.Register("ActualValue", typeof(double), typeof(SeekableSlider), new PropertyMetadata(0.0, (d, e) => ((SeekableSlider)d).OnActualValueChanged((double)e.NewValue)));

        void OnActualValueChanged(double newValue)
        {
            if (!IsScrubbing && !inboundValue)
            {
                inboundValue = true;
                try
                {
                    Value = newValue;
                }
                finally
                {
                    inboundValue = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the actual value of the slider to be able to maintain the value of the slider while the user is scrubbing.
        /// </summary>
        public double ActualValue
        {
            get { return (double)GetValue(ActualValueProperty); }
            set { SetValue(ActualValueProperty, value); }
        }

        #endregion

        #region MaxValue
        /// <summary>
        /// MaxValue DependencyProperty definition.
        /// </summary>
        public static DependencyProperty MaxValueProperty { get { return maxValueProperty; } }
        static readonly DependencyProperty maxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(SeekableSlider), new PropertyMetadata(double.NaN, (d, e) => ((SeekableSlider)d).OnMaxValueChanged()));

        void OnMaxValueChanged()
        {
            RestrictAvailability();
        }

        /// <summary>
        /// Gets or sets the max position of the timeline.
        /// </summary>
        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        double Max
        {
            get
            {
                return double.IsNaN(MaxValue) ? Maximum : MaxValue;
            }
        }

        #endregion

        #region SliderThumbStyle
        /// <summary>
        /// SliderThumbStyle DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderThumbStyleProperty { get { return sliderThumbStyleProperty; } }
        static readonly DependencyProperty sliderThumbStyleProperty = DependencyProperty.Register("SliderThumbStyle", typeof(Style), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the style for the slider thumb
        /// </summary>
        public Style SliderThumbStyle
        {
            get { return GetValue(SliderThumbStyleProperty) as Style; }
            set { SetValue(SliderThumbStyleProperty, value); }
        }

        #endregion

        #region HorizontalBackgroundContent
        /// <summary>
        /// HorizontalBackgroundContent DependencyProperty definition.
        /// </summary>
        public static DependencyProperty HorizontalBackgroundContentProperty { get { return horizontalBackgroundContentProperty; } }
        static readonly DependencyProperty horizontalBackgroundContentProperty = DependencyProperty.Register("HorizontalBackgroundContent", typeof(UIElement), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the UIElement to display behind the control
        /// </summary>
        public UIElement HorizontalBackgroundContent
        {
            get { return GetValue(HorizontalBackgroundContentProperty) as UIElement; }
            set { SetValue(HorizontalBackgroundContentProperty, value); }
        }

        #endregion

        #region HorizontalForegroundContent
        /// <summary>
        /// HorizontalForegroundContent DependencyProperty definition.
        /// </summary>
        public static DependencyProperty HorizontalForegroundContentProperty { get { return horizontalForegroundContentProperty; } }
        static readonly DependencyProperty horizontalForegroundContentProperty = DependencyProperty.Register("HorizontalForegroundContent", typeof(UIElement), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the UIElement to display in the foreground
        /// </summary>
        public UIElement HorizontalForegroundContent
        {
            get { return GetValue(HorizontalForegroundContentProperty) as UIElement; }
            set { SetValue(HorizontalForegroundContentProperty, value); }
        }

        #endregion

        #region VerticalBackgroundContent
        /// <summary>
        /// VerticalBackgroundContent DependencyProperty definition.
        /// </summary>
        public static DependencyProperty VerticalBackgroundContentProperty { get { return verticalBackgroundContentProperty; } }
        static readonly DependencyProperty verticalBackgroundContentProperty = DependencyProperty.Register("VerticalBackgroundContent", typeof(UIElement), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the UIElement to display behind the control
        /// </summary>
        public UIElement VerticalBackgroundContent
        {
            get { return GetValue(VerticalBackgroundContentProperty) as UIElement; }
            set { SetValue(VerticalBackgroundContentProperty, value); }
        }

        #endregion

        #region VerticalForegroundContent
        /// <summary>
        /// VerticalForegroundContent DependencyProperty definition.
        /// </summary>
        public static DependencyProperty VerticalForegroundContentProperty { get { return verticalForegroundContentProperty; } }
        static readonly DependencyProperty verticalForegroundContentProperty = DependencyProperty.Register("VerticalForegroundContent", typeof(UIElement), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the UIElement to display in the foreground
        /// </summary>
        public UIElement VerticalForegroundContent
        {
            get { return GetValue(VerticalForegroundContentProperty) as UIElement; }
            set { SetValue(VerticalForegroundContentProperty, value); }
        }

        #endregion

        #region SliderTrackDecreasePressedBackground
        /// <summary>
        /// SliderTrackDecreasePressedBackground DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderTrackDecreasePressedBackgroundProperty { get { return sliderTrackDecreasePressedBackgroundProperty; } }
        static readonly DependencyProperty sliderTrackDecreasePressedBackgroundProperty = DependencyProperty.Register("SliderTrackDecreasePressedBackground", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderTrackDecreasePressedBackground
        {
            get { return GetValue(SliderTrackDecreasePressedBackgroundProperty) as Brush; }
            set { SetValue(SliderTrackDecreasePressedBackgroundProperty, value); }
        }

        #endregion

        #region SliderTrackPressedBackground
        /// <summary>
        /// SliderTrackPressedBackground DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderTrackPressedBackgroundProperty { get { return sliderTrackPressedBackgroundProperty; } }
        static readonly DependencyProperty sliderTrackPressedBackgroundProperty = DependencyProperty.Register("SliderTrackPressedBackground", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderTrackPressedBackground
        {
            get { return GetValue(SliderTrackPressedBackgroundProperty) as Brush; }
            set { SetValue(SliderTrackPressedBackgroundProperty, value); }
        }

        #endregion

        #region SliderThumbPressedBackground
        /// <summary>
        /// SliderThumbPressedBackground DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderThumbPressedBackgroundProperty { get { return sliderThumbPressedBackgroundProperty; } }
        static readonly DependencyProperty sliderThumbPressedBackgroundProperty = DependencyProperty.Register("SliderThumbPressedBackground", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderThumbPressedBackground
        {
            get { return GetValue(SliderThumbPressedBackgroundProperty) as Brush; }
            set { SetValue(SliderThumbPressedBackgroundProperty, value); }
        }

        #endregion

        #region SliderThumbPressedBorder
        /// <summary>
        /// SliderThumbPressedBorder DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderThumbPressedBorderProperty { get { return sliderThumbPressedBorderProperty; } }
        static readonly DependencyProperty sliderThumbPressedBorderProperty = DependencyProperty.Register("SliderThumbPressedBorder", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderThumbPressedBorder
        {
            get { return GetValue(SliderThumbPressedBorderProperty) as Brush; }
            set { SetValue(SliderThumbPressedBorderProperty, value); }
        }

        #endregion

        #region SliderDisabledBorder
        /// <summary>
        /// SliderDisabledBorder DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderDisabledBorderProperty { get { return sliderDisabledBorderProperty; } }
        static readonly DependencyProperty sliderDisabledBorderProperty = DependencyProperty.Register("SliderDisabledBorder", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderDisabledBorder
        {
            get { return GetValue(SliderDisabledBorderProperty) as Brush; }
            set { SetValue(SliderDisabledBorderProperty, value); }
        }

        #endregion

        #region SliderTrackDecreaseDisabledBackground
        /// <summary>
        /// SliderTrackDecreaseDisabledBackground DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderTrackDecreaseDisabledBackgroundProperty { get { return sliderTrackDecreaseDisabledBackgroundProperty; } }
        static readonly DependencyProperty sliderTrackDecreaseDisabledBackgroundProperty = DependencyProperty.Register("SliderTrackDecreaseDisabledBackground", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderTrackDecreaseDisabledBackground
        {
            get { return GetValue(SliderTrackDecreaseDisabledBackgroundProperty) as Brush; }
            set { SetValue(SliderTrackDecreaseDisabledBackgroundProperty, value); }
        }

        #endregion

        #region SliderTrackDisabledBackground
        /// <summary>
        /// SliderTrackDisabledBackground DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderTrackDisabledBackgroundProperty { get { return sliderTrackDisabledBackgroundProperty; } }
        static readonly DependencyProperty sliderTrackDisabledBackgroundProperty = DependencyProperty.Register("SliderTrackDisabledBackground", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderTrackDisabledBackground
        {
            get { return GetValue(SliderTrackDisabledBackgroundProperty) as Brush; }
            set { SetValue(SliderTrackDisabledBackgroundProperty, value); }
        }

        #endregion

        #region SliderThumbDisabledBackground
        /// <summary>
        /// SliderThumbDisabledBackground DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderThumbDisabledBackgroundProperty { get { return sliderThumbDisabledBackgroundProperty; } }
        static readonly DependencyProperty sliderThumbDisabledBackgroundProperty = DependencyProperty.Register("SliderThumbDisabledBackground", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderThumbDisabledBackground
        {
            get { return GetValue(SliderThumbDisabledBackgroundProperty) as Brush; }
            set { SetValue(SliderThumbDisabledBackgroundProperty, value); }
        }

        #endregion

        #region SliderTrackDecreasePointerOverBackground
        /// <summary>
        /// SliderTrackDecreasePointerOverBackground DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderTrackDecreasePointerOverBackgroundProperty { get { return sliderTrackDecreasePointerOverBackgroundProperty; } }
        static readonly DependencyProperty sliderTrackDecreasePointerOverBackgroundProperty = DependencyProperty.Register("SliderTrackDecreasePointerOverBackground", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderTrackDecreasePointerOverBackground
        {
            get { return GetValue(SliderTrackDecreasePointerOverBackgroundProperty) as Brush; }
            set { SetValue(SliderTrackDecreasePointerOverBackgroundProperty, value); }
        }

        #endregion

        #region SliderTrackPointerOverBackground
        /// <summary>
        /// SliderTrackPointerOverBackground DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderTrackPointerOverBackgroundProperty { get { return sliderTrackPointerOverBackgroundProperty; } }
        static readonly DependencyProperty sliderTrackPointerOverBackgroundProperty = DependencyProperty.Register("SliderTrackPointerOverBackground", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderTrackPointerOverBackground
        {
            get { return GetValue(SliderTrackPointerOverBackgroundProperty) as Brush; }
            set { SetValue(SliderTrackPointerOverBackgroundProperty, value); }
        }

        #endregion

        #region SliderThumbPointerOverBackground
        /// <summary>
        /// SliderThumbPointerOverBackground DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderThumbPointerOverBackgroundProperty { get { return sliderThumbPointerOverBackgroundProperty; } }
        static readonly DependencyProperty sliderThumbPointerOverBackgroundProperty = DependencyProperty.Register("SliderThumbPointerOverBackground", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderThumbPointerOverBackground
        {
            get { return GetValue(SliderThumbPointerOverBackgroundProperty) as Brush; }
            set { SetValue(SliderThumbPointerOverBackgroundProperty, value); }
        }

        #endregion

        #region SliderThumbPointerOverBorder
        /// <summary>
        /// SliderThumbPointerOverBorder DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderThumbPointerOverBorderProperty { get { return sliderThumbPointerOverBorderProperty; } }
        static readonly DependencyProperty sliderThumbPointerOverBorderProperty = DependencyProperty.Register("SliderThumbPointerOverBorder", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderThumbPointerOverBorder
        {
            get { return GetValue(SliderThumbPointerOverBorderProperty) as Brush; }
            set { SetValue(SliderThumbPointerOverBorderProperty, value); }
        }

        #endregion

        #region SliderThumbBackground
        /// <summary>
        /// SliderThumbPointerOverBorder DependencyProperty definition.
        /// </summary>
        public static DependencyProperty SliderThumbBackgroundProperty { get { return sliderThumbBackgroundProperty; } }
        static readonly DependencyProperty sliderThumbBackgroundProperty = DependencyProperty.Register("SliderThumbBackground", typeof(Brush), typeof(SeekableSlider), null);

        /// <summary>
        /// Gets or sets the Brush to display in the foreground
        /// </summary>
        public Brush SliderThumbBackground
        {
            get { return GetValue(SliderThumbBackgroundProperty) as Brush; }
            set { SetValue(SliderThumbBackgroundProperty, value); }
        }

        #endregion

        #endregion

        #region Event Handlers

        private void PanelSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // bar size changed, update available bar
            RestrictAvailability();
        }

        private void ThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            UpdateScrubbingVisualState();
            OnScrubbingStarted(new SeekableSliderManipulatedEventArgs(Value));
        }

        private void ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (thumb.IsDragging)
            {
                if (Value > Max) Value = Max;
            }
        }

        private void ThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (!e.Canceled)
            {
                if (Value > Max) Value = Max;
                UpdateScrubbingVisualState();
                OnScrubbingCompleted(new SeekableSliderManipulatedEventArgs(Value));
            }
        }

#if SILVERLIGHT
        private void bar_PointerPressed(object sender, MouseButtonEventArgs e)
#else
        private void bar_PointerPressed(object sender, PointerRoutedEventArgs e)
#endif
        {
#if SILVERLIGHT
            pointerCaptured = ((FrameworkElement)sender).CaptureMouse();
#else
            pointerCaptured = ((FrameworkElement)sender).CapturePointer(e.Pointer);
#endif
            UpdateScrubbingVisualState();
            if (pointerCaptured)
            {
#if SILVERLIGHT
                pointerReleaseAction = () => ((FrameworkElement)sender).ReleaseMouseCapture();
#else
                pointerReleaseAction = () => ((FrameworkElement)sender).ReleasePointerCapture(e.Pointer);
#endif
                double? newValue = null;
                if (Orientation == Orientation.Horizontal) newValue = GetHorizontalPanelMousePosition(e);
                else newValue = GetVerticalPanelMousePosition(e);

                if (newValue.HasValue)
                {
                    var value = Math.Min(newValue.Value, Max);
                    var args = new SeekableSliderManipulatedEventArgs(value);
                    OnScrubbingStarted(args);
                    if (!args.Canceled)
                    {
                        Value = value;
                    }
                }
            }
            e.Handled = true;
        }

#if SILVERLIGHT
        private void bar_PointerReleased(object sender, MouseButtonEventArgs e)
#else
        private void bar_PointerReleased(object sender, PointerRoutedEventArgs e)
#endif
        {
            if (pointerCaptured)
            {
                double? newValue = null;
                if (Orientation == Orientation.Horizontal) newValue = GetHorizontalPanelMousePosition(e);
                else newValue = GetVerticalPanelMousePosition(e);

                if (newValue.HasValue)
                {
                    Value = Math.Min(newValue.Value, Max);
                }

                if (pointerCaptured)
                {
                    OnScrubbingCompleted(new SeekableSliderManipulatedEventArgs(Value));
                    pointerReleaseAction();
                    pointerReleaseAction = null;
                    pointerCaptured = false;
                    UpdateScrubbingVisualState();
                }
            }
            e.Handled = true;
        }

#if SILVERLIGHT
        private void bar_PointerMoved(object sender, MouseEventArgs e)
#else
        private void bar_PointerMoved(object sender, PointerRoutedEventArgs e)
#endif
        {
            if (pointerCaptured)
            {
                double? newValue = null;
                if (Orientation == Orientation.Horizontal) newValue = GetHorizontalPanelMousePosition(e);
                else newValue = GetVerticalPanelMousePosition(e);

                if (newValue.HasValue)
                {
                    Value = Math.Min(newValue.Value, Max);
                }
            }
#if NETFX_CORE
            e.Handled = true;
#endif
        }

        #endregion

        #region Misc
        private void UpdateScrubbingVisualState()
        {
            var state = IsScrubbing ? TimelineVisualStates.ScrubbingStates.IsScrubbing : TimelineVisualStates.ScrubbingStates.IsNotScrubbing;
            this.GoToVisualState(state);
        }

        /// <summary>
        /// Cancels the active scrub
        /// </summary>
        public void CancelScrub()
        {
            if (pointerCaptured)
            {
                pointerReleaseAction();
                pointerReleaseAction = null;
                pointerCaptured = false;
            }
            else if (thumb.IsDragging)
            {
                thumb.CancelDrag();
#if SILVERLIGHT
                thumb.ReleaseMouseCapture();
#else
                thumb.ReleasePointerCaptures();
#endif
            }
            UpdateScrubbingVisualState();
        }
        #endregion
    }
}
