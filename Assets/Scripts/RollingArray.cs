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

class RollingStats {
    public readonly int size;
    public float sum { get; private set; }
    public float average { get; private set; }
    public float variance { get; private set; }
    public float stddev { get; private set; }
    public float min { get; private set; }
    public float max { get; private set; }

    protected readonly RollingArray<float> values;

    public RollingStats(int size) {
        values = new RollingArray<float>(size);
        this.size = size;
    }

    public void Append(float val) {
        float oldValue = values[0];
        float oldAverage = average;

        sum -= oldValue;
        values.Append(val);
        sum += val;
        average = sum / size;
        variance += (val - oldValue) * (val - average + oldValue - oldAverage)/(size);
        stddev = (float)Math.Sqrt(variance);
    }

    public string toString() {
        string s = string.Format("sum: {0}, average: {1}, variance: {2}, stddev: {3}, values: ", sum, average, variance, stddev);
        for (int i = 0; i < size; i++) {
            s += values[i] + ",";
        }
        return s;
    }
}
