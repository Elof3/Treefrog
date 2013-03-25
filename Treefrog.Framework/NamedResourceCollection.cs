﻿using System;
using System.Collections.Generic;

namespace Treefrog.Framework
{
    public class NamedResourceCollection<T> : ResourceCollection<T>
        where T : INamedResource
    {
        private Dictionary<string, T> _indexMap;

        public event EventHandler<NamedResourceRemappedEventArgs<T>> ResourceRenamed;

        public NamedResourceCollection ()
        {
            _indexMap = new Dictionary<string, T>();
        }

        public T this[string name]
        {
            get
            {
                if (!_indexMap.ContainsKey(name))
                    throw new ArgumentException("The collection does not contain an item named '" + name + "'", "name");

                return _indexMap[name];
            }
        }

        public bool Contains (string name)
        {
            return _indexMap.ContainsKey(name);
        }

        protected override bool CheckAdd (T item)
        {
            if (_indexMap.ContainsKey(item.Name))
                return false;

            return base.CheckAdd(item);
        }

        protected override void AddCore (T item)
        {
            _indexMap.Add(item.Name, item);

            base.AddCore(item);
        }

        protected override void RemoveCore (T item)
        {
            base.RemoveCore(item);

            _indexMap.Remove(item.Name);
        }

        protected virtual void OnResourceRenamed (NamedResourceRemappedEventArgs<T> e)
        {
            var ev = ResourceRenamed;
            if (ev != null)
                ev(this, e);
        }

        private void NameChangingHandler (object sender, NameChangingEventArgs e)
        {
            if (!_indexMap.ContainsKey(e.OldName)) {
                e.Cancel = true;
            }

            if (_indexMap.ContainsKey(e.NewName)) {
                e.Cancel = true;
            }
        }

        private void NameChangedHandler (object sender, NameChangedEventArgs e)
        {
            if (!_indexMap.ContainsKey(e.OldName)) {
                throw new ArgumentException("The collection does not contain an item keyed by the old name: '" + e.OldName + "'");
            }

            if (_indexMap.ContainsKey(e.NewName)) {
                throw new ArgumentException("There is an existing item with the new name '" + e.NewName + "' in the collection");
            }

            T item = _indexMap[e.OldName];

            _indexMap.Remove(e.OldName);
            _indexMap[e.NewName] = item;

            OnResourceRenamed(new NamedResourceRemappedEventArgs<T>(item, e.OldName, e.NewName));
        }
    }
}
