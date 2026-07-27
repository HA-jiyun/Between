using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class SpreadSheetSync : EditorWindow
{
    public const int DATA_COLS = 7;

    private static UnityWebRequest www;
    private string url = "https://docs.google.com/spreadsheets/d/1tOZqilhiegaGUDo1oEZSoW1gyN2ZRSg7U6BkOZpEvFM/export?format=csv";

    [MenuItem("Tools/Sync Unit Data")]
    public static void ShowWindow() => GetWindow<SpreadSheetSync>("Data Sync");

    private void OnGUI()
    {
        if (GUILayout.Button("Update All Data"))
        {
            StartDownload();
        }
    }

    void StartDownload()
    {
        if (www != null) return;

        www = UnityWebRequest.Get(url);
        www.SendWebRequest();

        EditorApplication.update += CheckProgress;
    }

    private void CheckProgress()
    {
        if (www == null || !www.isDone) return;

        EditorApplication.update -= CheckProgress;

        using (www)
        {
            if (www.result == UnityWebRequest.Result.Success)
                ProcessCSV(www.downloadHandler.text);
            else
                Debug.LogError("Sync error: " + www.error);
        }
        www = null;
    }

    void ProcessCSV(string csvText)
    {
        string[] rows = csvText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < rows.Length; i++)
        {
            string[] columns = rows[i].Split(',');
            if (columns.Length < DATA_COLS) continue;

            string unitCode = columns[0].Trim();
            SaveUnitAsset(unitCode, columns);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Sync Completed!");
    }

    void SaveUnitAsset(string entityCode, string[] data)
    {
        string path = $"Assets/Resources/Data/Units/{entityCode}.asset";
        UnitData asset = AssetDatabase.LoadAssetAtPath<UnitData>(path);
        if (asset == null) 
        {
            asset = CreateInstance<UnitData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.code = entityCode;
        asset.myName = data[1].Trim();
        if (Enum.TryParse(data[2].Trim(), out UnitElement result))
            asset.element = result;
        else
        {
            asset.element = UnitElement.Error;
            Debug.LogWarning($"{data[2]}는 유효한 속성이 아닙니다!");
        }
        asset.hp = int.Parse(data[3].Trim());
        asset.basicDamage = int.Parse(data[4].Trim());
        asset.specialDamage = int.Parse(data[5].Trim());
        asset.dis = int.Parse(data[6].Trim());
        asset.moveRange = int.Parse(data[7].Trim());

        EditorUtility.SetDirty(asset);
    }

}
