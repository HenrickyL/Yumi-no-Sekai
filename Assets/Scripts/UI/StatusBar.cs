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

    public void SetStatus(UnitStatus status){
        HPSlider.maxValue = status.MaxHp;
        HPSlider.value = status.HP;
        ULTSlider.maxValue = status.MaxUlt;
        ULTSlider.value = status.Ult;
        Refresh();
    }
    public void SetUlt(int value){
        if(value>0){
            ULTSlider.value = value;
            Refresh();
        }
        
    }
    public void SetMaxHealth(int value){
        HPSlider.maxValue = value;
        HPSlider.value = value;
        Refresh();
    }
    public void SetMaxUlt(int value){
        ULTSlider.maxValue = value;
        ULTSlider.value = value;
        Refresh();
    }

    private void UpdateHP(){
        hpText.text =  string.Format("{0}/{1}",HPSlider.value,HPSlider.maxValue);
    } 

    void Refresh(){
        UpdateHP();
    }
}
