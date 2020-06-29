using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableMouse : MonoBehaviour
{
#if !UNITY_EDITOR //Se define para que en la release no se muestre el cursor
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
#endif
}
