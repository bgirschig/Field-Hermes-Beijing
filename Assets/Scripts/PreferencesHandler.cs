using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PreferencesHandler : MonoBehaviour
{
    [Header("Buttons")]
    public Button resetButton;
    
    [Header("Detector")]
    public Dropdown cameraSelector;
    public Detector detector;
    public SharedWebcam webcam;
    public Radio screenDirection;

    [Header("Movement")]
    public CameraSwing cameraSwing;
    public Dropdown swingModeDropdown;
    public SmoothFollow lanternPlane;
    public Slider speedSlider;
    public Slider idleSpeedSlider;
    public Slider lanterSmoothingSlider;

    [Header("Style")]
    public InputField lanternColorInput;
    public Renderer lantern;
    // controlled elements

    // Start is called before the first frame update
    void Start() {
        resetButton.interactable = false;
        resetButton.onClick.AddListener(reset);
        init();
    }

    void init() {
        cameraSelector.options.Clear();
        cameraSelector.AddOptions(webcam.devices);
        initOption("detector.camera_name", cameraSelector, (string val) => webcam.setCamera(val), "");
        initOption("detector.screenDirection", screenDirection, (int value) => detector.invert = value == 0, 0);
        
        swingModeDropdown.options.Clear();
        swingModeDropdown.AddOptions(CameraSwing.modes);
        initOption("swingControl.mode", swingModeDropdown, (string val) => cameraSwing.setMode(val), "DETECTOR");
        initOption("swingControl.speed", speedSlider, (float val) => cameraSwing.overallSpeed = val, 20);
        initOption("swingControl.idleSpeed", idleSpeedSlider, (float val) => cameraSwing.idleSpeed = val, 0.01f);

        initOption("lantern.smoothing", lanterSmoothingSlider, (float val) => lanternPlane.smoothTime = val, 0.2f);

        initOption("style.lanternColor", lanternColorInput, (string val) => {
            Color color = colorFromHex(val);
            lantern.material.SetColor("_Color", color);
            PortalItem.findTwin<Renderer>(lantern).material.SetColor("_Color", color);
        }, "#009178ff");
    }

    // Initialize a float config option: load from playerprefs, default value, update the global
    // 'resettable' and 'saveable' state, and change callback
    void initOption(string name, Slider input, UnityAction<float> onChange, float defaultValue=0) {
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetFloat(name, defaultValue);
        
        float value = PlayerPrefs.GetFloat(name);
        if (value != defaultValue) resetButton.interactable = true;
        
        input.value = value;
        onChange.Invoke(value);
        
        input.onValueChanged.AddListener(delegate {
            resetButton.interactable = true;
            onChange.Invoke(input.value);
            PlayerPrefs.SetFloat(name, input.value);
        });
    }

    // Initialize a float config option: load from playerprefs, default value, update the global
    // 'resettable' and 'saveable' state, and change callback
    void initOption(string name, Slider input, UnityAction<int> onChange, int defaultValue=0) {
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetInt(name, defaultValue);
        
        int value = PlayerPrefs.GetInt(name);
        if (value != defaultValue) resetButton.interactable = true;

        input.value = value;
        onChange.Invoke(value);
        
        input.onValueChanged.AddListener(delegate {
            resetButton.interactable = true;
            onChange.Invoke((int)input.value);
            PlayerPrefs.SetInt(name, (int)input.value);
        });
    }

    // Initialize a string config option: load from playerprefs, default value, update the global
    // 'resettable' and 'saveable' state, and change callback
    void initOption(string name, InputField input, UnityAction<string> onChange, string defaultValue="") {
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetString(name, defaultValue);
        
        string value = PlayerPrefs.GetString(name);
        if (value != defaultValue) resetButton.interactable = true;

        input.text = value;
        onChange.Invoke(value);
        
        input.onEndEdit.AddListener(delegate {
            resetButton.interactable = true;
            onChange.Invoke(input.text);
            PlayerPrefs.SetString(name, input.text);
        });
    }

    // Initialize a boolean config option: load from playerprefs, default value, update the global
    // 'resettable' and 'saveable' state, and change callback
    void initOption(string name, Toggle input, UnityAction<bool> onChange, bool defaultValue=false) {
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetInt(name, defaultValue ? 1 : 0);
        
        bool value = PlayerPrefs.GetInt(name) == 1;
        if (value != defaultValue) resetButton.interactable = true;

        input.isOn = value;
        onChange.Invoke(value);
        
        input.onValueChanged.AddListener(delegate {
            resetButton.interactable = true;
            onChange.Invoke(input.isOn);
            PlayerPrefs.SetInt(name, input.isOn ? 1 : 0);
        });
    }

    // Initialize a 'radio' config option: load from playerprefs, default value, update the global
    // 'resettable' and 'saveable' state, and change callback
    void initOption(string name, Radio input, UnityAction<int> onChange, int defaultValue=0) {
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetInt(name, defaultValue);
        
        int value = PlayerPrefs.GetInt(name);
        if (value != defaultValue) resetButton.interactable = true;

        input.setValue(value);
        onChange.Invoke(value);
        
        input.onChange.AddListener(delegate {
            resetButton.interactable = true;
            onChange.Invoke(input.value);
            PlayerPrefs.SetInt(name, input.value);
        });
    }

    // Initialize a dropdown config option: load from playerprefs, default value, update the global
    // 'resettable' and 'saveable' state, and change callback
    // Note: dropdown outputs an index, but to keep consistency if the order changes, we are saving
    // the label instead of its index in the options list.
    void initOption(string name, Dropdown input, UnityAction<string> onChange, string defaultValue="emulator") {
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetString(name, defaultValue);
        
        string savedValue = PlayerPrefs.GetString(name);
        if (savedValue != defaultValue) resetButton.interactable = true;

        bool foudValue = false;
        for (int i = 0; i < input.options.Count; i++)
        {
            if (input.options[i].text == savedValue) {
                foudValue = true;
                break;
            }
        }

        int valueIndex = 0;
        if (foudValue) valueIndex = input.options.FindIndex(item => { return item.text == savedValue; });
        input.value = valueIndex;
        onChange.Invoke(input.options[valueIndex].text);

        input.onValueChanged.AddListener(delegate {
            resetButton.interactable = true;
            string newValue = input.options[input.value].text;
            onChange.Invoke(newValue);
            PlayerPrefs.SetString(name, newValue);
        });
    }

    // reset all options to their defaults
    public void reset() {
        PlayerPrefs.DeleteAll();
        resetButton.interactable = false;
        init();
    }

    // save the playerprefs
    // unused because playerprefs are saved automatically by unity,
    // making the "save" button rather confusing
    public void save() {
        PlayerPrefs.Save();
    }

    private static Color colorFromHex(string hexColor) {
        Color color = new Color();
        ColorUtility.TryParseHtmlString(hexColor, out color);
        return color;
    }
}
