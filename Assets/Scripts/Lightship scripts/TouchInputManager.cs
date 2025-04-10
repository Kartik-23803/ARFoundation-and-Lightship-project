using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInputManager : MonoBehaviour
{
    public bool IsTouching()
    {
        #if UNITY_EDITOR
        return Input.GetMouseButton(0);
        #else
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        #endif
    }

    public Vector2 GetTouchPosition()
    {
        #if UNITY_EDITOR
        return Input.mousePosition;
        #else
        return Input.GetTouch(0).position;
        #endif
    }

    public bool IsTouchValidOnScreen(Vector2 touchPosition)
    {
        return touchPosition.x > 0 && touchPosition.x < Screen.width &&
               touchPosition.y > 0 && touchPosition.y < Screen.height;
    }
}
