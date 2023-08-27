using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float scrollSpeed = 2000.0f;

    void Start()
    {
        
    }

    void Update()
    {
        float scroollWheel = Input.GetAxis("Mouse ScrollWheel");

        Camera.main.fieldOfView += scroollWheel * Time.deltaTime * scrollSpeed;
    }
}
