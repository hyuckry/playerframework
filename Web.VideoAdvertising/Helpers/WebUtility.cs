using System.Windows.Browser;

namespace System.Net
{
    /// <summary>
    /// Used for compatibility with Win8
    /// </summary>
    internal class WebUtility
    {
        public static string UrlEncode(string source)
        {
            return HttpUtility.UrlEncode(source);
        }
    }
}
