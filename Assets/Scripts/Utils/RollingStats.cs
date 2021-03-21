// Compute some statistics on a rolling array (sum, average, variance, min, max, standard deviation, etc...)
// These values are updated on each insert (rather than re-computed by looping over the whole array)

using System;

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
