using System.Diagnostics;
using System.Web;

namespace quebrix.Helpers;

public static class UriEncoder
{
    public static string EncodeUrl(this string text) => HttpUtility.UrlEncode(text);

    public static string DecodeUri(this string text) => HttpUtility.UrlDecode(text);
}

