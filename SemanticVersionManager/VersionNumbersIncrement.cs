namespace SemanticVersionManager
{
    public class VersionNumbersIncrement
    {
        public IncrementMethod Major { get; set; } = IncrementMethod.None;

        public IncrementMethod Minor { get; set; } = IncrementMethod.None;

        public IncrementMethod Patch { get; set; } = IncrementMethod.None;

        public IncrementMethod Build { get; set; } = IncrementMethod.None;

        public IncrementMethod Revision { get; set; } = IncrementMethod.None;
    }
}