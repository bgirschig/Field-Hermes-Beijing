using UnityEngine;

public static class MathUtils {

  // val=1, from1=0, from2=1, to1=0, to2=715 -> Infinity
  public static float Map(float val, float from1, float from2, float to1, float to2) {
    return ((val - from1) / (from2 - from1)) * (to2 - to1) + to1;
  }

  public static float Map(float val, float from1, float from2, float to1, float to2, bool clamp) {
    val = Map(val, from1, from2, to1, to2);
    if (clamp) val = Mathf.Clamp(val, to1, to2);
    return val;
  }
}
