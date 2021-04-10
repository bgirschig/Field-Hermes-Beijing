using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class Stars : MonoBehaviour {
  public int starCount = 1000;
  
  public float minRotation = 0;
  public float maxRotation = 0;

  public float starSize = 0.01f;
  public bool compensatePerspective = true;

  public float clipXMin = 0;
  public float clipXMax = 1;
  public float clipYMin = 0;
  public float clipYMax = 1;
  public float clipZMin = 0;
  public float clipZMax = 1;

  private ParticleSystem.Particle[] points;
  private ParticleSystem star_particleSystem;

  private float minX;
  private float maxX;
  private float minY;
  private float maxY;
  private float minZ;
  private float maxZ;
  private float frustumHeight;
  private float frustumWidth;
  private float frustumDepth;

  public void UpdateCameraParams() {
    frustumDepth = Camera.main.farClipPlane - Camera.main.nearClipPlane;
    frustumHeight = 2.0f * (Camera.main.farClipPlane - (1-clipZMax)*frustumDepth) * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
    frustumWidth = frustumHeight * Camera.main.aspect;

    minX = -frustumWidth / 2f + clipXMin * frustumWidth;
    maxX =  frustumWidth / 2f - (1-clipXMax) * frustumWidth;
    minY = -frustumHeight / 2f + clipYMin * frustumHeight;
    maxY =  frustumHeight / 2f - (1-clipYMax) * frustumHeight;
    minZ = -Camera.main.farClipPlane + (1-clipZMax) * frustumDepth;
    maxZ = -Camera.main.nearClipPlane - (clipZMin) * frustumDepth;

    frustumWidth *= clipXMax - clipXMin;
    frustumHeight *= clipYMax - clipYMin;
    frustumDepth *= clipZMin + clipZMax;
  }

  private void Create() {
    points = new ParticleSystem.Particle[starCount];

    for (int i = 0; i < starCount; i++) {
      points[i].position = Camera.main.cameraToWorldMatrix.MultiplyPoint(new Vector3(
        Random.Range(minX, maxX),
        Random.Range(minY, maxY),
        Random.Range(minZ, maxZ)
      ));

      points[i].startSize = starSize;
      points[i].startColor = new Color(1, 1, 1, 1);
      points[i].rotation = Random.Range(minRotation, maxRotation);
    }

    star_particleSystem = gameObject.GetComponent<ParticleSystem>();
    star_particleSystem.SetParticles(points, points.Length);
  }

  void Start() {
    UpdateCameraParams();
    Create();
    // FitStars();
  }

  void FitStars() {
    // Loop particles around so that we can have a small number of particles "following" the
    // camera. We're using camera-space position rather than viewport positions because looping
    // particles from front to back using this technique creates artefacts (particles get
    // centered on the camera's z axis when going backward)
    // Camera-space positions allow for a much more consistent behaviour (at the cost of having
    // some stars "wasted" out of view)
    Vector3 relativePos;

    for (int i = 0; i < starCount; i++) {      
      relativePos = Camera.main.worldToCameraMatrix.MultiplyPoint(points[i].position);

      if (relativePos.x < minX) relativePos.x += frustumWidth;
      if (relativePos.x > maxX) relativePos.x -= frustumWidth;

      if (relativePos.y < minY) relativePos.y += frustumHeight;
      if (relativePos.y > maxY) relativePos.y -= frustumHeight;

      if (relativePos.z < minZ) relativePos.z += frustumDepth;
      if (relativePos.z > maxZ) relativePos.z -= frustumDepth;

      points[i].position = Camera.main.cameraToWorldMatrix.MultiplyPoint(relativePos);
      points[i].startSize = compensatePerspective ? relativePos.z*starSize : starSize;
    }
    star_particleSystem.SetParticles(points, points.Length);
  }

  void Update() {
    FitStars();
  }

  float Map(float val, float from1, float from2, float to1, float to2) {
    return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
  }
  float Map(float val, float from1, float from2, float to1, float to2, bool clamp) {
    val = Map(val, from1, from2, to1, to2);
    if (clamp) val = Mathf.Clamp(val, to1, to2);
    return val;
  }

  public void setStarCount(int count) {
    starCount = count;
    Create();
  }

  void OnValidate() {
    UpdateCameraParams();
  }

  public void OnDrawGizmosSelected() {
    Gizmos.color = Color.cyan;
    Gizmos.matrix = Camera.main.transform.localToWorldMatrix;
    var center = new Vector3 (
      (minX+maxX) / 2,
      (minY+maxY) / 2,
      -(minZ+maxZ) / 2
    );
    var size = new Vector3 (
      maxX-minX,
      maxY-minY,
      maxZ-minZ
    );
    Gizmos.DrawWireCube(center, size);
  }
}