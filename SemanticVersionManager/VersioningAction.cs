namespace SemanticVersionManager
{
    public class VersioningAction
    {
        public VersionAction Action { get; set; }

    }

    public enum VersionAction
    {
        Version,
        Promote
    }
}