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
    private float hoverHeightRange = 1; // Диапазон высоты висения
    private float hoverMaxPower = 1.3f; // Пределы мощности для контроля висения
    private float hoverMinPower = 0.7f;

    // Органы управления
    private float moveHorizontal = 0;
    private float moveVertical = 0;
    private float moveHeight = 0;
    private float moveRotation = 0;

    // Датчики ориентации
    [SerializeField]
    public Vector3 speedVector = Vector3.zero;
    private Vector3 thrustVector = Vector3.zero;
    public Vector3 accelerationVector = Vector3.zero;
    private Vector3 lastSpeed = Vector3.zero;
    private Vector3 startPointPosition; // Позиция относительно которой все высчитывается 
    private Vector3 curPosition;

    [SerializeField]
    private float baseEnginePower; // Тяга при которой обеспечивается висение
    [SerializeField]
    public float flyPower; // Ручка регулировки тяги (1=висение) 
    private float flyPowerStep = 0.05f; // Шаг ручки
    [SerializeField]
    private float enginePower; // Итоговая тяга
    [SerializeField]
    private float maxEnginePower = 150; // Максимальная мощность двигателя, теперь редактируется в инспекторе
    [SerializeField]
    private float rotSpeed = 3; // Скорость наклонов, теперь редактируется в инспекторе
    [SerializeField]
    private float speedMultiplier = 1.0f; // Множитель скорости для увеличения скорости дрона

    // Переменные для калибровки силы двигателя
    private int callibStage = 0;
    private bool callibMode = false;
    private float callibHeight = 1.5f;
    private float callibEnginePowerTemp;
    private float callibEnginePowerStep = 0.01f;

    // Датчики и погрешности
    public Text скоростьТекст;
    public Text высотаТекст;
    public Text ускорениеТекст;
    public Text уголRollТекст;  // Текстовое поле для Roll угла
    public Text уголPitchТекст; // Текстовое поле для Pitch угла
    public Text уголYawТекст;   // Текстовое поле для Yaw угла
    public Text направлениеТекст;

    public float altitude;
    private float altitudeError;
    private float accelerationError;
    private float angleError;
    private float directionError;

    // Симуляция ветра
    [SerializeField]
    private Vector3 windForce = Vector3.zero; // Сила ветра
    private Vector3 windDirection = Vector3.zero; // Направление ветра
    [SerializeField]
    private float windChangeInterval = 2.0f; // Интервал изменения направления ветра (в секундах), теперь редактируется в инспекторе
    [SerializeField]
    private float windStrength = 15.0f; // Сила ветра, теперь редактируется в инспекторе
    private float windChangeTimer = 0.0f; // Таймер для изменения направления ветра

    public Vector3 CurrentSpeed
    {
        get { return speedVector; }
    }



    // Переключение камер
    [SerializeField]
    private List<Camera> cameras; // Список камер, которые будут переключаться
    private int currentCameraIndex = 0;

    private void Start()
    {
        startPointPosition = transform.position;
        curPosition = Vector3.zero;

        altitudeError = 0.05f; // Пример погрешности для высотомера
        accelerationError = 0.02f; // Пример погрешности для акселерометра
        angleError = 1f; // Пример погрешности для угла
        directionError = 2f; // Пример погрешности для направления севера

        // Начальные значения для ветра
        ChangeWindDirection();

        // Убедимся, что только одна камера активна при старте
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].gameObject.SetActive(i == currentCameraIndex);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) // Режим висения
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

        // Задание новой высоты висения
        if (moveHeight != 0) hoverHeight = curPosition.y;

        // Обновление значений датчиков
        altitude = curPosition.y + Random.Range(-altitudeError, altitudeError);
        Vector3 accelerationWithError = accelerationVector + new Vector3(Random.Range(-accelerationError, accelerationError), Random.Range(-accelerationError, accelerationError), Random.Range(-accelerationError, accelerationError));
        float rollAngle = transform.eulerAngles.x + Random.Range(-angleError, angleError);
        float pitchAngle = transform.eulerAngles.z + Random.Range(-angleError, angleError);
        float yawAngle = transform.eulerAngles.y + Random.Range(-angleError, angleError);
        float direction = transform.eulerAngles.y + Random.Range(-directionError, directionError);

        скоростьТекст.text = "Скорость: " + speedVector.magnitude.ToString("F2");
        высотаТекст.text = "Высота: " + altitude.ToString("F2") + " м";
        ускорениеТекст.text = "Ускорение: " + accelerationWithError.ToString("F2");
        уголRollТекст.text = "Roll: " + rollAngle.ToString("F2") + " градусов";
        уголPitchТекст.text = "Pitch: " + pitchAngle.ToString("F2") + " градусов";
        уголYawТекст.text = "Yaw: " + yawAngle.ToString("F2") + " градусов";
        направлениеТекст.text = "Направление: " + direction.ToString("F2") + " градусов";

        // Обновление направления ветра через определенные интервалы времени
        windChangeTimer += Time.deltaTime;
        if (windChangeTimer > windChangeInterval)
        {
            windChangeTimer = 0.0f;
            ChangeWindDirection();
        }

        // Переключение камер
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCamera();
            shit();
        }
    }


    private void FixedUpdate()
    {
        // Вычисляются значения векторов
        float time = Time.deltaTime;
        speedVector = _rb.velocity;
        accelerationVector = (speedVector - lastSpeed) / time;
        curPosition = curPosition + lastSpeed * time + (accelerationVector * time * time) / 2;
        lastSpeed = speedVector;

        // Работа двигателя
        if (moveHeight != 0) flyPower += flyPowerStep * moveHeight;
        if (flyPower > 2) flyPower = 2;
        if (flyPower < 0) flyPower = 0;
        enginePower = baseEnginePower * flyPower;
        if (enginePower > maxEnginePower) enginePower = maxEnginePower;
        _rb.AddRelativeForce(Vector3.up * enginePower);

        // Наклоны
        _rb.AddRelativeTorque(new Vector3(moveVertical, moveRotation, moveHorizontal) * rotSpeed);

        // Применение силы ветра
        _rb.AddForce(windForce);

        // Применение горизонтального движения с учетом множителя скорости
        Vector3 moveDirection = transform.forward * moveVertical + transform.right * moveHorizontal;
        _rb.AddForce(moveDirection * speedMultiplier, ForceMode.Acceleration);

        if (hoverMode) HoverMode();
        if (callibMode) CallibratingEngines();
    }

    // Функция для изменения направления ветра
    private void ChangeWindDirection()
    {
        windDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        windForce = windDirection * windStrength;
        Debug.Log("New Wind Direction: " + windDirection + " with force: " + windForce);
    }

    // Режим висения
    public void HoverMode()
    {
        // Двигатель должен поддерживать текущую высоту
        float heightError = hoverHeight - curPosition.y;
        float targetFlyPower = Mathf.Clamp01(1f + heightError); // Пропорционально ошибке высоты

        // Выравниваем тягу дрона по высоте
        flyPower = Mathf.Lerp(flyPower, targetFlyPower, 0.1f);
        enginePower = baseEnginePower * flyPower;

        if (enginePower > maxEnginePower) enginePower = maxEnginePower;
        _rb.AddRelativeForce(Vector3.up * enginePower);

        // Выравниваем наклон
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

    // Метод для задания движения (вызов из сервера)
    public void SetMovement(float moveX, float moveY, float moveZ)
    {
        moveHorizontal = moveX;
        moveVertical = moveY;
        moveHeight = moveZ;
    }

    // Метод для переключения камеры (вызов из сервера)
    public void SwitchCamera()
    {
        cameras[currentCameraIndex].gameObject.SetActive(false);
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Count;
        cameras[currentCameraIndex].gameObject.SetActive(true);
    }

    public void shit()
    {
        Debug.Log("Это говно работает");
        Debug.Log("Это говно работает");
    }

    public float GetCurrentSpeed()
    {
        return speedVector.magnitude;
    }

    public float GetCurrentAltitude()
    {
        // Возвращаем текущее значение высоты с накапливающейся погрешностью
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
        ChangeWindDirection(); // Пересчитайте направление ветра с новой силой
        Debug.Log($"Wind strength set to: {windStrength}");
    }

}
