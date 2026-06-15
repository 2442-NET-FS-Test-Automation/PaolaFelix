using PlaylistKata.Domain;
namespace PlaylistKata.App;

public class Program
{
    static List<Track> playlist = new();
    static void Main()
    {
        playlist.Add(new Song("the cure", "Olivia Rodrigo", 297, new DateTime(2026, 5,22), "pop", "you seem pretty sad for a girl so in love"));
        playlist.Add(new Song("espresso", "Sabrina Carpenter", 175, new DateTime(2024, 8, 23), "pop", "Short n' Sweet"));
        playlist.Add(new PodcastEpisode("Semana Santa", "Lupita Villalobos y Kass Quezada", 4020, new DateTime(2026, 4, 29), "Las Alucines", "02x39"));

        var running = true;
        while (running)
        {
            PrintMenu();
            int choice = int.Parse(Console.ReadLine());  
            switch (choice)
            {
                case 1: AddSong(); break;
                case 2: AddPodcast(); break;
                case 3: ListTracks(); break;
                case 4: PlayTrack(); break;
                case 5: running = false; break;
                
                default:
                    Console.WriteLine("Invalid option, try again.");
                    break;
            }
        }
    }

    static void PrintMenu()
    {
        Console.WriteLine("=== PLAYLIST MENU");
        Console.WriteLine("1 -> Add Song");
        Console.WriteLine("2 -> Add a Podcast Episode");
        Console.WriteLine("3 -> List all tracks");
        Console.WriteLine("4 -> Play a track");
        Console.WriteLine("5 -> Quit");
        
    }

    static void AddSong()
    {
        
    }

    static void AddPodcast()
    {
        
    }

    static void ListTracks()
    {
        
    }

    static void PlayTrack()
    {
        
    }
}




