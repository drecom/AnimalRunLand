using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Liver/Create ItemPattern", fileName = "ItemPatternInfo")]
public class ItemPatternInfo : ScriptableObject
{
    // 出現率をアニマルカーごとに上書き
    [System.Serializable]
    public class ExtraRate
    {
        public AnimalKind Kind;
        public int Rate;
    }


    [System.Serializable]
    public class Body
    {
        public TextAsset Pattern;
        public int Rate;
        public List<ExtraRate> ExtraRate;
    }
    public List<Body> Patterns;
}
