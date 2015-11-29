using System.Collections.Generic;

namespace SemanticVersionManager
{
    public class PatternConstants
    {
        public const string Major = "MAJOR";

        public const string Minor = "MINOR";

        public const string Patch = "PATCH";

        public const string Build = "BUILD";

        public const string Revision = "REVISION";

        public const string Suffix = "PRSUFFIX";

        public static Dictionary<string, string> GetDictionary()
        {
            return new Dictionary<string, string>
            {
                {"MAJOR", string.Empty},
                {"MINOR", string.Empty},
                {"PATCH", string.Empty},
                {"BUILD", string.Empty},
                {"REVISION", string.Empty},
                {"PRSUFFIX", string.Empty}
            };
        }
    }
}