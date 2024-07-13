using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCameraScript1 : MonoBehaviour
{

    public Transform target;
    public float distance = 10.0f;
    public float height = 5.0f;
    public float rotationDamping = 1.0f;
    public float heightDamping = 1.0f;
    private float distanceOffset = 0.0f;
    [SerializeField]
    private float lookForwardOffset = 2;

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(target.GetComponent<Rigidbody>().velocity.normalized, Vector3.up);
        Quaternion targetRotation = Quaternion.Euler(lookRotation.eulerAngles.x, lookRotation.eulerAngles.y, 0);
        Quaternion currentRotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationDamping * Time.deltaTime);
        Vector3 forward = target.forward;
        forward.y *= 0.2f;
        Vector3 targetPosition = target.position - forward * (distance + distanceOffset);
        Vector3 currentPosition = Vector3.Lerp(transform.position, targetPosition + Vector3.up * height, heightDamping * Time.deltaTime);
        transform.position = currentPosition;
        transform.rotation = currentRotation;
        transform.LookAt(target.position + forward*lookForwardOffset);
    }
}



