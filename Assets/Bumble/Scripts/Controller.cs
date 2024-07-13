using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float rotationSpeed = 0.35f;
    public float moveSpeed = 30f;
    public float targetPosStep = 10f;
    public int WebFieldSize = 3;
    public GameObject target;

    private int[] CurFieldPos = new int[] { 1 , 1 };
    private Rigidbody _rb;
    [SerializeField]
    private bool onTarget = false;
    [SerializeField]
    private bool lootAtZero = true; //чтобы сделать фото смотрим на север
    //private bool lookToTarget = false;
    private bool stop = false;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!stop)
        {
            moveToTarget();
            if (onTarget && lootAtZero)
            {
                if (sendImage())
                {
                    if(getNewTarget()== false) stop = true;
                    onTarget = false;
                    ///lootAtZero = false;
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == target.name)
        {
            onTarget = true;
            Debug.Log(other.name);
        }
    }

    void moveToTarget()
    {
        //if (onTarget & !lootAtZero)
        //{
        //    //transform.position = target.transform.position;
        //    transform.rotation = target.transform.rotation;
        //    //quaternion direction = target.transform.rotation;
        //    //transform.rotation = Quaternion.LerpUnclamped(transform.rotation,direction, Time.deltaTime);
        //    if (transform.rotation == target.transform.rotation) lootAtZero = true;
        //    Debug.Log(transform.rotation);

        //}
        //движемся к цели
        if(!onTarget)
        {
            transform.position = Vector3.Lerp(transform.position, target.transform.position, Time.deltaTime);
        }
        //поворачиваемся к цели
        //if (!lookToTarget)
        //{
        //    Quaternion direction = Quaternion.LookRotation(target.transform.position - transform.position);
        //    transform.rotation = Quaternion.Lerp(transform.rotation,direction, Time.deltaTime);
        //}
    }
    bool getNewTarget()
    {
        if (CurFieldPos[1] == WebFieldSize+1) return false;
        if (CurFieldPos[1] % 2 == 0)
        {
            if (CurFieldPos[0] == 1)
            {
                CurFieldPos[1] += 1;
                target.transform.position = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z + targetPosStep);
            }
            else {
                CurFieldPos[0] -= 1;
                target.transform.position = new Vector3(target.transform.position.x - targetPosStep, target.transform.position.y, target.transform.position.z);
            }
        }
        else
        {
            if (CurFieldPos[0] == WebFieldSize)
            {
                CurFieldPos[1] += 1;
                target.transform.position = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z + targetPosStep);
            }
            else
            {
                CurFieldPos[0] += 1;
                target.transform.position = new Vector3(target.transform.position.x + targetPosStep, target.transform.position.y, target.transform.position.z);
            }
        }
        //target.transform.position = new Vector3(target.transform.position.x + targetPosStep, target.transform.position.y, target.transform.position.z);
        //Debug.Log(CurFieldPos[0]+" " + CurFieldPos[1]);
        //Debug.Log(target.transform.position);
        return true;
    }

    bool sendImage()
    {
        ScreenCapture.CaptureScreenshot("Photo[" + CurFieldPos[0] + "][" + CurFieldPos[1] + "].png");
        Debug.Log("Image sended");
        return true;
    }
}
