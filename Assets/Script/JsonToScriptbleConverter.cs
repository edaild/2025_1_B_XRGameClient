#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class JsonToScriptbleConverter : EditorWindow
{
    private string jsonFiilePath = "";                              //JSON ���� ��� ���ڿ� ��
    private string outputFolder = "Assets/ScriptbleObjects/items";      // ��� SO ������ ��� ��
    private bool createDatabase = true;                                 // ������ ���̽��� ��� �� �������� ���� bool ��

    [MenuItem("Tools/JSON to Scriptble Objects")]
    public static void ShowWindows()
    {
        GetWindow<JsonToScriptbleConverter>("JSON to Scriptble Objects");
    }

    private void ConvertJsonToScriptableObjects()               // JSON ������ ScriptableObject ���Ϸ� ��ȯ �����ִ�  �Լ�
    {
        // Ǯ�� ����
        if (!Directory.Exists(outputFolder))                    // ���� ��ġ�� Ȯ���ϰ� ������ ���� �Ѵ�.
        {
            Directory.CreateDirectory(outputFolder);
        }

        // JSON ���� ��
        string jsonText = File.ReadAllText(jsonFiilePath);      // JSON ������ �д´�.

        try
        {
            //JSON �Ľ�
            List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createITems = new List<ItemSO>();   // ItemSO ����Ʈ ����
            
            // �� ������ �����͸� ��ũ���ͺ� ������Ʈ�� ��ȯ
            foreach(var itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();

                // ������ ����
                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                // ������ ��ȯ
                if(System.Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.Log($"������ '{itemData.itemName}'�� ��ȿ���� ���� Ÿ�� : {itemData.itemTypeString}");
                }

                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;

               // ������ �ε� (��ΰ� �ִ� ���)
                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}.png");

                    if (itemSO.icon == null)
                    {
                        Debug.Log($"������ '{itemData.nameEng}'�� �������� ã�� �� �����ϴ�. : {itemData.iconPath}");
                    }
                }
                // ��ũ���ͺ� ������Ʈ ���� - ID�� �����ϴ� 4�ڸ� ���ڷ� ������
                string assetPath = $"{outputFolder}/item_{itemData.id.ToString("D4")}_{itemData.nameEng}asset";
                AssetDatabase.CreateAsset(itemSO, assetPath);

                // ���� �̿� ����
                itemSO.name = $"item {itemData.id.ToString("04")}+{itemData.nameEng}";
                createITems.Add(itemSO);

                EditorUtility.SetDirty(itemSO);
            }

            // �����ͺ��̽� ����
            if (createDatabase && createITems.Count > 0)
            {
                ItemDatabaseSO database = ScriptableObject.CreateInstance<ItemDatabaseSO>();      //ItemDatabaseSO ����
                    database.items = createITems;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/ItemDatabase.asset");
                EditorUtility.SetDirty (database);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Sucess", $"Created {createITems.Count} scriptble objects!:", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error" ,$"Failed to Covert JSON : { e.Message}" ,"OK");
            Debug.LogError($"JSON ��ȯ ���� : {e}");
        }
    }
     void OnGUI()
    {
        GUILayout.Label("JSON to Scriptable Obejct Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if(GUILayout.Button("Select JSON File"))
        {
            jsonFiilePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
        }

        EditorGUILayout.LabelField("Selected File : ", jsonFiilePath);
        EditorGUILayout.Space();
        outputFolder= EditorGUILayout.TextField("Output Folder :" , outputFolder);
        createDatabase = EditorGUILayout.Toggle("Create Databaes Asset", createDatabase);
        EditorGUILayout.Space();
        if (GUILayout.Button("Convert to Scriptalbe Objects"))
        {
            if(string.IsNullOrEmpty("convert to Scriptable Objects"))
            {
                if (string.IsNullOrEmpty(jsonFiilePath))
                {
                    EditorUtility.DisplayDialog("Error", "Please select a JSON file firest!", "OK");
                    return;
                }
                ConvertJsonToScriptableObjects();
            }
        }
    }

}

#endif