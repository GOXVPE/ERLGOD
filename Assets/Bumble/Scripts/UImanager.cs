using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    public Slider powerSlider;
    public DroneFlyController drone;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        powerSlider.value = drone.flyPower;
    }
}
