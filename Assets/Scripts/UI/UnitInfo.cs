using UnityEngine;
using UnityEngine.UI;

public class UnitInfo : MonoBehaviour
{
    public Slider HPSlider;
    public Text Hp;
    public Text Straigth;
    public Text Defense;
    public Text Speed;
    public Text Range;
	public RawImage perfil;


    public void SetUnit( HexUnit unit){
        HPSlider.value = unit.HP;
        HPSlider.maxValue = unit.MaxHp;
        Hp.text = $"{unit.HP}";
        Speed.text = $"{unit.Speed}";
        Range.text = $"{unit.Range}";
        Straigth.text = $"{unit.Strength}";
        Defense.text = $"{unit.Defense}";
        perfil.texture = unit.perfil;
    }

}
