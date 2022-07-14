using UnityEngine;
using UnityEngine.UI;


public class StatusBar : MonoBehaviour
{
    public Slider HPSlider;
    public Text hpText;

    public void SetStatus(UnitStatus status){
        HPSlider.value = status.HP;
        HPSlider.maxValue = status.MaxHp;
        Refresh();
    }
  
    public void SetHP(int value){
        if(value>0){
            HPSlider.value = value;
            Refresh();
        }
    }
    public void SetMaxHealth(int value){
        HPSlider.maxValue = value;
        HPSlider.value = value;
        Refresh();
    }
   

    private void UpdateHP(){
        hpText.text =  string.Format("{0}/{1}",HPSlider.value,HPSlider.maxValue);
    } 
    
    void Refresh(){
        UpdateHP();
    }
}
