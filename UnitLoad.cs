namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    using System;

    [Serializable]
    public class UnitLoad
    {
        public string UldId { get; set; }

        public string Contour { get; set; }

        public string LoadingStatus { get; set; }

        public int Weight { get; set; }

        public string Flight { get; set; }

        public string Image { get; set; }

        public string Location { get; set; }
    }
}