/// toggle groups of Gameobjects on a key press

using UnityEngine;
using System;

public class GameobjectToggler : MonoBehaviour
{
    [Serializable]
    public class ToggleGroup {
        public string note;
        public GameObject[] gameObjects;
    }

    public ToggleGroup[] toggleGroups;
    public KeyCode toggleKey;
    public int currentGroup = 0;

    void Start() {
        setGroupActive(toggleGroups[currentGroup], true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(toggleKey)) {
            setGroupActive(toggleGroups[currentGroup], false);
            currentGroup = (currentGroup + 1) % toggleGroups.Length;
            setGroupActive(toggleGroups[currentGroup], true);
        }
    }

    void setGroupActive(ToggleGroup group, bool active) {
        foreach (var toggleGroup in toggleGroups) {
            foreach (var item in toggleGroup.gameObjects) {
                item.SetActive(false);
            }
        }
        foreach (var item in group.gameObjects) {
            item.SetActive(active);
        }
    }
}
