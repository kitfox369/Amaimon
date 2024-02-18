using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StuffInfo  :MonoBehaviour
{
    int itmIdx=-1;
    int cost = -1;
    string name;
    int num = -1;

    public void intialize(int itmIdx, int cost,string name)
    {
        this.itmIdx = itmIdx;
        this.cost = cost;
        this.name = name;
    }

    public int GetItmIdx()
    {
        return this.itmIdx;
    }

    public int GetItmCost()
    {
        return this.cost;
    }

    public RewardPackage BuyStuff()
    {
        return new RewardPackage(-cost, itmIdx, name, 1);
    }
}
