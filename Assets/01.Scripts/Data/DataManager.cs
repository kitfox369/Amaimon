using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

/**
 * 
 * 
 * 
 */
[System.Serializable]
public class Item
{
    public string name;
    //public Content[] itm;
    public string[] itm;
    public Item() { }
}

[System.Serializable]
public class Storage
{
    public string name;
    public Item[] items;
    public Storage() { }
}

[System.Serializable]
public class Category
{
    public string category;       //directory
    public string[] tagName;
}

[System.Serializable]
public class DataManager
{
    int resouceSize;
    public string[] xmlFileNmae;
    public Category[] categories;
    public Storage[] storages;
    //생성자
    public DataManager() { }

    // Start is called before the first frame update
    public void Load()
    {
        //Shop 관련 data 불러오기
        storages = new Storage[categories.Length];
        storages.Initialize();

        for (int i = 0; i < categories.Length; i++)
        {
            storages[i] = new Storage();
            storages[i].items = new Item[categories[i].tagName.Length];
            storages[i].name = categories[i].category;
            for (int z = 0; z < categories[i].tagName.Length; z++)
            {
                storages[i].items[z] = new Item();
                storages[i].items[z].name = categories[i].tagName[z];
                LoadXML(categories[i].category, xmlFileNmae[i], categories[i].tagName[z], storages[i].items[z]);
            }
        }

    }

    private void LoadXML(string category, string fileName, string tagName, Item content)
    {
        TextAsset txtAsset = (TextAsset)Resources.Load("XML/" + fileName);
        XmlDocument xmlDoc = new XmlDocument();
        //Debug.Log(txtAsset.text);
        xmlDoc.LoadXml(txtAsset.text);

        // 하나씩 가져오기 테스트 예제.
        XmlNodeList cost_Table = xmlDoc.GetElementsByTagName(tagName);
        content.itm = new string[cost_Table.Count];
        int idx = 0;
        foreach (XmlNode cost in cost_Table)
        {
            content.itm[idx] = cost.InnerText;
            idx++;
            //Debug.Log("[one by one] cost : " + cost.InnerText);
        }

    }

}
