using UnityEngine;

public enum UnitElement { Error, Fire, Water, Wind, Light, Dark }

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject
{
    public string code;
    public string myName;
    public UnitElement element;
    public int hp;
    public int basicDamage;
    public int specialDamage;
    public int dis;
    public int moveRange;
}
