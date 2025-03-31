#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

public enum ConversionType
{
    items,
    Dialogs
}
[Serializable]
public class DialogRowData
{
    public int? id;                                         // int?�� Nullable<int>�� ��� ǥ���Դϴ�. ����Ǹ� Null ���� ���� �� �ִ� �������� �˴ϴ�.
    public string characterName;
    public string text;
    public int? nextId;
    public string protraitPath;
    public string choiceText;
    public int? choiceNextid;
}

public class JsonToScriptbleConverter : EditorWindow
{
    private string jsonFiilePath = "";                              //JSON ���� ��� ���ڿ� ��
    private string outputFolder = "Assets/ScriptbleObjects";      // ��� SO ������ ��� ��
    private bool createDatabase = true;                                 // ������ ���̽��� ��� �� �������� ���� bool ��
    private ConversionType conversionType = ConversionType.items;

    [MenuItem("Tools/JSON to Scriptble Objects")]
    public static void ShowWindows()
    {
        GetWindow<JsonToScriptbleConverter>("JSON to Scriptble Objects");
    }

    private void ConvertJsonToItemScriptableObjects()               // JSON ������ ScriptableObject ���Ϸ� ��ȯ �����ִ�  �Լ�
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

    // ��ȭ JSON�� ��ũ���ͺ� ������Ʈ�� ��ȯ
    private void ConvertJsonToDialogScriptableObjects()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        // JSON ���� �б�
        string jsonText = File.ReadAllText(jsonFiilePath);
        try
        {
            //JSON �Ľ�
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(jsonText);

            // ��ȭ ������ �籸��
            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> createDialogs = new List<DialogSO>();

            // 1�ܰ� : ��ȭ Ȯ�� ����
            foreach(var rowData in rowDataList)
            {
                // id �ִ� ���� ��ȭ�� ó��
                if (rowData.id.HasValue)
                {
                    DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();

                    // ������ ����
                    dialogSO.id = rowData.id.Value;
                    dialogSO.characterName = rowData.characterName;
                    dialogSO.text = rowData.text;
                    dialogSO.nextId = rowData.nextId.HasValue ? rowData.nextId.Value : -1;
                    dialogSO.protraitPath = rowData.protraitPath;
                    dialogSO.choices = new List<DialogChoiceSO>();
                    // �ʻ�ȭ �ε� (��ΰ� �ִ� ���)
                    if (!string.IsNullOrEmpty(rowData.protraitPath))
                    {
                        dialogSO.portrait = Resources.Load<Sprite>(rowData.protraitPath);

                        if(dialogSO.portrait == null)
                        {
                            Debug.LogWarning($"��ȭ {rowData.id}�� �ʻ�ȭ�� ã�� �� �����ϴ�.");
                        }
                    }
                    //dialogMap�� ����
                    dialogMap[dialogSO.id] = dialogSO;
                    createDialogs.Add(dialogSO);
                }
            }
            // 2�ܰ� : ������ Ȯ�� ó�� �D ����
           foreach(var rowData in rowDataList)
            {
                // id ��  ���� choiceText �� �ִ� ���� �������� ó��
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextid.HasValue)
                {
                    // ���� ���� ID�� �θ� ID�� ��� (���ӵǴ� �������� ���)
                    int parentId = -1;

                    // �� ������ �ٷ� ���� �մ� ��ȭ (Id�� �ִ� ���)�� ã��
                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentId = rowDataList[i].id.Value;
                            break;
                        }
                    }
                    // �θ� ID�� ã�� ���ްų� �θ� DI �� -1�� ��� (ù ��° �׸�)
                    if (parentId == -1)
                    {
                        Debug.LogWarning($"������ '{rowData.choiceText}'�� �θ� ��ȭ�� ã�� �� �����ϴ�.");
                    }

                    if (dialogMap.TryGetValue(parentId, out DialogSO parenIDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextid = rowData.choiceNextid.Value;

                        // ������ ���� ����
                        string choicassetPath = $"{outputFolder}Choice {parentId} {parenIDialog.choices.Count + 1}.asset";
                        AssetDatabase.CreateAsset(choiceSO, choicassetPath);
                        EditorUtility.SetDirty(choiceSO);

                        parenIDialog.choices.Add(choiceSO);
                    }
                    else
                    {
                        Debug.LogWarning($"������ '{rowData.choiceText}'�� ������ ��ȭ (id : {parentId})�� ã�� �� �����ϴ�.");
                    }
                }
            }

            // 3�ܰ� : ��ȭ ��ũ���ͺ� ������Ʈ ����
            foreach (var dialog in createDialogs)
            {
                // ��ũ���ͺ� ������Ʈ ���� - ID�� 4�ڸ� ������ ������
                string assetPath = $"{outputFolder}/Dialog_{dialog.id.ToString("D4")}.asset";
                AssetDatabase.CreateAsset(dialog, assetPath);

                // ���� �̸� ����
                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";

                EditorUtility.SetDirty(dialog);
            }

            // ������ ���̽� ����
            if(createDatabase && createDialogs.Count > 0)
            {
                DialogDatabaseSO database = ScriptableObject.CreateInstance<DialogDatabaseSO>();
                database.dialogs = createDialogs;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/DialogDatabase.asset");
                EditorUtility.SetDirty(database);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Creatd {createDialogs.Count} dialog scriptable objects!", "OK");
        }
        catch(System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to convert JSON: {e.Message}", "OK");
            Debug.LogError($"JSON ��ȯ ���� : {e}");
        }
    }

     void OnGUI()
    {
        GUILayout.Label("JSON to Scriptable Obejct Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // ��ȯ Ÿ�� ����
        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type:", conversionType);

        // Ÿ�Կ� ���� �⺻ ��� ���� ����
        if (conversionType == ConversionType.items && outputFolder == "Asset/ScriptableObjects")
        {
            outputFolder = "Assets/ScriptableObjects/Items";
        }
        else if(conversionType == ConversionType.Dialogs && outputFolder == "assets/ScriptableObjects")
        {
            outputFolder = "Assets/ScriptableObjects/Dialogs";
        }

        if (GUILayout.Button("Select JSON File"))
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

                switch (conversionType)
                {
                    case ConversionType.items:
                        ConvertJsonToItemScriptableObjects();
                        break;

                    case ConversionType.Dialogs:
                        ConvertJsonToDialogScriptableObjects();
                        break;

                }
                
            }
        }
    }

}

#endif