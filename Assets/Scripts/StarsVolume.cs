using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StarsVolume : MonoBehaviour {
  public int starCount = 1000;
  public float starSize = 0.01f;
  public Material material;

  public Bounds bounds;

  private ParticleSystem.Particle[] points;
  private ParticleSystem particle_system;

  private void Create() {
    points = new ParticleSystem.Particle[starCount];

    for (int i = 0; i < starCount; i++) {
      points[i].position = new Vector3(
        Random.Range(bounds.min.x, bounds.max.x),
        Random.Range(bounds.min.y, bounds.max.y),
        Random.Range(bounds.min.z, bounds.max.z)
      );

      points[i].startSize = starSize;
      points[i].startColor = new Color(1, 1, 1, 1);
      points[i].rotation = 0;
    }

    particle_system = gameObject.GetComponent<ParticleSystem>();
    particle_system.SetParticles(points, points.Length);
  }

  void Start() {
    Create();
  }

  void Update() {
    // Create();
  }

  void Reset() {
    particle_system = gameObject.GetComponent<ParticleSystem>();

    var main = particle_system.main;
    main.loop = false;
    main.startSpeed = 0;
    main.simulationSpeed = 0;
    main.startDelay = 0;
    main.playOnAwake = false;

    var emission = particle_system.emission;
    emission.enabled = false;

    var shape = particle_system.shape;
    shape.enabled = false;

    var renderer = particle_system.GetComponent<ParticleSystemRenderer>();
    renderer.material = material;
  }

  public void setStarCount(int count) {
    starCount = count;
    Create();
  }

  public void OnValidate() {
    var renderer = GetComponent<ParticleSystemRenderer>().material = material;
  }

  public void OnDrawGizmosSelected() {
    Gizmos.matrix = transform.localToWorldMatrix;
    Gizmos.DrawWireCube(bounds.center, bounds.size);
  }
}