using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public DataManager dataM;
    public MonsterManager monsterM;
    [Header("[ShoptNPC]")]
    public Transform ShopNPCGroup;
    Vector3[] ShopNPCOriginRot;
    Transform[] ShopNPC;
    ShopNPCContainer[] ShopNPCCon;
    public int ShopNPCCount;
    // Start is called before the first frame update
    [Header("[QuestNPC]")]
    public Transform NPCGroup;
    Vector3[] NPCOriginRot;
    Transform[] NPC;
    NPCContainer[] npcCon;
    public int NpcCount;

    public Material[] questMat;
    public Material[] shopMat;

    [Header("[ItemDB]")]
    public ItemDataList itmDB;
    [Header("[MiniMapContainer]")]
    public MiniMapContainer miniMap;

    public Transform getNPCTrans(int index)
    {
        return NPC[index];
    }

    public Transform getShopNPCTrans(int index)
    {
        return ShopNPC[index];
    }

    public ShopNPCContainer getShopNPCCon(int index)
    {
        return ShopNPCCon[index];
    }

    public void InteractiveWithPlayer(int index,Vector3 playerPos,int npcKind)
    {
        if (npcKind == 0)
        {
            NPC[index].rotation = Quaternion.LookRotation(playerPos - NPC[index].localPosition);
            NPC[index].GetChild(0).GetComponent<Animator>().SetBool("IsTalking", true);
        }
        else
        {
            ShopNPC[index].rotation = Quaternion.LookRotation(playerPos - ShopNPC[index].localPosition);
            ShopNPC[index].GetChild(0).GetComponent<Animator>().SetBool("IsTalking", true);
        }
    }

    public void UninteractiveWithPlayer(int index, Vector3 playerPos, int npcKind)
    {
        if (npcKind == 0)
        {
            NPC[index].rotation = Quaternion.Euler(NPCOriginRot[index].x, NPCOriginRot[index].y, NPCOriginRot[index].z);
            NPC[index].GetChild(0).GetComponent<Animator>().SetBool("IsTalking", false);
        }
        else
        {
            ShopNPC[index].rotation = Quaternion.Euler(ShopNPCOriginRot[index].x, ShopNPCOriginRot[index].y, ShopNPCOriginRot[index].z);
            ShopNPC[index].GetChild(0).GetComponent<Animator>().SetBool("IsTalking", false);
        }
    }

    public QuestType acceptQuest(int index)
    {
        npcCon[index].doingQuest();
        miniMap.UpdateQuestIcon(index);
        int questMonIdx = npcCon[index].GetQuestMonsterIndex();
        int questMonKind= npcCon[index].GetQuestMonsterKind();
        if(questMonKind==0)
            miniMap.AddQuestArea(monsterM.monsterParentTrans[questMonIdx], monsterM.mMonsterZone[questMonIdx]);
        else
            miniMap.AddQuestArea(monsterM.bossParentTrans[questMonIdx], monsterM.mBossZone[questMonIdx]);
        return npcCon[index].quests[0];
    }

    public void completeQuest(int index,int npcID)
    {
        npcCon[npcID].completeQuest();
        miniMap.UpdateQuestIcon(npcID);
        miniMap.DisableQuestArea(index);
    }

    public void UpdateNPCQuest(int index)
    {
        npcCon[index].quests.RemoveAt(0);
        if (npcCon[index].haveQuest())
        {
            miniMap.UpdateQuestIcon(index);
        }
        else
        {
            miniMap.DisableQuestIcon(index);
        }
    }

    public int GetShopKind(int index)
    {
        return ShopNPCCon[index].ShopKind;
    }


    public int GetQuestState(int index)
    {
        return npcCon[index].quests[0].questState;
    }

    public int GetQuestIdxWithNPCID(int npcIdx)
    {
        return npcCon[npcIdx].quests[0].mID;
    }

    public RewardPackage GiveReward(int index)
    {
        QuestType quest = npcCon[index].quests[0];
        RewardPackage reward = new RewardPackage(quest.mRewardMoney, quest.mRewardItmIdx, itmDB.GetItmName(quest.mRewardItmIdx), quest.mRewardItmNum);
        UpdateNPCQuest(index);
        return reward;
    }

    void Start()
    {
        //quest NPC
        NPC = new Transform[NPCGroup.childCount];
        NpcCount = NPCGroup.childCount;
        NPCOriginRot = new Vector3[NPCGroup.childCount];
        for (int i=0;i<NPC.Length;i++)
        {
            NPC[i] = NPCGroup.GetChild(i);
            NPCOriginRot[i] = NPC[i].rotation.eulerAngles;
        }

        //shop NPC
        ShopNPC = new Transform[ShopNPCGroup.childCount];
        ShopNPCCount = ShopNPCGroup.childCount;
        ShopNPCOriginRot = new Vector3[ShopNPCGroup.childCount];
        for (int i = 0; i < ShopNPC.Length; i++)
        {
            ShopNPC[i] = ShopNPCGroup.GetChild(i);
            ShopNPCOriginRot[i] = ShopNPC[i].rotation.eulerAngles;
        }

      
        dataM.Load();
        assignName();
        assignQuest();
        assignShop();

        miniMap.InitializeQuestIcon();
        miniMap.InitializeShopIcon();


    }

    public Comment GetComment(int index)
    {
        QuestType quest = npcCon[index].quests[0];
        return new Comment(npcCon[index].NPCName, quest.mDescription,
            quest, quest.mQuestDescription, quest.mHintMent, quest.mFinishMent);
    }

    public ShopComment GetShopComment(int index)
    {
        return new ShopComment(ShopNPCCon[index].NPCName, ShopNPCCon[index].GeneralComment,
            ShopNPCCon[index].replyMent);
    }

    private void assignName()
    {
        npcCon = new NPCContainer[NPC.Length];
        for (int i = 0; i < NPC.Length; i++)
        {
            NPC[i].AddComponent<NPCContainer>();
            npcCon[i] = NPC[i].GetComponent<NPCContainer>();
            npcCon[i].ID = i;
            npcCon[i].questState = questMat;
        }

        Storage nameStorage = dataM.storages[0];
        for (int i = 0; i < nameStorage.items[0].itm.Length; i++)
        {
            int npcIdx = Int32.Parse(nameStorage.items[0].itm[i]);
            string npcName = nameStorage.items[1].itm[i];
            if (NPC[npcIdx].GetComponent<NPCContainer>() != null)
            {
                npcCon[npcIdx].NPCName = npcName;
            }
        }

        ShopNPCCon = new ShopNPCContainer[ShopNPC.Length];
        for (int i = 0; i < ShopNPC.Length; i++)
        {
            ShopNPC[i].AddComponent<ShopNPCContainer>();
            ShopNPCCon[i] = ShopNPC[i].GetComponent<ShopNPCContainer>();
            ShopNPCCon[i].ID = i;
            ShopNPCCon[i].ShopKindMat = shopMat[i];
        }

        nameStorage = dataM.storages[2];
        for (int i = 0; i < ShopNPC.Length; i++)
        {
            int npcIdx = Int32.Parse(nameStorage.items[0].itm[i]);
            string npcName = nameStorage.items[1].itm[i];
            if (ShopNPC[npcIdx].GetComponent<ShopNPCContainer>() != null)
            {
                ShopNPCCon[npcIdx].NPCName = npcName;
            }
        }

    }

    //
    private void assignQuest()
    {
        Storage questStorage = dataM.storages[1];
        for (int i = 0; i < questStorage.items[0].itm.Length; i++)
        {
            int npcIdx = Int32.Parse(questStorage.items[1].itm[i]);
            if (NPC[npcIdx].GetComponent<NPCContainer>() != null)
            {
                int id = Int32.Parse(questStorage.items[0].itm[i]);
                string questDesc = questStorage.items[2].itm[i];
                string description = questStorage.items[3].itm[i];
                string hintMent = questStorage.items[4].itm[i];
                string finishMent = questStorage.items[5].itm[i];
                int kind = Int32.Parse(questStorage.items[6].itm[i]);
                bool main = (questStorage.items[7].itm[i] == "Y") ? true : false;
                bool repeat = (questStorage.items[8].itm[i] == "Y") ? true : false;
                if (kind == 0)
                {
                    npcCon[npcIdx].quests.Add(new QuestType(kind, description, id, npcIdx, questDesc, hintMent, finishMent, main,repeat));
                }
                else if (kind == 1)
                {
                    int monsterKind = Int32.Parse(questStorage.items[10].itm[i]);
                    int monsterId = Int32.Parse(questStorage.items[11].itm[i]);
                    int monsterNum = Int32.Parse(questStorage.items[12].itm[i]);
                    int rewardMoney = Int32.Parse(questStorage.items[13].itm[i]);
                    int rewardItmIdx = Int32.Parse(questStorage.items[14].itm[i]);
                    int rewardItmNum = Int32.Parse(questStorage.items[15].itm[i]);
                    npcCon[npcIdx].quests.Add(new HuntQuest(kind, description, id, npcIdx, questDesc, hintMent, finishMent, main, repeat, monsterKind, monsterId, monsterNum, rewardMoney,rewardItmIdx,rewardItmNum));
                }
            }
        }
    }

    private void assignShop()
    {
        Storage questStorage = dataM.storages[3];
        //for (int i = 0; i < questStorage.items[0].itm.Length; i++)
        for (int i = 0; i < ShopNPCCount; i++)
        {
            int npcIdx = Int32.Parse(questStorage.items[0].itm[i]);
            if (ShopNPC[npcIdx].GetComponent<ShopNPCContainer>() != null)
            {
                int id = Int32.Parse(questStorage.items[0].itm[i]);
                string comment = questStorage.items[1].itm[i];
                string replyMent = questStorage.items[2].itm[i];
                int shopKind = Int32.Parse(questStorage.items[3].itm[i]);

                ShopNPCCon[npcIdx].initializeShopNPC(id,comment, replyMent, shopKind);

                // 아이템 리스트 옮기기
                string itmList = questStorage.items[4].itm[i];
                string[] itmS;
                itmS = itmList.Split(",");
                int[] itmSIn = new int[itmS.Length];
                for(int j=0; j<itmS.Length;j++) { itmSIn[j] = int.Parse(itmS[j]); }
                ShopNPCCon[npcIdx].itm = itmSIn;

                //아이템 가격 리스트 옮기기
                string costList = questStorage.items[5].itm[i];
                string[] costS;
                costS = costList.Split(",");
                int[] costSIn = new int[costS.Length];
                for (int j = 0; j < costS.Length; j++) { costSIn[j] = int.Parse(costS[j]); }
                ShopNPCCon[npcIdx].cost = costSIn;

            }
        }
    }
}

[System.Serializable]
public struct RewardPackage
{
    public int gold;
    public int itemIds;
    public string itemName;
    public int itemNum;

    public RewardPackage(int gold, int itemIds,string itemName,int itemNum)
    {
        this.gold = gold;
        this.itemIds = itemIds;
        this.itemName = itemName;
        this.itemNum = itemNum;
    }

    public RewardPackage(RewardPackage reward)
    {
        this.gold = reward.gold;
        this.itemIds = reward.itemIds;
        this.itemName = reward.itemName;
        this.itemNum = reward.itemNum;
    }
}