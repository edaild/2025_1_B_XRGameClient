using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;

public class ItemDataLoader : MonoBehaviour
{
    [SerializeField]
    private string jsonFileName = "items";          // Resources 폴더에서 가져올 JSON 파일 이름

    private List<ItemData> itemList;
    
    // Start is called before the first frame update
    void Start()
    {
        LoadItemData();
    }

    void LoadItemData()
    {
        TextAsset jsonFlie = Resources.Load<TextAsset>(jsonFileName);           //textAsset 형태로 Json 파일을 로딩한다.
        if(jsonFlie != null)
        {
            byte[] bytes = Encoding.Default.GetBytes(jsonFlie.text);
            string correntText = Encoding.UTF8.GetString(bytes);

            // 변환된 텍스트 사용
            itemList = JsonConvert.DeserializeObject<List<ItemData>>(correntText);
            Debug.Log($"로드된 아이템 수 : {itemList.Count}");

             foreach(var item in itemList)
            {
                Debug.Log($"아이템: {EncodeKorean(item.itemName)}, 설명 : {EncodeKorean(item.description)}");
            }
        }
        else
        {
            Debug.LogError($"JSON 파일을 찾을 수 없습니다. : {jsonFileName}");
        }
    }

    // 한글 이코딩을 위한 헬퍼 함수
     private string EncodeKorean(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";          // 텍스트가 NULL 값이며 함수를 끝낸다.
        byte[] bytes = Encoding.Default.GetBytes(text);      // string 을 Byte 배열로 변환한 후
        return Encoding.UTF8.GetString(bytes);              // 인코딩을 UTF8로 바꾼다.
    }

}
