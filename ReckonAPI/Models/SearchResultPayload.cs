namespace ReckonAPI.Models
{
    public class SearchResultPayload
    {
        public string Candidate { get; set; }
        public string Text { get; set; }
        public List<SearchResult> Results { get; set; }
    }
}