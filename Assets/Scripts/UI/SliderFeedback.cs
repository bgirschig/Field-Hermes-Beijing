using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SliderFeedback : MonoBehaviour
{
    Slider slider;
    Text text;
    public string format = "0.00";

    void Start() {
        slider = transform.parent.GetComponentInChildren<Slider>();
        text = GetComponent<Text>();

        slider.onValueChanged.AddListener(val => updateText(val));
        updateText(slider.value);
    }

    void updateText(float val) {
        text.text = val.ToString(format);
    }

    void Reset() {
        Start();
    }

    #if UNITY_EDITOR
    void Update() {
        if (!Application.isPlaying) {
            updateText(slider.value);
        }
    }
    #endif
}
