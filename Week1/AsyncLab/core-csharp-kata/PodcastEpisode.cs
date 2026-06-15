namespace PlaylistKata;

public class PodcastEpisode : Track
{
    public string Showname {get; set;}
    public int EpisodeNumber{get; set;}

    public PodcastEpisode(string title, string artist, int duration, DateTime releaseDate, string showName, int episodeNumber)
        : base(title, artist, duration, releaseDate)
    {
        Showname = showName;
        EpisodeNumber = episodeNumber;
    }

    public override void Play()
    {
        Console.WriteLine($"Now playing: Episode {EpisodeNumber} from {Showname} ");
    }
}