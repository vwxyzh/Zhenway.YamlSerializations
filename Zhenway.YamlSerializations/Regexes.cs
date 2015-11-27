using System.Text.RegularExpressions;

namespace Zhenway.YamlSerializations
{
    internal static class Regexes
    {
        public static readonly Regex BooleanLike = new Regex(@"^(true|false)$", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        public static readonly Regex IntegerLike = new Regex(@"^-?(0|[1-9][0-9]*)$", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        public static readonly Regex DoubleLike = new Regex(@"^-?(0|[1-9][0-9]*)(\.[0-9]*)?([eE][-+]?[0-9]+)?$", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
    }
}
