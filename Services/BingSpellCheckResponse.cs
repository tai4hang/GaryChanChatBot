namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    using Newtonsoft.Json;

    public class BingSpellCheckResponse
    {
        [JsonProperty("_type")]
        public string Type { get; set; }

        public BingSpellCheckFlaggedToken[] FlaggedTokens { get; set; }

        public BingSpellCheckError Error { get; set; }
    }
}