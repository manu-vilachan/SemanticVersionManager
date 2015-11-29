namespace SemanticVersionManager
{
    public static class Parameters
    {
        public const string GenerateVC = "GenerateVC";

        public const string VCPath = "VCPath";

        public const string Definition = "Definition";

        public const string BuildName = "BuildName";

        public const string Action = "Action";

        public const string Major = "Major";

        public const string Minor = "Minor";

        public const string Patch = "Patch";

        public const string Build = "Build";

        public const string Revision = "Revision";

        public const string DestinationDefinition = "DestinationDefinition";

        public const string DestinationBuild = "DestinationBuild";

        public static string TL(this string s)
        {
            return s.ToLower();
        }
    }

    public static class Commands
    {
        public const string DoVersioning = "DoVersioning";

        public const string GenerateVC = "GenerateVC";

        public const string NotRecognized = "NotRecognized";
    }
}