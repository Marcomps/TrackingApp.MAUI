using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace TrackingApp.Helpers
{
    /// <summary>
    /// ObservableCollection that supports bulk-replace with a single Reset notification,
    /// avoiding O(N²) UI re-renders when replacing large sets of items.
    /// </summary>
    public class BulkObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Clears and repopulates the collection, firing exactly ONE CollectionChanged
        /// (Reset) notification instead of N individual Add notifications.
        /// </summary>
        public void ReplaceAll(IEnumerable<T> items)
        {
            Items.Clear();
            foreach (var item in items)
                Items.Add(item);

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
