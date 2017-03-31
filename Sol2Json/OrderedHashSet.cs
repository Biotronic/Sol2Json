using System.Collections.ObjectModel;

namespace SolJson
{
    public class OrderedHashSet<T> : KeyedCollection<T,T>
    {
        protected override T GetKeyForItem(T item)
        {
            return item;
        }
    }
}
