namespace PlaylistKata.Domain;

public class Song : Track
{
    public string Genre {get; set;}
    public string Album{get; set;}

    public Song(string title, string artist, int duration, DateTime releaseDate, string genre, string album)
        : base(title, artist, duration, releaseDate)
    {
        Genre = genre;
        Album = album;
    }

    public override void Play()
    {
        Console.WriteLine($"Now playing: {Title} by {Artist} [Album: {Album} | Genre: {Genre}]");
    }
}