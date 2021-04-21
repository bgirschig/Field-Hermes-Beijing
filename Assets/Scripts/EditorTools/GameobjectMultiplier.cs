using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameobjectMultiplier : MonoBehaviour
{
    public int countX = 5;
    public int countZ = 5;
    public Bounds bounds;

    // Start is called before the first frame update
    void Start()
    {
        GameObject twin = GameObject.Instantiate(gameObject);
        Destroy(twin.GetComponent<GameobjectMultiplier>());
        twin.transform.parent = transform;

        // we won't be rendering the original instance. only the copies
        Destroy(GetComponent<MeshRenderer>());
        Destroy(GetComponent<MeshFilter>());
        Destroy(twin);

        float offsetX = bounds.size.x * countX / 2f;
        float offsetZ = bounds.size.z * countZ / 2f;
        for (int x = 0; x < countX; x++)
        {
            for (int z = 0; z < countZ; z++) {
                GameObject.Instantiate(
                    twin,
                    new Vector3(bounds.size.x*x - offsetX, 0, bounds.size.z*z - offsetZ),
                    Quaternion.identity,
                    transform);
            }
        }
    }

    void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
