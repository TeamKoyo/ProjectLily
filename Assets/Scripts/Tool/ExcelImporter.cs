using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ExcelImporter : EditorWindow
{
    private string excelFolderPath = "Assets/Excels";
    private string savePath = "Assets/ScriptableObjects/Database.asset";

    [MenuItem("Tools/Excel → ScriptableObject(DB)")]
    public static void ShowWindow()
    {
        GetWindow<ExcelImporter>("Excel Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Excel Import Settings", EditorStyles.boldLabel);
        excelFolderPath = EditorGUILayout.TextField("Excel Folder", excelFolderPath);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if (GUILayout.Button("Import All Excel Files"))
        {
            ImportAll();
        }
    }

    private void ImportAll()
    {
        SODatabase db = AssetDatabase.LoadAssetAtPath<SODatabase>(savePath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<SODatabase>();
            AssetDatabase.CreateAsset(db, savePath);
        }

        string[] excelFiles = Directory.GetFiles(excelFolderPath, "*.xlsx");
        string mappingPath = "Assets/Excels/ExcelMapping.json";
        ExcelMapping mapping = JsonUtility.FromJson<ExcelMapping>(File.ReadAllText(mappingPath));

        foreach (string file in excelFiles)
        {
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                string fileName = Path.GetFileName(file);
                string dtoName = mapping.GetDTO(fileName);

                if (dtoName == nameof(CardData))
                    db.cards = ParseTable<CardData>(reader);
                else if (dtoName == nameof(CharData))
                    db.chars = ParseTable<CharData>(reader);
                if (dtoName == nameof(MonsterData))
                    db.monsters = ParseTable<MonsterData>(reader);
                else if (dtoName == nameof(MonsterSequence))
                    db.sequences = ParseTable<MonsterSequence>(reader);
            }
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log("DB화 성공");
    }

    private List<T> ParseTable<T>(IExcelDataReader reader) where T : new()
    {
        List<T> list = new List<T>();
        FieldInfo[] fields = typeof(T).GetFields();

        reader.Read(); // 헤더 스킵

        while (reader.Read())
        {
            if(reader.GetValue(0) == null) // id 없으면 더이상 읽지 않음
            {
                break;
            }

            T obj = new T();

            int colIndex = 0;
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(List<int>))
                {
                    List<int> values = new List<int>();

                    for (int i = 0; i < 10; i++)
                    {
                        object val = reader.GetValue(colIndex++);
                        if (val != null)
                        {
                            values.Add(Convert.ToInt32(val));
                        }
                    }

                    field.SetValue(obj, values);
                }
                else if (field.FieldType == typeof(List<string>))
                {
                    List<string> values = new List<string>();

                    for (int i = 0; i < 10; i++)
                    {
                        object val = reader.GetValue(colIndex++);
                        if (val != null)
                        {
                            values.Add(val.ToString());
                        }
                    }

                    field.SetValue(obj, values);
                }
                else // 단일 값
                {
                    object val = reader.GetValue(colIndex++);
                    if (val != null)
                    {
                        object converted = Convert.ChangeType(val, field.FieldType);
                        field.SetValue(obj, converted);
                    }
                }
            }

            list.Add(obj);
        }

        return list;
    }
}

[System.Serializable]
public class ExcelMapping
{
    public List<ExcelMap> maps;

    public string GetDTO(string fileName)
    {
        ExcelMap map = maps.Find(x => x.fileName == fileName);
        return map != null ? map.dtoName : null;
    }
}

[System.Serializable]
public class ExcelMap
{
    public string fileName; // "Char.xlsx"
    public string dtoName;  // "CharData"
}