namespace ElectronicsStore.Models
{
    public class ImportError
    {
        public int LineNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}