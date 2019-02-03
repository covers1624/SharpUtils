﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharpUtilities
{
    /// <summary>
    /// Helper class for caching objects inside the dictionary. New object will be cached on the request.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    public class DictionaryCache<TKey, TValue> : ICache
    {
        /// <summary>
        /// The populate action
        /// </summary>
        private readonly Func<TKey, TValue> populateAction;

        /// <summary>
        /// The cached values
        /// </summary>
        private ConcurrentDictionary<TKey, TValue> values = new ConcurrentDictionary<TKey, TValue>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryCache{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="populateAction">The populate action.</param>
        public DictionaryCache(Func<TKey, TValue> populateAction)
        {
            this.populateAction = populateAction;
        }

        /// <summary>
        /// Clears this cache.
        /// </summary>
        public void Clear()
        {
            values.Clear();
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        public IEnumerable<TValue> Values => values.Values;

        /// <summary>
        /// Gets the number of entries inside the cache.
        /// </summary>
        public int Count => values.Count;

        /// <summary>
        /// Gets or sets the &lt;TValue&gt; with the specified key.
        /// </summary>
        /// <param name="key">The key value.</param>
        public TValue this[TKey key]
        {
            get
            {
                TValue value;

                if (!values.TryGetValue(key, out value))
                    lock (values)
                        if (!values.TryGetValue(key, out value))
                        {
                            value = populateAction(key);
                            values.TryAdd(key, value);
                        }
                return value;
            }

            set
            {
                values[key] = value;
            }
        }

        /// <summary>
        /// Gets the existing value in the cache associated with the specified key. Value won't be populated if it is not in cache.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key,
        /// if the key is found; otherwise, the default value for the type of the value parameter. This parameter
        /// is passed uninitialized.</param>
        /// <returns>
        ///   <c>true</c> if the <see cref="DictionaryCache{TKey, TValue}" /> contains an element with the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetExistingValue(TKey key, out TValue value)
        {
            return values.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the value in the cache associated with the specified key. Value will be populated if it is not in cache.
        /// </summary>
        /// <param name="typeName">The key of the value to get.</param>
        /// <param name="userType">When this method returns, contains the value associated with the specified key,
        /// if the key is found; otherwise, the default value for the type of the value parameter. This parameter
        /// is passed uninitialized.</param>
        /// <returns>
        ///   <c>true</c> if the <see cref="DictionaryCache{TKey, TValue}" /> contains an element with the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(TKey typeName, out TValue userType)
        {
            try
            {
                userType = this[typeName];
                return true;
            }
            catch (Exception)
            {
                userType = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Removes dictionary entry with given key.
        /// </summary>
        /// <param name="key">Key of entry to be removed..</param>
        /// <param name="value">Value assoiated with the key that is being removed.</param>
        public bool RemoveEntry(TKey key, out TValue value)
        {
            return values.TryRemove(key, out value);
        }

        /// <summary>
        /// Returns all cached values in this cache.
        /// </summary>
        /// <returns>IEnumerator of all the cache values.</returns>
        public IEnumerator GetEnumerator()
        {
            return values.GetEnumerator();
        }

        /// <summary>
        /// Invalidates this cache.
        /// </summary>
        public void InvalidateCache()
        {
            Clear();
        }
    }
}
