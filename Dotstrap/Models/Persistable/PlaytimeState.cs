namespace Dotstrap.Models.Persistable
{
    public class PlaytimeEntry
    {
        public string Name { get; set; } = "";
        public long TotalSeconds { get; set; } = 0;
        public int SessionCount { get; set; } = 0;
        public DateTime LastPlayed { get; set; } = DateTime.MinValue;
    }

    public class PlaytimeState
    {
        public Dictionary<long, PlaytimeEntry> Games { get; set; } = new();
    }
}
