/// Display the current app's ID (name, version, etc...) on the attached Text element

using UnityEngine;
using UnityEngine.UI;

public class AppId : MonoBehaviour
{
    void Start()
    {
        GetComponent<Text>().text = $"com.{Application.companyName}.{Application.productName} {Application.version} (unity {Application.unityVersion})";
        print($"Application.identifier {Application.identifier}");
    }
 }
