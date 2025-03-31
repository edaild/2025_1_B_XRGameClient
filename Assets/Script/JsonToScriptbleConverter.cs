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
    public int? id;                                         // int?는 Nullable<int>의 축약 표현입니다. 선언되면 Null 값도 가질 수 있는 정수형이 됩니다.
    public string characterName;
    public string text;
    public int? nextId;
    public string protraitPath;
    public string choiceText;
    public int? choiceNextid;
}

public class JsonToScriptbleConverter : EditorWindow
{
    private string jsonFiilePath = "";                              //JSON 파읽 경로 문자열 값
    private string outputFolder = "Assets/ScriptbleObjects";      // 출력 SO 파일을 경로 값
    private bool createDatabase = true;                                 // 데이터 베이스를 사용 할 것인지에 대한 bool 값
    private ConversionType conversionType = ConversionType.items;

    [MenuItem("Tools/JSON to Scriptble Objects")]
    public static void ShowWindows()
    {
        GetWindow<JsonToScriptbleConverter>("JSON to Scriptble Objects");
    }

    private void ConvertJsonToItemScriptableObjects()               // JSON 파일을 ScriptableObject 파일로 변환 시켜주는  함수
    {
        // 풀더 생성
        if (!Directory.Exists(outputFolder))                    // 폴더 위치를 확인하고 없으면 생성 한다.
        {
            Directory.CreateDirectory(outputFolder);
        }

        // JSON 파일 읽
        string jsonText = File.ReadAllText(jsonFiilePath);      // JSON 파일을 읽는다.

        try
        {
            //JSON 파싱
            List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createITems = new List<ItemSO>();   // ItemSO 리스트 생성
            
            // 각 아이템 데이터를 스크립터블 오브젝트로 변환
            foreach(var itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();

                // 데이터 복사
                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                // 열거형 변환
                if(System.Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.Log($"아이템 '{itemData.itemName}'의 유효하지 않은 타입 : {itemData.itemTypeString}");
                }

                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;

               // 아이콘 로드 (경로가 있는 경우)
                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}.png");

                    if (itemSO.icon == null)
                    {
                        Debug.Log($"아이템 '{itemData.nameEng}'의 아이콘을 찾을 수 없습니다. : {itemData.iconPath}");
                    }
                }
                // 스크립터블 오브젝트 저장 - ID를 저장하는 4자르 숫자로 포맷팀
                string assetPath = $"{outputFolder}/item_{itemData.id.ToString("D4")}_{itemData.nameEng}asset";
                AssetDatabase.CreateAsset(itemSO, assetPath);

                // 에셋 이용 지정
                itemSO.name = $"item {itemData.id.ToString("04")}+{itemData.nameEng}";
                createITems.Add(itemSO);

                EditorUtility.SetDirty(itemSO);
            }

            // 데이터베이스 생성
            if (createDatabase && createITems.Count > 0)
            {
                ItemDatabaseSO database = ScriptableObject.CreateInstance<ItemDatabaseSO>();      //ItemDatabaseSO 생성
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
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }

    // 대화 JSON을 스크립터블 오브젝트로 변환
    private void ConvertJsonToDialogScriptableObjects()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        // JSON 파일 읽기
        string jsonText = File.ReadAllText(jsonFiilePath);
        try
        {
            //JSON 파싱
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(jsonText);

            // 대화 데이터 재구성
            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> createDialogs = new List<DialogSO>();

            // 1단계 : 대화 확목 생성
            foreach(var rowData in rowDataList)
            {
                // id 있는 행은 대화로 처리
                if (rowData.id.HasValue)
                {
                    DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();

                    // 데이터 복사
                    dialogSO.id = rowData.id.Value;
                    dialogSO.characterName = rowData.characterName;
                    dialogSO.text = rowData.text;
                    dialogSO.nextId = rowData.nextId.HasValue ? rowData.nextId.Value : -1;
                    dialogSO.protraitPath = rowData.protraitPath;
                    dialogSO.choices = new List<DialogChoiceSO>();
                    // 초상화 로드 (경로가 있는 경우)
                    if (!string.IsNullOrEmpty(rowData.protraitPath))
                    {
                        dialogSO.portrait = Resources.Load<Sprite>(rowData.protraitPath);

                        if(dialogSO.portrait == null)
                        {
                            Debug.LogWarning($"대화 {rowData.id}의 초상화를 찾을 수 없습니다.");
                        }
                    }
                    //dialogMap에 ㅊ가
                    dialogMap[dialogSO.id] = dialogSO;
                    createDialogs.Add(dialogSO);
                }
            }
            // 2단계 : 선택지 확목 처리 멏 연결
           foreach(var rowData in rowDataList)
            {
                // id 가  없고 choiceText 가 있는 행은 선택지로 처리
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextid.HasValue)
                {
                    // 이전 행의 ID를 부모 ID로 사용 (연속되는 선택지의 경우)
                    int parentId = -1;

                    // 이 선택지 바로 위에 잇는 대화 (Id가 있는 방목)를 찾음
                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentId = rowDataList[i].id.Value;
                            break;
                        }
                    }
                    // 부모 ID를 찾지 못햇거나 부모 DI 가 -1인 경우 (첫 번째 항목)
                    if (parentId == -1)
                    {
                        Debug.LogWarning($"선택지 '{rowData.choiceText}'의 부모 대화를 찾을 수 없습니다.");
                    }

                    if (dialogMap.TryGetValue(parentId, out DialogSO parenIDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextid = rowData.choiceNextid.Value;

                        // 선택지 에셋 지정
                        string choicassetPath = $"{outputFolder}Choice {parentId} {parenIDialog.choices.Count + 1}.asset";
                        AssetDatabase.CreateAsset(choiceSO, choicassetPath);
                        EditorUtility.SetDirty(choiceSO);

                        parenIDialog.choices.Add(choiceSO);
                    }
                    else
                    {
                        Debug.LogWarning($"선택지 '{rowData.choiceText}'를 연결할 대화 (id : {parentId})를 찾을 수 없습니다.");
                    }
                }
            }

            // 3단계 : 대화 스크립터블 오브젝트 저장
            foreach (var dialog in createDialogs)
            {
                // 스크립터블 오브젝트 저장 - ID를 4자리 숫저로 포맷팅
                string assetPath = $"{outputFolder}/Dialog_{dialog.id.ToString("D4")}.asset";
                AssetDatabase.CreateAsset(dialog, assetPath);

                // 에셋 이름 지정
                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";

                EditorUtility.SetDirty(dialog);
            }

            // 데이터 베이스 생성
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
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }

     void OnGUI()
    {
        GUILayout.Label("JSON to Scriptable Obejct Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 변환 타입 선택
        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type:", conversionType);

        // 타입에 따라 기본 출력 폴더 설정
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