public struct UnitStatus {
    public int HP { get; set; }
    public int MaxHp { get; set; }
    public int Ult { get; set; }
    public int MaxUlt { get; set; }
    public int Range { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
    public int Strength { get; set; }


    public void Default(){
        HP = 450;
        MaxHp = 500;
        Ult = 4;
        MaxUlt = 5;
        Range = 1;
        Defense = 5;
        Speed = 2;
        Strength = 75;
    }

     public void SetValues(UnitStatus status){
        MaxHp = status.HP;
        HP = status.HP;
        Ult = status.Ult;
        MaxUlt = status.MaxUlt;
        Range = status.Range;
        Defense = status.Defense;
        Speed = status.Speed;
        Strength = status.Strength;
    }
}
