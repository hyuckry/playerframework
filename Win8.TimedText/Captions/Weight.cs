using System;
#if SILVERLIGHT
using System.Windows;
#else
using Windows.UI.Text;
#endif

namespace Microsoft.TimedText
{
    internal static class FontWeightConverter
    {
        public static FontWeight Convert(Weight weight)
        {
            switch (weight)
            {
                case Weight.Bold: return FontWeights.Bold;
                case Weight.Normal: return FontWeights.Normal;
                default: throw new NotImplementedException();
            }
        }
    }

    internal enum Weight
    {
        Bold,
        Normal
    }
}
