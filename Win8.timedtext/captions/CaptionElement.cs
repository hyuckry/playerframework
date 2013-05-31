
namespace Microsoft.TimedText
{
    /// <summary>
    /// Represents a closed caption
    /// </summary>
    internal class CaptionElement : TimedTextElement
    {
        public CaptionElement()
        {
            CaptionElementType = TimedTextElementType.Text;
        }

        public int Index { get; set; }

    }
}