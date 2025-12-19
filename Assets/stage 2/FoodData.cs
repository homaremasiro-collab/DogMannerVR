public enum FoodResult
{
    Good,
    Bad,
    Danger,
    Conditional
}

[System.Serializable]
public class FoodData
{
    public string foodName;
    public FoodResult result;
    public int affectionChange;
    public string description;
}
