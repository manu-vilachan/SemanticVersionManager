namespace SemanticVersionManager
{
    public class VersionNumbersIncrement
    {
        public IncrementMethod Major { get; set; }

        public IncrementMethod Minor { get; set; }

        public IncrementMethod Patch { get; set; }

        public IncrementMethod Build { get; set; }

        public IncrementMethod Revision { get; set; }
    }
}