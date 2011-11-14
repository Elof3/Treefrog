﻿using System;
using System.Collections.Generic;

namespace Treefrog.Framework.Model
{
    /// <summary>
    /// Represents a collection of stacked <see cref="Tile"/> objects ordered from bottom to top.
    /// </summary>
    public class TileStack : IEnumerable<Tile>
    {
        #region Fields

        private List<Tile> _tiles;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty <see cref="TileStack"/>.
        /// </summary>
        public TileStack ()
        {
            _tiles = new List<Tile>();
        }

        /// <summary>
        /// Creates a new <see cref="TileStack"/> containing the same <see cref="Tile"/> objects as <paramref name="stack"/>.
        /// </summary>
        /// <param name="stack">The <see cref="TileStack"/> to copy <see cref="Tile"/> references from.</param>
        public TileStack (TileStack stack)
        {
            if (stack == null) {
                stack = new TileStack();
            }

            _tiles = new List<Tile>(stack._tiles);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a <see cref="Tile"/> object from the given index.
        /// </summary>
        /// <param name="index">The index of a <see cref="Tile"/> in the stack.  Index 0 is the bottom-most tile.</param>
        /// <returns>A <see cref="Tile"/> at the given index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the requested index is out of range for this <see cref="TileStack"/>.</exception>
        public Tile this[int index]
        {
            get
            {
                if (index < 0 || index >= _tiles.Count) {
                    throw new IndexOutOfRangeException("The range of acceptable indexes for this TileStack is (" + index.ToString() + " - " + _tiles.Count.ToString() + ").");
                }
                return _tiles[index];
            }
        }

        /// <summary>
        /// Gets the number of <see cref="Tile"/> objects in the stack.
        /// </summary>
        public int Count
        {
            get { return _tiles.Count; }
        }

        /// <summary>
        /// Gets the top-most <see cref="Tile"/> from the stack, or <c>null</c> if the stack is empty.
        /// </summary>
        public Tile Top
        {
            get
            {
                if (_tiles.Count == 0) {
                    return null;
                }

                return _tiles[_tiles.Count - 1];
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the internal state of the TileStack is modified.
        /// </summary>
        public event EventHandler Modified;

        #endregion

        #region Event Dispatchers

        /// <summary>
        /// Raises the <see cref="Modified"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnModified (EventArgs e)
        {
            if (Modified != null) {
                Modified(this, e);
            }
        }

        #endregion

        /// <summary>
        /// Adds a new <see cref="Tile"/> to the top of the stack.
        /// </summary>
        /// <param name="tile">The <see cref="Tile"/> to add.</param>
        public void Add (Tile tile)
        {
            _tiles.Add(tile);
            OnModified(EventArgs.Empty);
        }

        /// <summary>
        /// Removes the given <see cref="Tile"/> from the stack if it exists.
        /// </summary>
        /// <param name="tile">The <see cref="Tile"/> to remove.</param>
        public void Remove (Tile tile)
        {
            _tiles.Remove(tile);
            OnModified(EventArgs.Empty);
        }

        /// <summary>
        /// Clears the <see cref="TileStack"/>.
        /// </summary>
        public void Clear ()
        {
            _tiles.Clear();
            OnModified(EventArgs.Empty);
        }

        #region IEnumerable<Tile> Members

        /// <inherit/>
        public IEnumerator<Tile> GetEnumerator ()
        {
            foreach (Tile t in _tiles) {
                yield return t;
            }
        }

        #endregion

        #region IEnumerable Members

        /// <inherit/>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
