using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.UI;

public class DroneFlyController : MonoBehaviour
{
    public Rigidbody _rb;

    [SerializeField]
    private bool hoverMode = true;
    private float hoverHeight;
    private float hoverHeightRange = 1; // �������� ������ �������
    private float hoverMaxPower = 1.3f; // ������� �������� ��� �������� �������
    private float hoverMinPower = 0.7f;

    // ������ ����������
    private float moveHorizontal = 0;
    private float moveVertical = 0;
    private float moveHeight = 0;
    private float moveRotation = 0;

    // ������� ����������
    [SerializeField]
    public Vector3 speedVector = Vector3.zero;
    private Vector3 thrustVector = Vector3.zero;
    public Vector3 accelerationVector = Vector3.zero;
    private Vector3 lastSpeed = Vector3.zero;
    private Vector3 startPointPosition; // ������� ������������ ������� ��� ������������� 
    private Vector3 curPosition;

    [SerializeField]
    private float baseEnginePower; // ���� ��� ������� �������������� �������
    [SerializeField]
    public float flyPower; // ����� ����������� ���� (1=�������) 
    private float flyPowerStep = 0.05f; // ��� �����
    [SerializeField]
    private float enginePower; // �������� ����
    [SerializeField]
    private float maxEnginePower = 150; // ������������ �������� ���������, ������ ������������� � ����������
    [SerializeField]
    private float rotSpeed = 3; // �������� ��������, ������ ������������� � ����������
    [SerializeField]
    private float speedMultiplier = 1.0f; // ��������� �������� ��� ���������� �������� �����

    // ���������� ��� ���������� ���� ���������
    private int callibStage = 0;
    private bool callibMode = false;
    private float callibHeight = 1.5f;
    private float callibEnginePowerTemp;
    private float callibEnginePowerStep = 0.01f;

    // ������� � �����������
    public Text �������������;
    public Text �����������;
    public Text ��������������;
    public Text ����Roll�����;  // ��������� ���� ��� Roll ����
    public Text ����Pitch�����; // ��������� ���� ��� Pitch ����
    public Text ����Yaw�����;   // ��������� ���� ��� Yaw ����
    public Text ����������������;

    public float altitude;
    private float altitudeError;
    private float accelerationError;
    private float angleError;
    private float directionError;

    // ��������� �����
    [SerializeField]
    private Vector3 windForce = Vector3.zero; // ���� �����
    private Vector3 windDirection = Vector3.zero; // ����������� �����
    [SerializeField]
    private float windChangeInterval = 2.0f; // �������� ��������� ����������� ����� (� ��������), ������ ������������� � ����������
    [SerializeField]
    private float windStrength = 15.0f; // ���� �����, ������ ������������� � ����������
    private float windChangeTimer = 0.0f; // ������ ��� ��������� ����������� �����

    public Vector3 CurrentSpeed
    {
        get { return speedVector; }
    }



    // ������������ �����
    [SerializeField]
    private List<Camera> cameras; // ������ �����, ������� ����� �������������
    private int currentCameraIndex = 0;

    private void Start()
    {
        startPointPosition = transform.position;
        curPosition = Vector3.zero;

        altitudeError = 0.05f; // ������ ����������� ��� ����������
        accelerationError = 0.02f; // ������ ����������� ��� �������������
        angleError = 1f; // ������ ����������� ��� ����
        directionError = 2f; // ������ ����������� ��� ����������� ������

        // ��������� �������� ��� �����
        ChangeWindDirection();

        // ��������, ��� ������ ���� ������ ������� ��� ������
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].gameObject.SetActive(i == currentCameraIndex);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) // ����� �������
        {
            hoverMode = !hoverMode;
            if (hoverMode)
            {
                hoverHeight = curPosition.y;
                Debug.Log($"Hover mode activated. Hover height set to {hoverHeight}");
            }
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            callibMode = true;
            callibStage = 0;
            Debug.Log("Calibration mode activated");
        }
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
        moveHeight = Input.GetAxis("Height");
        moveRotation = Input.GetAxis("Yaw");

        // ������� ����� ������ �������
        if (moveHeight != 0) hoverHeight = curPosition.y;

        // ���������� �������� ��������
        altitude = curPosition.y + Random.Range(-altitudeError, altitudeError);
        Vector3 accelerationWithError = accelerationVector + new Vector3(Random.Range(-accelerationError, accelerationError), Random.Range(-accelerationError, accelerationError), Random.Range(-accelerationError, accelerationError));
        float rollAngle = transform.eulerAngles.x + Random.Range(-angleError, angleError);
        float pitchAngle = transform.eulerAngles.z + Random.Range(-angleError, angleError);
        float yawAngle = transform.eulerAngles.y + Random.Range(-angleError, angleError);
        float direction = transform.eulerAngles.y + Random.Range(-directionError, directionError);

        �������������.text = "��������: " + speedVector.magnitude.ToString("F2");
        �����������.text = "������: " + altitude.ToString("F2") + " �";
        ��������������.text = "���������: " + accelerationWithError.ToString("F2");
        ����Roll�����.text = "Roll: " + rollAngle.ToString("F2") + " ��������";
        ����Pitch�����.text = "Pitch: " + pitchAngle.ToString("F2") + " ��������";
        ����Yaw�����.text = "Yaw: " + yawAngle.ToString("F2") + " ��������";
        ����������������.text = "�����������: " + direction.ToString("F2") + " ��������";

        // ���������� ����������� ����� ����� ������������ ��������� �������
        windChangeTimer += Time.deltaTime;
        if (windChangeTimer > windChangeInterval)
        {
            windChangeTimer = 0.0f;
            ChangeWindDirection();
        }

        // ������������ �����
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCamera();
            shit();
        }
    }


    private void FixedUpdate()
    {
        // ����������� �������� ��������
        float time = Time.deltaTime;
        speedVector = _rb.velocity;
        accelerationVector = (speedVector - lastSpeed) / time;
        curPosition = curPosition + lastSpeed * time + (accelerationVector * time * time) / 2;
        lastSpeed = speedVector;

        // ������ ���������
        if (moveHeight != 0) flyPower += flyPowerStep * moveHeight;
        if (flyPower > 2) flyPower = 2;
        if (flyPower < 0) flyPower = 0;
        enginePower = baseEnginePower * flyPower;
        if (enginePower > maxEnginePower) enginePower = maxEnginePower;
        _rb.AddRelativeForce(Vector3.up * enginePower);

        // �������
        _rb.AddRelativeTorque(new Vector3(moveVertical, moveRotation, moveHorizontal) * rotSpeed);

        // ���������� ���� �����
        _rb.AddForce(windForce);

        // ���������� ��������������� �������� � ������ ��������� ��������
        Vector3 moveDirection = transform.forward * moveVertical + transform.right * moveHorizontal;
        _rb.AddForce(moveDirection * speedMultiplier, ForceMode.Acceleration);

        if (hoverMode) HoverMode();
        if (callibMode) CallibratingEngines();
    }

    // ������� ��� ��������� ����������� �����
    private void ChangeWindDirection()
    {
        windDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        windForce = windDirection * windStrength;
        Debug.Log("New Wind Direction: " + windDirection + " with force: " + windForce);
    }

    // ����� �������
    public void HoverMode()
    {
        // ��������� ������ ������������ ������� ������
        float heightError = hoverHeight - curPosition.y;
        float targetFlyPower = Mathf.Clamp01(1f + heightError); // ��������������� ������ ������

        // ����������� ���� ����� �� ������
        flyPower = Mathf.Lerp(flyPower, targetFlyPower, 0.1f);
        enginePower = baseEnginePower * flyPower;

        if (enginePower > maxEnginePower) enginePower = maxEnginePower;
        _rb.AddRelativeForce(Vector3.up * enginePower);

        // ����������� ������
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), 0.05f);
    }

    public void CallibratingEngines()
    {
        if (callibStage == 0)
        {
            if (Mathf.Abs(curPosition.y - callibHeight) < 0.1f)
            {
                callibEnginePowerTemp = baseEnginePower;
                callibStage = 1;
            }
            else
            {
                float heightError = callibHeight - curPosition.y;
                float targetFlyPower = Mathf.Clamp01(1f + heightError);
                flyPower = Mathf.Lerp(flyPower, targetFlyPower, 0.1f);
                enginePower = baseEnginePower * flyPower;

                if (enginePower > maxEnginePower) enginePower = maxEnginePower;
                _rb.AddRelativeForce(Vector3.up * enginePower);
            }
        }
        else if (callibStage == 1)
        {
            baseEnginePower = Mathf.Lerp(baseEnginePower, callibEnginePowerTemp, 0.01f);
            if (Mathf.Abs(baseEnginePower - callibEnginePowerTemp) < callibEnginePowerStep)
            {
                callibMode = false;
                Debug.Log("Calibration complete. Base engine power set to: " + baseEnginePower);
            }
        }
    }

    // ����� ��� ������� �������� (����� �� �������)
    public void SetMovement(float moveX, float moveY, float moveZ)
    {
        moveHorizontal = moveX;
        moveVertical = moveY;
        moveHeight = moveZ;
    }

    // ����� ��� ������������ ������ (����� �� �������)
    public void SwitchCamera()
    {
        cameras[currentCameraIndex].gameObject.SetActive(false);
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Count;
        cameras[currentCameraIndex].gameObject.SetActive(true);
    }

    public void shit()
    {
        Debug.Log("��� ����� ��������");
        Debug.Log("��� ����� ��������");
    }

    public float GetCurrentSpeed()
    {
        return speedVector.magnitude;
    }

    public float GetCurrentAltitude()
    {
        // ���������� ������� �������� ������ � ��������������� ������������
        return transform.position.y + Random.Range(-altitudeError, altitudeError);
    }

    public float GetCurrentAcceleration()
    {
        return (accelerationVector + new Vector3(
            Random.Range(-accelerationError, accelerationError),
            Random.Range(-accelerationError, accelerationError),
            Random.Range(-accelerationError, accelerationError)
        )).magnitude;
    }

    public float GetCurrentRollAngle()
    {
        return transform.eulerAngles.x + Random.Range(-angleError, angleError);
    }

    public float GetCurrentPitchAngle()
    {
        return transform.eulerAngles.z + Random.Range(-angleError, angleError);
    }

    public float GetCurrentYawAngle()
    {
        return transform.eulerAngles.y + Random.Range(-angleError, angleError);
    }

    public float GetCurrentDirection()
    {
        return transform.eulerAngles.y + Random.Range(-directionError, directionError);
    }

    public void SetWindStrength(float newWindStrength)
    {
        windStrength = newWindStrength;
        ChangeWindDirection(); // ������������ ����������� ����� � ����� �����
        Debug.Log($"Wind strength set to: {windStrength}");
    }

}
