namespace CustomSearchSpotlight.Models
{
    public class SearchResult
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public ResultType Type { get; set; }
        public object Icon { get; set; } // Can be an ImageSource or string path
    }

    public enum ResultType
    {
        File,
        Folder,
        Application,
        Web,
        AI
    }
}
