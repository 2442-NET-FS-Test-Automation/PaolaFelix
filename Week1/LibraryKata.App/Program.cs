namespace LibraryKata.App;

public class Program
{
    
    public static void Main()
    {
        DataTypesAndOperators();
    }

    private static void DataTypesAndOperators()
    {
        Console.WriteLine("=== Data types and operators ==");
        
        int copies = 3;
        double lateFee = 1;
        bool isMember = true;
        char shelf = 'A';
        string title = "Clean Code";

        string user = "Paola";
        int total = copies * 2;
        bool isEnough = total > 4;
        bool exactlySix = total == 6;
        bool lendable = isMember && isEnough;

        Console.WriteLine(title + " has been checked out by " + user);
        Console.WriteLine($"{title} on shelf {shelf}: {copies} copies, fee {lateFee}");
        
    }

}
