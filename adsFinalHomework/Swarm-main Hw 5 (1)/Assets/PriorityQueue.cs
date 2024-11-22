using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PriorityQueue<TElement, TPriority> : MonoBehaviour
{
    private readonly SortedDictionary<TPriority, Queue<TElement>> elements = new SortedDictionary<TPriority, Queue<TElement>>();

    public int Count { get; private set; }

    public void Enqueue(TElement element, TPriority priority)
    {
        if (!elements.ContainsKey(priority))
        {
            elements[priority] = new Queue<TElement>();
        }

        elements[priority].Enqueue(element);
        Count++;
    }

    public TElement Dequeue()
    {
        if (elements.Count == 0) throw new InvalidOperationException("Queue is empty.");

        var firstPair = elements.First();
        var element = firstPair.Value.Dequeue();

        if (firstPair.Value.Count == 0)
        {
            elements.Remove(firstPair.Key);
        }

        Count--;
        return element;
    }
}