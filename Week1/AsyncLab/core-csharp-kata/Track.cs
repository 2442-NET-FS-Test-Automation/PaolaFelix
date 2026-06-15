namespace PlaylistKata;

public abstract class Track
{
    public string Title {get; set;}
    public string Artist {get; set;}
    public DateTime ReleaseDate {get; set;}
    public bool IsLiked {get; set;}
    private int _duration;

    public int Duration
    {
        get {return _duration;}
        set
        {
            if (value < 0)
            {
                _duration = 0;
            }
            else
            {
                _duration = value;
            }
        }
    }

    public Track(string title, string artist, int duration, DateTime releaseDate)
    {
        Title = title;
        Artist = artist;
        Duration = duration;
        ReleaseDate = releaseDate;
        IsLiked = false;
    }

    public static int TracksAdded {get; private set;}
    public abstract void Play();
}