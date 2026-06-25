using System.Collections;
namespace LibraryKata.Domain;

//The second half of my class
// I dont have to mirror rhe interface implementation or any inheritance across both class pfiles
// however, I can still only inherit from parent
public partial class Catalog : IEnumerable<LibraryItem>
{
    // this is the one we actually want to provide logic for, the one that uses a generic

    public IEnumerator<LibraryItem> GetEnumerator()
    {
        foreach (LibraryItem item in _items)
        {
            // We want to lazily return items one at a time, we dont want to return a second list
            // or anything like that. We will use "yield" with out return
            yield return item;
        }
           
    }

    // This version
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    // Lets make a method to return only ILendable items (things that implement the ILendable interface)
    public IEnumerable<LibraryItem> Lendable()
    {
        foreach (LibraryItem item in _items)
        {
            if(item is ILendable)
            {
                yield return item;
            }
        }
    }
    // Search function for the catalog
    // We are going to use Predicate to pass a delegate to our function
    // A deleate is just a reference to mehod in an arguments list
    // Predicate<LibraryItem> match represents a function that takes a LibraryItem, and returns a boolean

    // When we call this Find() method, we will combine it with a Lambda. Lambdas are the C# implementation
    // of anonymous or arrow functions. Just a quick definition that we dont bother storing a reference to.
    // authorItems = Find(item => item.Author == "Frank Herbert"); find every item where its author equals "Frank Herbert"
    public List<LibraryItem> Find(Predicate<LibraryItem> match)
    {
        // match is a methos, not an object or a value
        // its a pointer to some methos that gets passed in when we call Find()
        List<LibraryItem> foundItems = new();

        foreach (LibraryItem item in _items)
        {
            if(match(item))
            {
                foundItems.Add(item);
            }
        }
        return foundItems;
    }

}