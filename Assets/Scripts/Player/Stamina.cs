using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    public Slider slider;

    public void SetStamina(int stamina)
    {
        slider.value = stamina;
    }

    public void SetMaxStamina(int stamina)
    {
        slider.maxValue = stamina;
        this.SetStamina(stamina);

    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
