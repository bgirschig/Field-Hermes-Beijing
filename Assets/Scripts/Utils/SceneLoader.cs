using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start() {
        if (!SceneManager.GetSceneByName("Scenery").isLoaded) {
            SceneManager.LoadScene("Scenery", LoadSceneMode.Additive);
        }
    }
}
