namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public class CompUtilQuery
    {
        [Prompt("which zone or ALL")]
        [Optional]
        public string Zone { get; set; }
    }
}