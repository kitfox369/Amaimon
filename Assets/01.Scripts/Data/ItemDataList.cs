using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDataList : MonoBehaviour
{
    public List<InGameItem> list;

    public string GetItmName(int idx)
    {
        return list[idx].name;
    }

    public Sprite GetItmImg(int idx)
    {
        return list[idx].itmImg;
    }
}

[System.Serializable]
public class InGameItem
{
    public int idx;
    public string name;
    public Sprite itmImg;
    public int count { get; protected set; }

    public InGameItem(int idx,string name, int num)
    {
        this.idx = idx;
        this.name = name;
        this.count = num;
    }

    public void AddItm(int num)
    {
        this.count += num;
    }

    public void UseItm()
    {
        this.count -= 1;
    }

}