using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class Compatibility
    {
        public static string[] Split(this string s, char[] separator, int count)
        {
            List<string> result = new List<string>();
            var sb = new StringBuilder();
            int total = 0;
            foreach (char c in s)
            {
                if (total < count && separator.Contains(c))
                {
                    result.Add(sb.ToString());
                    total++;
                    sb = new StringBuilder();
                }
                else
                {
                    sb.Append(c);
                }
            }
            return result.ToArray();
        }
    }
}
