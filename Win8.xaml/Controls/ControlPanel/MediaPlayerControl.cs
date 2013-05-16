using System;
using System.ComponentModel;
#if SILVERLIGHT
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Automation;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Automation;
using Windows.Foundation.Metadata;
#endif

namespace Microsoft.PlayerFramework
{
    public static class MediaPlayerControl
    {
        /// <summary>
        /// Identifies the Size dependency property.
        /// </summary>
        public static DependencyProperty SizeProperty { get { return sizeProperty; } }
        static readonly DependencyProperty sizeProperty = DependencyProperty.RegisterAttached("Size", typeof(double), typeof(MediaPlayerControl), new PropertyMetadata(26.0));

        /// <summary>
        /// Gets the diameter of the button.
        /// </summary>
        /// <param name="obj">An instance of a ButtonBase object.</param>
        /// <returns>The size for the corresponding button</returns>
        public static double GetSize(DependencyObject obj)
        {
            return (double)obj.GetValue(SizeProperty);
        }

        /// <summary>
        /// Sets the diameter of the button.
        /// </summary>
        /// <param name="obj">An instance of a ButtonBase object.</param>
        /// <param name="value">A value supplying the size for the associated button.</param>
        public static void SetSize(DependencyObject obj, double value)
        {
            obj.SetValue(SizeProperty, value);
        }

        /// <summary>
        /// Identifies the StrokeThickness dependency property.
        /// </summary>
        public static DependencyProperty StrokeThicknessProperty { get { return strokeThicknessProperty; } }
        static readonly DependencyProperty strokeThicknessProperty = DependencyProperty.RegisterAttached("StrokeThickness", typeof(double), typeof(MediaPlayerControl), new PropertyMetadata(2.0));

        /// <summary>
        /// Gets the thickness of the button border.
        /// </summary>
        /// <param name="obj">An instance of a ButtonBase object.</param>
        /// <returns>The StrokeThickness for the corresponding button</returns>
        public static double GetStrokeThickness(DependencyObject obj)
        {
            return (double)obj.GetValue(StrokeThicknessProperty);
        }

        /// <summary>
        /// Sets the thickness of the button border.
        /// </summary>
        /// <param name="obj">An instance of a ButtonBase object.</param>
        /// <param name="value">A value supplying the StrokeThickness for the associated button.</param>
        public static void SetStrokeThickness(DependencyObject obj, double value)
        {
            obj.SetValue(StrokeThicknessProperty, value);
        }

        /// <summary>
        /// Identifies the ContentTransform dependency property.
        /// </summary>
        public static DependencyProperty ContentTransformProperty { get { return contentTransformProperty; } }
        static readonly DependencyProperty contentTransformProperty = DependencyProperty.RegisterAttached("ContentTransform", typeof(Transform), typeof(MediaPlayerControl), null);

        /// <summary>
        /// Gets the Transform to apply to the inner content of a button.
        /// </summary>
        /// <param name="obj">An instance of a ButtonBase object.</param>
        /// <returns>The Transform object for the corresponding button</returns>
        public static Transform GetContentTransform(DependencyObject obj)
        {
            return obj.GetValue(ContentTransformProperty) as Transform;
        }

        /// <summary>
        /// Sets the Transform to apply to the inner content of a button.
        /// </summary>
        /// <param name="obj">An instance of a ButtonBase object.</param>
        /// <param name="value">A value supplying the Transform for the associated button.</param>
        public static void SetContentTransform(DependencyObject obj, Transform value)
        {
            obj.SetValue(ContentTransformProperty, value);
        }
    }
}
