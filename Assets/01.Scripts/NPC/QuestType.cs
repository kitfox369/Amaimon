using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class QuestType
{
    public string mName { get; protected set; }
    public string mDescription { get; protected set; }
    public string mQuestDescription { get; protected set; }
    public string mHintMent { get; protected set; }
    public string mFinishMent { get; protected set; }
    public int mID { get; protected set; }
    public int mNPCID { get; protected set; }
    public int questState { get; protected set; }
    public int mKind { get; protected set; }
    public bool isMain { get; protected set; }
    public bool isRepeat { get; protected set; }
    public int mHuntedNum { get;protected set; }
    public int mHuntMonNum { get; protected set; }
    public int mHuntMonKind { get; protected set; }
    public int mHuntMonIdx { get; protected set; }
    public int mRewardMoney { get; protected set; }
    public int mRewardItmIdx { get; protected set; }
    public int mRewardItmNum { get; protected set; }

    public QuestType(int kind, string description, int id, int npcID, string questDescpt,string hintMent,string finishMent, bool isMain, bool isRepeat)
    {
        mKind = kind;
        mDescription = description;
        mID = id;
        mNPCID = npcID;
        this.mQuestDescription = questDescpt;
        this.mHintMent = hintMent;
        this.mFinishMent = finishMent;
        this.isMain = isMain;
        this.isRepeat = isRepeat;
        questState = 0;
    }

    public virtual bool CheckConstraint()
    {
        return true;
    }

    public void SetQuestState(int state)
    {
        questState = state;
    }

}

[System.Serializable]
public class HuntQuest : QuestType
{
    public HuntQuest(int kind, string description, int id,int npcID,string questDescription, string hintMent, string finishMent, bool isMain, bool isRepeat,int monsterKind, int monsterIdx,int num,int rewardMoney, int rewardItmIdx,int rewardItmNum) : base(kind, description, id,npcID, questDescription,hintMent,finishMent,isMain,isRepeat)
    {
        mHuntedNum = 0;
        mHuntMonNum = num;
        mHuntMonKind = monsterKind;
        mHuntMonIdx = monsterIdx;
        mRewardMoney = rewardMoney;
        mRewardItmIdx = rewardItmIdx;
        mRewardItmNum = rewardItmNum;

    }

    public override bool CheckConstraint()
    {
        //base.CheckConstraint();
        if (mHuntedNum == mHuntMonNum) {
            questState = 2;
            return true; 
        }
        else return false;
    }

    public int UpdateMonNum()
    {
        return ++mHuntedNum;
    }
    public string getQuestInfo() 
    {
        return "HuntQuest";
    }

}

//public class GetQuest : QuestType
//{


//    public GetQuest(string name, string description, int id,string questDescription) : base(name, description, id, questDescription)
//    {

//    }

//    public override void CheckConstraint()
//    {
//        //base.CheckConstraint();
//    }
//}

//public class ConveyQuest : QuestType
//{

//    public ConveyQuest(string name, string description, int id,string questDescription) : base(name, description, id, questDescription)
//    { 

//    }

//    public override void CheckConstraint()
//    {
//        //base.CheckConstraint();
//    }
//}
