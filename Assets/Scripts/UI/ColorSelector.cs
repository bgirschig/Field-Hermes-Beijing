/// UI element displaying the current foreground and background brush color of a MaskCanvas

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorSelector : MonoBehaviour, IPointerClickHandler
{
    Image image;

    public MaskCanvas maskCanvas; // controlled mask canvas

    // Start is called before the first frame update
    void Start()
    {
        image = transform.GetChild(0).GetComponent<Image>();
        updateColor();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.X) && EventSystem.current.currentSelectedGameObject == null) {
            maskCanvas.SwapColors();
            updateColor();
        }
    }

    void updateColor() {
        image.color = maskCanvas.foregroundColor;
    }

    public void OnPointerClick(PointerEventData evt) {
        maskCanvas.SwapColors();
        updateColor();
    }
}
