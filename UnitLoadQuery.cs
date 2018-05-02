namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public class UnitLoadQuery
    {
        [Prompt("Please enter your {&}")]
        [Optional]
        public string UldId { get; set; }

        [Prompt("Near which zone")]
        [Optional]
        public string Zone { get; set; }

        [Prompt("of which flight carrier code")]
        [Optional]
        public string AirportCode { get; set; }
    }
}