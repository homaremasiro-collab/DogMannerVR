using UnityEngine;

public enum FoodType
{
    Egg = 0,
    Fish = 1,
    Grape = 2,
}

public class FoodData : MonoBehaviour
{
    public FoodType type;
    public bool isSafe; // 犬にOKならtrue、ダメならfalse
}
