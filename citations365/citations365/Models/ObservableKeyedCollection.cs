using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace citations365.Models {
    public class ObservableKeyedCollection : KeyedCollection<string, Quote>, INotifyCollectionChanged, INotifyPropertyChanged {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Specifies the key value for the dictionnary
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override string GetKeyForItem(Quote item) {
            return item.Link;
        }

        /// <summary>
        /// Delete all item from the collection
        /// </summary>
        protected override void ClearItems() {
            base.ClearItems();
            NotifyCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        /// <summary>
        /// Add an item to the collection with its associated key
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int index, Quote item) {
            base.InsertItem(index, item);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        /// <summary>
        /// Delete an item from the collection with its associated key
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveItem(int index) {
            var item = this.Items[index];
            base.RemoveItem(index);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        /// <summary>
        /// Replace the item at the specified index by the new on passed to the method
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void SetItem(int index, Quote item) {
            base.SetItem(index, item);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, item, index);
        }

        /// <summary>
        /// Notifies when an collection's property has changed
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyPropertyChanged(String propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Notifies when an action has been performed on the collection
        /// </summary>
        /// <param name="action">The corresponding action</param>
        /// <param name="item">The item involved in the action</param>
        /// <param name="index">The index in the collection where the action took place</param>
        private void NotifyCollectionChanged(NotifyCollectionChangedAction action, Quote item, int index) {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (null != handler) {
                handler(this, new NotifyCollectionChangedEventArgs(action, item, index));
            }
        }

        /// <summary>
        /// Notifies when an action has been performed on the collection
        /// </summary>
        /// <param name="action">The corresponding action</param>
        private void NotifyCollectionChanged(NotifyCollectionChangedAction action) {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (null != handler) {
                handler(this, new NotifyCollectionChangedEventArgs(action));
            }
        }
    }
}
