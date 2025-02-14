using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {
    [SerializeField] private BillboardType billboardType;
    [SerializeField] private Camera cameraToBillboardTowards;

    [Header("Lock Rotation")]
    [SerializeField] private bool lockX;
    [SerializeField] private bool lockY;
    [SerializeField] private bool lockZ;

    private Vector3 originalRotation;

    public Camera CameraToBillboardTowards { get => cameraToBillboardTowards; set => cameraToBillboardTowards = value; }

    public enum BillboardType { LookAtCamera, CameraForward };

    private void Awake() 
    {
        originalRotation = transform.rotation.eulerAngles;
        if(CameraToBillboardTowards == null)
        {
            CameraToBillboardTowards = Camera.main;
        }
    }

     // Use Late update so everything should have finished moving.
    private void LateUpdate() 
    {
        // There are two ways people billboard things.

        switch (billboardType) 
        {
        case BillboardType.LookAtCamera:
                {
                    transform.LookAt(CameraToBillboardTowards.transform.position, Vector3.up);
                    break;
                }
            
        case BillboardType.CameraForward:
                {
                    transform.forward = CameraToBillboardTowards.transform.forward;
                    break;
                }

        default:
                {
                    break;
                }
        }

        // Modify the rotation in Euler space to lock certain dimensions.
        Vector3 rotation = transform.rotation.eulerAngles;
        if (lockX)
        {
            rotation.x = originalRotation.x;
        }
        if (lockY)
        {
            rotation.y = originalRotation.y;
        }
        if (lockZ)
        {
            rotation.z = originalRotation.z;
        }


        transform.rotation = Quaternion.Euler(rotation);
  }
}
