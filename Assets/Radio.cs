using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Radio : MonoBehaviour
{
    public int value = 0;
    public Color selectedColor;
    public Color unselectedColor;
    public UnityEvent onChange = new UnityEvent();

    Button[] buttons;

    // Start is called before the first frame update
    void Start()
    {
        buttons = GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++) buttons[i].onClick.AddListener(OnClickButton);

        UpdateView();
    }

    void OnClickButton() {
        var button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var idx = button.GetComponent<RadioOption>().value;
        setValue(idx);
    }

    void UpdateView() {
        for (int i = 0; i < buttons.Length; i++) {
            bool isSelected = (i == value);
            buttons[i].targetGraphic.color = isSelected ? selectedColor : unselectedColor;
        }
    }

    public void setValue(int newValue) {
        value = newValue;
        UpdateView();
        onChange.Invoke();
    }
}
