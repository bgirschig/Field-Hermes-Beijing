/// UI element displaying the current foreground and background brush color of a MaskCanvas

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorSelector : MonoBehaviour, IPointerClickHandler
{
    Image foreground;
    Image background;

    public MaskCanvas maskCanvas; // controlled mask canvas

    // Start is called before the first frame update
    void Start()
    {
        foreground = transform.GetChild(1).GetComponent<Image>();
        background = transform.GetChild(0).GetComponent<Image>();
        updateColors();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.X)) {
            maskCanvas.SwapColors();
            updateColors();
        }
    }

    void updateColors() {
        foreground.color = maskCanvas.foregroundColor;
        background.color = maskCanvas.backgroundColor;
    }

    public void OnPointerClick(PointerEventData evt) {
        print("click");
    }
}
