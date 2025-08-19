using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class CSVImporter : EditorWindow
{
    private UnityEngine.Object excelFile;
    private string outputPath = "Assets/ScriptableObjects";
    private DataSet excelDataSet;
    private List<string> sheetNames = new List<string>();
    private SODatabase data;

    [MenuItem("Tools/Excel → ScriptableObject 변환기")]
    public static void ShowWindow()
    {
        GetWindow<CSVImporter>("Excel Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Excel to ScriptableObject", EditorStyles.boldLabel);
        UnityEngine.Object newExcelFile = EditorGUILayout.ObjectField("Excel File", excelFile, typeof(UnityEngine.Object), false);
        outputPath = EditorGUILayout.TextField("Output Folder", outputPath);

        data = AssetDatabase.LoadAssetAtPath<SODatabase>(Path.Combine(outputPath, excelFile.name + ".asset"));

        if (newExcelFile != excelFile)
        {
            excelFile = newExcelFile;
            LoadSheet();
        }

        if (data == null)
        {
            if(GUILayout.Button("Create ScriptableObject"))
            {
                CreateDataAsset();
            }
        }
        else
        {
            GUILayout.Space(10);
            GUILayout.Label("Import Individual Sheets", EditorStyles.boldLabel);

            foreach (string sheet in sheetNames)
            {
                if (GUILayout.Button($"Import {sheet}"))
                {
                    ImportSheet(sheet);
                }
            }
        }
    }

    private void LoadSheet()
    {
        sheetNames.Clear();

        string assetPath = AssetDatabase.GetAssetPath(excelFile);
        string path = Path.Combine(Application.dataPath.Replace("Assets", ""), assetPath);

        if (!File.Exists(path))
        {
            Debug.LogError("Excel 파일 경로가 잘못되었습니다.");
            return;
        }

        using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        excelDataSet = reader.AsDataSet();

        foreach (DataTable table in excelDataSet.Tables)
        {
            sheetNames.Add(table.TableName);
        }
    }

    private void CreateDataAsset()
    {
        data = ScriptableObject.CreateInstance<SODatabase>();

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        string savePath = Path.Combine(outputPath, excelFile.name + ".asset");
        AssetDatabase.CreateAsset(data, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("ScriptableObject 생성");
    }

    private void ImportSheet(string sheetName)
    {
        if (excelDataSet == null || data == null)
        {
            Debug.LogError("Excel / ScriptableObject 필요");
            return;
        }

        string key = sheetName.ToLowerInvariant();
        DataTable table = excelDataSet.Tables[sheetName];
        RegistDic(table);

        if (importDic.TryGetValue(key, out var action))
        {
            action(table, data);
        }
        else
        {
            Debug.LogWarning($"'{sheetName}' 시트는 아직 로직이 정의되지 않았습니다.");
        }

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        Debug.Log($"{sheetName} 갱신");
    }

    private Dictionary<string, Action<DataTable, SODatabase>> importDic;

    private void RegistDic(DataTable table)
    {
        importDic = new(StringComparer.OrdinalIgnoreCase)
        {
            { sheetNames[0], (table, data) => ImportData<CardData>(table, ref data.cards) }
        };
    }

    private void ImportData<T>(DataTable table, ref List<T> list) where T : new()
    {
        list = new List<T>();
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

        for (int i = 3; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            T instance = new T();

            for (int col = 0; col < table.Columns.Count && col < fields.Length; col++)
            {
                try
                {
                    var field = fields[col];
                    object raw = row[col];

                    if (raw == null) continue;

                    object value = Convert.ChangeType(raw, field.FieldType);
                    field.SetValue(instance, value);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ImportLookUp] {typeof(T).Name} 필드 {col} 변환 실패: {e.Message}");
                }
            }

            list.Add(instance);
        }
    }
}