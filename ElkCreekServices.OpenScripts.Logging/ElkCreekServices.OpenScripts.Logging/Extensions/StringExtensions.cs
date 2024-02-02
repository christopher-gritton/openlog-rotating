using System.Text.RegularExpressions;

namespace ElkCreekServices.OpenScripts.Logging;
public static class StringExtensions
{

    public static string CreateValidLogFileName(this string s, string replacement = "")
    {
        return Regex.Replace(s,
          "[" + Regex.Escape(new String(System.IO.Path.GetInvalidFileNameChars())) + "]",
          replacement, //can even use a replacement string of any length
          RegexOptions.IgnoreCase);
        //not using System.IO.Path.InvalidPathChars (deprecated insecure API)
    }
}
