using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MapManager : MonoBehaviour
{
    // Start is called before the first frame update\

    public List<linkedPotal> potalArray;
    public Transform parentObjMap;
    public Transform parentObjPotal;

    public int[] bgmArrays;

    private void Start()
    {
        Transform temp;
        int tempCount = parentObjPotal.childCount;
        for (int i = 0; i < tempCount; i++)
        {
            temp = parentObjPotal.GetChild(i);
            int tempChildCount = temp.childCount;

            Transform[] tempArray = new Transform[2];
            string[] nameArray = new string[2];
            for (int j = 0; j < tempArray.Length; j++)
            {
                tempArray[j] = temp.GetChild(j);
                nameArray[j] = tempArray[j].name;
            }

            potalArray.Add(new linkedPotal(tempArray, nameArray, bgmArrays));
        }
    }

    public Potal enterPotal(Vector3 tempPos)
    {
        for(int i=0;i<potalArray.Count;i++)
        {
            for(int j = 0; j < potalArray[i].potal.Length; j++)
            {
                if ((potalArray[i].potal[j].potalTrans.position.x==tempPos.x)&& (potalArray[i].potal[j].potalTrans.position.z == tempPos.z))
                {
                    if (j == 0){return potalArray[i].potal[1];}
                    else{return potalArray[i].potal[0];}
                }
            }
        }

        return null;
    }

}

[System.Serializable]
public class linkedPotal
{
    public Potal[] potal = new Potal[2];
    //public string[] name = new string[2];
    //public Transform[] potal = new Transform[2];
    public Potal getPotalTrans(int num) { return potal[num]; }

    public linkedPotal(Transform[] transArray, string[] nameArray, int[] bgmArray)
    {
        for(int i = 0; i < transArray.Length; i++)
        {
            potal[i] = new Potal(transArray[i], nameArray[i], bgmArray[i]);
        }
    }
}

[System.Serializable]
public class Potal
{
    public string name;
    public int bgmNum;
    public Transform potalTrans;

    public Potal(Transform trans, string name, int bgmNum)
    {
        potalTrans = trans;
        this.name = name;
        this.bgmNum = bgmNum;
    }
}