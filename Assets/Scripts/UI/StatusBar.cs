using UnityEngine;
using UnityEngine.UI;


public class StatusBar : MonoBehaviour
{
    public Slider HPSlider;
    public Slider ULTSlider;

    public void SetHealth(int health){
        if(health>0){
            HPSlider.value = health;
        }
    }
    public void SetUlt(int value){
        if(value>0){
            ULTSlider.value = value;
        }
    }
    public void SetMaxHealth(int value){
        HPSlider.maxValue = value;
        HPSlider.value = value;
    }
    public void SetMaxUlt(int value){
        ULTSlider.maxValue = value;
        ULTSlider.value = value;
    }
}
