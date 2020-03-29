using System;
using System.Collections.Generic;
using System.Text;

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// Custom conversion methods used by the library.
    /// </summary>
    public static class OwinConvert
    {
        /// <summary>
        /// As per System.Uri.UnescapeDataString except that it will decode + correctly.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string UrlDecode(string url)
        {
            return url == null
                ? null
                : Uri.UnescapeDataString(url.Replace("+", "%20"));
        }
    }
}
