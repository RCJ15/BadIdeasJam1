using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that contains an array of <see cref="Component"/> of type <typeparamref name="T"/>. <para/>
/// This class has custom editor implementation to allow for caching in the editor. <para/>
/// Also supports caching any interface type. 
/// </summary>
[Serializable]
public class ComponentCollection<T> : IEnumerable<T> where T : class
{
    [SerializeField] private Component[] components;

    /// <summary>
    /// Array of the Component Collection.
    /// </summary>
    public T[] Array
    {
        get
        {
            if (_array == null)
            {
                List<T> tempList = new List<T>();

                foreach (Component component in components)
                {
                    if (!typeof(T).IsAssignableFrom(component.GetType()))
                    {
                        continue;
                    }

                    tempList.Add(component as T);
                }

                _array = tempList.ToArray();
            }

            return _array;
        }
    }
    private T[] _array = null;

    /// <summary>
    /// The length of this collection, but cached to hopefully be only slightly more optimized.
    /// </summary>
    public int Length
    {
        get
        {
            if (!_lengthCache.HasValue)
            {
                _lengthCache = Array.Length;
            }

            return _lengthCache.Value;
        }
    }
    private int? _lengthCache;

    public T this[int index] => Array[index];

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)Array).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Array.GetEnumerator();
    }

    public static implicit operator T[](ComponentCollection<T> componentCollection) => componentCollection.Array;
}