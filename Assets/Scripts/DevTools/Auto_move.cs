// Move the gameObject at a fixed speed;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auto_move : MonoBehaviour
{
    public Vector3 speed;
    [Tooltip("speed when holding down the shift key")]
    public Vector3 shiftSpeed;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (shiftPressed) {
            transform.Translate(shiftSpeed * Time.deltaTime);
        } else {
            transform.Translate(speed * Time.deltaTime);
        }
    }
}
