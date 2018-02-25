using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public bool DebugMovement;
    public float DebugCameraSpeed = 1.0f;
    public static CameraManager Instance;
    public Camera MainCamera;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    void Start ()
    {
#if SERVER
        StartCoroutine(DebugMovementUpdate());
#else
        if(DebugMovement)
        {
            StartCoroutine(DebugMovementUpdate());
        }
#endif
    }

    public void ChangeCameraPosition(Vector3 newPosition)
    {
        newPosition.z = MainCamera.transform.position.z;
        MainCamera.transform.position = newPosition;
    }

    IEnumerator DebugMovementUpdate()
    {
        Vector3 CurrentVelocity = Vector3.zero;

        while(true)
        {
            CurrentVelocity *= 0.95f;
            if (Input.GetKey(KeyCode.A))
            {
                CurrentVelocity -= DebugCameraSpeed * Vector3.right;
            }
            if (Input.GetKey(KeyCode.D))
            {
                CurrentVelocity += DebugCameraSpeed * Vector3.right;
            }
            if (Input.GetKey(KeyCode.W))
            {
                CurrentVelocity += DebugCameraSpeed * Vector3.up;
            }
            if (Input.GetKey(KeyCode.S))
            {
                CurrentVelocity += DebugCameraSpeed * Vector3.down;
            }
            MainCamera.transform.position += CurrentVelocity;
            yield return new WaitForEndOfFrame();
        }
    }
}
