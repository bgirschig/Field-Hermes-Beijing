using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SliderFeedback : MonoBehaviour
{
    Slider slider;
    Text text;

    void Start() {
        slider = transform.parent.GetComponentInChildren<Slider>();
        text = GetComponent<Text>();

        slider.onValueChanged.AddListener(val => updateText(val));
        updateText(slider.value);
    }

    void updateText(float val) {
        text.text = $"{val:0.00}";
    }

    void Reset() {
        Start();
    }
}
