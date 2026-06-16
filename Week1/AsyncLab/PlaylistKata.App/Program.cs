using PlaylistKata.Domain;
namespace PlaylistKata.App;

public class Program
{
    static List<Track> playlist = new();
    static void Main()
    {
        playlist.Add(new Song("the cure", "Olivia Rodrigo", 297, new DateTime(2026, 5,22), "pop", "you seem pretty sad for a girl so in love"));
        playlist.Add(new Song("espresso", "Sabrina Carpenter", 175, new DateTime(2024, 8, 23), "pop", "Short n' Sweet"));
        playlist.Add(new PodcastEpisode("Semana Santa", "Lupita Villalobos y Kass Quezada", 4020, new DateTime(2026, 4, 29), "Las Alucines", 39));

        var running = true;
        while (running)
        {
            PrintMenu();
            int choice = int.Parse(Console.ReadLine());  
            switch (choice)
            {
                case 1: AddTrackMenu(); break;
                case 2: ListTracks(); break;
                case 3: PlayTrack(); break;
                case 4: running = false; break;
                
                default:
                    Console.WriteLine("Invalid option, try again.");
                    break;
            }
        }
    }

    static void PrintMenu()
    {
        Console.WriteLine("=== PLAYLIST MENU");
        Console.WriteLine("1 -> Add Track");
        Console.WriteLine("2 -> List all tracks");
        Console.WriteLine("3 -> Play a track");
        Console.WriteLine("4 -> Quit");
        
    }

    static void AddTrackMenu()
    {
        Console.WriteLine("\n--- ADD NEW TRACK ---");

        Console.Write("Title: ");
        string title = Console.ReadLine() ?? "";

        Console.Write("Artist: ");
        string artist = Console.ReadLine() ?? "";

        Console.Write("Duration (in seconds): ");
        int duration = int.Parse(Console.ReadLine() ?? "0");

        Console.Write("Release Date (YYYY-MM-DD): ");
        DateTime releaseDate = DateTime.Parse(Console.ReadLine() ?? "2026-01-01");

        Console.Write("Is this a (1) Song or (2) Podcast? ");
        int typeChoice = int.Parse(Console.ReadLine() ?? "0");

        if (typeChoice == 1)
        {
            Console.Write("Genre: ");
            string genre = Console.ReadLine() ?? "";

            Console.Write("Album: ");
            string album = Console.ReadLine() ?? "";

            AddTrack(title, artist, duration, releaseDate, genre, album);
            Console.WriteLine($"Successfully added Song: \"{title}\"");
            Console.WriteLine($"Total tracks added: {Track.TracksAdded}");
        }
        else if (typeChoice == 2)
        {
            Console.Write("Show Name: ");
            string showName = Console.ReadLine() ?? "";

            Console.Write("Episode Number: ");
            int episodeNumber = int.Parse(Console.ReadLine() ?? "0");

            AddTrack(title, artist, duration, releaseDate, showName, episodeNumber);
            Console.WriteLine($"Successfully added Podcast Episode: \"{title}\"");
            Console.WriteLine($"Total tracks added: {Track.TracksAdded}");
        }
        else
        {
            Console.WriteLine("Invalid track type. Track not added.");
        }
    }

    static void AddTrack(string title, string artist, int duration, DateTime releaseDate, string genre, string album)
    {
        playlist.Add(new Song(title, artist, duration, releaseDate, genre, album));
        Track.IncrementTracksAdded();
    }

    static void AddTrack(string title, string artist, int duration, DateTime releaseDate, string showName, int episodeNumber)
    {
        playlist.Add(new PodcastEpisode(title, artist, duration, releaseDate, showName, episodeNumber));
        Track.IncrementTracksAdded();
    }

    static void ListTracks()
    {
        int index = 1;
        foreach (Track track in playlist)
        {
            string type = track is PodcastEpisode ? "Podcast" : "Song";

            Console.WriteLine($"{index}. {track.Title} - {track.Artist} ({track.Duration}s) [{type}]");
            index++;
        }
    }

    static void PlayTrack()
    {
        ListTracks();
        if (playlist.Count == 0) return;

        Console.Write("\nEnter the track number to play: ");
        int index = int.Parse(Console.ReadLine() ?? "0") - 1;

        if (index >= 0 && index < playlist.Count)
        {
            Console.WriteLine();
            playlist[index].Play(); 
        }
        else
        {
            Console.WriteLine("Invalid track number.");
        }
    }
}




