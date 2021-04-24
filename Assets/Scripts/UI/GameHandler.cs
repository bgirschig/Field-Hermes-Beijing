using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public void quitGame() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit(0);
        #endif
    }
}
