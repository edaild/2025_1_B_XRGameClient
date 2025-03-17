using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class ItemData 
{
    public int id;
    public string itemName;
    public string description;
    public string nameEng;
    public string itemTypeString;
    [NonSerialized]
    public ItemType ItemType;
    public int price;
    public int power;
    public int level;
    public bool isStackable;
    public string iconPath;

    // 문자열을 열거형으로 변환하는 메서드
    public void InitalizeEnums()
    {
        if(Enum.TryParse(itemTypeString, out ItemType parsedType))
        {
            ItemType = parsedType;
        }
        else
        {
            Debug.Log($"아이템 !{itemName}'에 유효하지 않은 아이템 타입 : {itemTypeString}");
            // 기본값 설정
            ItemType = ItemType.Consumable;
        }
    }
}

