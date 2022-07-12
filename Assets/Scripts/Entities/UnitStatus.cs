public struct UnitStatus {
    public int HP { get; set; }
    public int MaxHp { get; set; }
    public int Ult { get; set; }
    public int MaxUlt { get; set; }
    public int Range { get; set; }
    public int Defense { get; set; }
    public int Displacement { get; set; }
    public int Strength { get; set; }
    public int Ability { get; set; }


    public void Default(){
        MaxHp = 500;
        HP = 450;
        Ult = 4;
        MaxUlt = 5;
        Range = 1;
        Defense = 10;
        Displacement = 1;
        Strength = 25;
        Ability = 8;
    }
}
