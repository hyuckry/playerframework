using System;
using System.Collections.Generic;
#if SILVERLIGHT
using System.Windows;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Represents a caption or subtitle track.
    /// </summary>
#if SILVERLIGHT
    public sealed class Caption : DependencyObject
#else
    public sealed class Caption : FrameworkElement, ICaption
#endif
    {
        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler PayloadChanged;
#else
        public event EventHandler<object> PayloadChanged;
#endif

        /// <summary>
        /// Invokes the PayloadChanged event
        /// </summary>
        void OnPayloadChanged()
        {
            if (PayloadChanged != null) PayloadChanged(this, EventArgs.Empty);
        }

        /// <inheritdoc /> 
        public event EventHandler<PayloadAugmentedEventArgs> PayloadAugmented;

        /// <summary>
        /// Invokes the PayloadChanged event
        /// </summary>
        public void AugmentPayload(object payload, TimeSpan startTime, TimeSpan endTime)
        {
            if (PayloadAugmented != null) PayloadAugmented(this, new PayloadAugmentedEventArgs(payload, startTime, endTime));
        }

        /// <summary>
        /// Id DependencyProperty definition.
        /// </summary>
        public static DependencyProperty IdProperty { get { return idProperty; } }
        static readonly DependencyProperty idProperty = DependencyProperty.Register("Id", typeof(string), typeof(Caption), null);

        /// <inheritdoc /> 
        public string Id
        {
            get { return GetValue(IdProperty) as string; }
            set { SetValue(IdProperty, value); }
        }

        /// <summary>
        /// Description DependencyProperty definition.
        /// </summary>
        public static DependencyProperty DescriptionProperty { get { return descriptionProperty; } }
        static readonly DependencyProperty descriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(Caption), null);

        /// <inheritdoc /> 
        public string Description
        {
            get { return GetValue(DescriptionProperty) as string; }
            set { SetValue(DescriptionProperty, value); }
        }

        /// <summary>
        /// Payload DependencyProperty definition.
        /// </summary>
        public static DependencyProperty PayloadProperty { get { return payloadProperty; } }
        static readonly DependencyProperty payloadProperty = DependencyProperty.Register("Payload", typeof(object), typeof(Caption), new PropertyMetadata(null, (d, o) => ((Caption)d).OnPayloadChanged()));

        /// <inheritdoc /> 
        public object Payload
        {
            get { return GetValue(PayloadProperty) as object; }
            set { SetValue(PayloadProperty, value); }
        }

        /// <summary>
        /// Gets or sets the source Uri for the timed text. Useful for Xaml binding
        /// </summary>
        public Uri Source
        {
            get { return Payload as Uri; }
            set { Payload = value; }
        }

        /// <inheritdoc /> 
        public override string ToString()
        {
            return Description;
        }
    }
}
