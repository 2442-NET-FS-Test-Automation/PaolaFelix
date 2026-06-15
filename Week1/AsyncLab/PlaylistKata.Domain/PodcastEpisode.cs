namespace PlaylistKata.Domain;

public class PodcastEpisode : Track
{
    public string Showname {get; set;}
    public string? EpisodeNumber{get; set;}

    public PodcastEpisode(string title, string artist, int duration, DateTime releaseDate, string showName, string episodeNumber)
        : base(title, artist, duration, releaseDate)
    {
        Showname = showName;
        EpisodeNumber = episodeNumber;
    }

    public override void Play()
    {
        if (EpisodeNumber != null)
        {
            Console.WriteLine($"Now playing: Episode {Title} | {EpisodeNumber} from {Showname} "); 
        }
        else{
            Console.WriteLine($"Now playing: Episode {Title} from {Showname} ");
        }
    }
}