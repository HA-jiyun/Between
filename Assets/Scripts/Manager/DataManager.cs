using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    private Dictionary<string, UnitData> units = new Dictionary<string, UnitData>();

    private void Awake()
    {
        instance = this;
        LoadAllUnitData();
    }

    void LoadAllUnitData()
    {
        UnitData[] assets = Resources.LoadAll<UnitData>("Data/Units");
        foreach (var asset in assets)
        {
            if(!units.ContainsKey(asset.code))
                units.Add(asset.code, asset);
        }
    }

    public UnitData GetUnitData(string code)
    {
        if(units.TryGetValue(code, out UnitData data))
            return data;
        return null;
    }
}
