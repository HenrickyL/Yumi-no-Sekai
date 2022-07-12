using UnityEngine;
using UnityEngine.UI;


public class StatusBar : MonoBehaviour
{
    public Slider HPSlider;
    public Slider ULTSlider;
    public Text hpText;

    public void SetHealth(int health){
        if(health>0){
            HPSlider.value = health;
            UpdateHP();
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
        UpdateHP();
    }
    public void SetMaxUlt(int value){
        ULTSlider.maxValue = value;
        ULTSlider.value = value;
    }

    private void UpdateHP(){
        hpText.text =  string.Format("{0}/{1}",HPSlider.value,HPSlider.maxValue);
    } 
}
