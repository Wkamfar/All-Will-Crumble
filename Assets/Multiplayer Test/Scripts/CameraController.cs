using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraController : MonoBehaviour
{
    [SerializeField] float sensitivity;
    [SerializeField] Transform head;
    float xRot, yRot;
    //Vector2 lastPosition = new Vector2();
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        xRot = head.eulerAngles.x;
        yRot = head.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 delta = Mouse.current.delta.ReadValue();
        yRot += delta.x * sensitivity * Time.deltaTime;
        xRot -= delta.y * sensitivity * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -89f, 89f);
        head.eulerAngles = new Vector3(xRot, yRot);
        transform.eulerAngles = new Vector3(0, yRot);
    }
}
