// Swap between the child objects. Useful to compare model versions

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildSwapper : MonoBehaviour
{
    int currentIdx = -1;
    GameObject[] children;
    public string key = "space";

    // Start is called before the first frame update
    void Start()
    {
        children = new GameObject[transform.childCount];
        int idx = 0;
        foreach (Transform child in transform) {
            children[idx] = child.gameObject;
            idx += 1;
        }

        nextObject();
    }

    void nextObject() {
        foreach (GameObject child in children) {
            child.SetActive(false);
        }
        currentIdx = (currentIdx+1) % transform.childCount;
        children[currentIdx].gameObject.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(key)) nextObject();
    }
}
