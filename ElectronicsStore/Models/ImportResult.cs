namespace ElectronicsStore.Models
{
    public class ImportResult
    {
        public int TotalRows { get; set; }
        public int ImportedCount { get; set; }
        public List<ImportError> Errors { get; set; } = new List<ImportError>();
    }
}