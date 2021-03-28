using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode()]
public class PortalSequence : MonoBehaviour
{
    public bool loop;
    private List<GameObject> items;
    Portal firstPortal = null;

    // Start is called before the first frame update
    void Start()
    {
        UpdateChain();
    }

    void UpdateChain() {
        items = new List<GameObject>();
        firstPortal = null;
        Portal prevPortal = null;
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Portal portal = child.GetComponent<Portal>();
            if (portal != null) {
                if (firstPortal == null) firstPortal = portal;
                else portal.deactivate();

                if (i < transform.childCount-1) {
                    portal.target = transform.GetChild(i+1);
                } else {
                    portal.target = null;
                }

                if (prevPortal != null) prevPortal.nextPortal = portal;
                prevPortal = portal;
            }
        }
        firstPortal.activate();
        if (prevPortal != null) {
            if (loop) {
                // prevPortal.target = transform.GetChild(0);
                prevPortal.nextPortal = firstPortal;
            } else {
                prevPortal.nextPortal = null;
            }
        } else {
        }
    }

    #if UNITY_EDITOR
    void Update()
    {
        if(!EditorApplication.isPlaying ) UpdateChain();
    }
    #endif
}
