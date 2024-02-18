using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopNPCContainer : MonoBehaviour
{
    public int ID;
    public string NPCName;
    public string GeneralComment;
    public string replyMent;
    public int ShopKind { get; set; }
    public Material ShopKindMat;
    public int[] itm;
    public int[] cost;


    public void initializeShopNPC(int id,string comment, string replyMent, int shopKind)
    {
        ID = id;
        GeneralComment = comment;
        this.replyMent = replyMent;
        ShopKind = shopKind;
    }

}
