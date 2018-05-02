namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    using System;

    [Serializable]
    public class Location
    {
        public string Id { get; set; }

        public string Floor { get; set; }

        public string Zone { get; set; }

    }
}