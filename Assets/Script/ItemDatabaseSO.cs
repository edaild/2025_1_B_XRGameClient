using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Database")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();                  // ItemSO�� ����Ʈ�� ���� �Ѵ�.

    //ĳ���� ���� ����
    private Dictionary<int, ItemSO> itemsByld;                  // ID�� ������ ã�� ���� ĳ��
    private Dictionary<string, ItemSO> itemsByName;             // �̸����� ������ ã��

    public void Initialize()                                    // �ʱ� ���� �Լ�
    {
        itemsByld = new Dictionary<int, ItemSO>();              // ���� ���� �߱� ������ Dictionary �Ҵ�
        itemsByName = new Dictionary<string, ItemSO>();

        foreach (var item in items)                             //items ����Ʈ�� ���� �Ǿ� �ִ°��� ������ Dictionary�� �Է��Ѵ�.
        {
            itemsByld[item.id] = item;
            itemsByName[item.itemName] = item;
        }
    }

    // ID�� ������ ã��
    public ItemSO GetItemByld(int id)
    {
       if(itemsByld == null)                                         //itemsByld  �� ĳ���� �Ǿ� ���� �ʴٸ� �ʱ�ȭ �Ѵ�.
        {
            Initialize();
        }

       if (itemsByld.TryGetValue(id, out ItemSO item))              // id ���� ã�Ƽ� ItemSO �� ���� �Ѵ�.
            return item;

       return null;                                                 // ���� ��� NULL
    }

    // �̸����� ������ ã��
    public ItemSO GetItemByName(string name)
    {
        if(itemsByName == null)                                  //ItemsByname �� ĳ���� �Ǿ� ���� �ʴٸ� �ʱ�ȭ �Ѵ�
        {
            Initialize();
        }
        if(itemsByName.TryGetValue(name, out ItemSO item))      // name ���� ã�Ƽ� itemSO �� ���� �Ѵ�.
            return item;

        return null;
    }

    // Ÿ������ ������ ���͸�
    public List<ItemSO> GetItemsByType(ItemType type)
    {
        return items.FindAll(item => item.itemType == type); 
    }
}
