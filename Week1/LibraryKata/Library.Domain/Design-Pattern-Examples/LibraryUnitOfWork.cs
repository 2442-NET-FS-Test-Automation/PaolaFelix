using Serilog;

namespace LibraryKata.Domain;

public class LibraryUnitOfWork : IUnitOfWork
{
    public ILibraryRepository Items {get;}

    private readonly List<string> _staged = new();
    public LibraryUnitOfWork(ILibraryRepository items)
    {
        Items = items;
    }

    public int Commit()
    {
        int count = _staged.Count;
        Log.Information($"LibraryUnitOfWork commited{count} staged changes(s)", count);
        _staged.Clear();

        return count;
    }

    public void Stage(string change)
    {
        _staged.Add(change);
    }
}