// basic rolling array: fixed size, every time a value is added, the oldest one is removed

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class RollingArray<T> {
    public readonly int size;
    protected readonly T[] array;
    protected int front;

    public RollingArray(int size) {
        array = new T[size];
        this.size = size;
        front = 0;
    }

    public T this[int index] {
        get {
            return array[(front + index) % size];
        }
        set {
            array[(front + index) % size] = value;
        }
    }

    public void Append(T val) {
        front = (front + 1) % size;
        this[size-1] = val;
    }
}