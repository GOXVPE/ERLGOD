using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Camera cam;
    public Transform target;
    // Start is called before the first frame update
    void Start()
    {
        cam.transform.position = target.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        cam.transform.position = target.transform.position;
        cam.transform.rotation = target.transform.rotation * Quaternion.Euler(90f, 90f, 0f); 
    }
}
