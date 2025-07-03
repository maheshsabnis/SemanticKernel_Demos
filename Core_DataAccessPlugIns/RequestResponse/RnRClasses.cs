namespace Core_DataAccessPlugIns.RequestResponse
{
    public class PromptRequest
    {
        public string? Prompt { get; set; }
    }

    public class PromptResponse
    {
        public string? Prompt { get; set; }
        public string? Response { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
