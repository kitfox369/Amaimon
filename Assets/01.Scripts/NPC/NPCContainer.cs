using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCContainer : MonoBehaviour
{
    public int ID;
    public string NPCName;
    public string GeneralComment;
    public List<QuestType> quests;
    private MeshRenderer plane;
    public Material[] questState;


    private void OnEnable()
    {
        quests = new List<QuestType>();
        plane = transform.GetChild(1).GetComponent<MeshRenderer>();
    }

    public void doingQuest()
    {
        plane.enabled = false;
        quests[0].SetQuestState(1);
    }

    public bool haveQuest()
    {
        if (quests.Count > 0)
        {
            if (quests[0].mKind == 0)       //MAIN
            {
                plane.material = questState[0];
            }
            else
            {
                plane.material = questState[1];
            }

            return true;
        }
        else
        {
            plane.enabled=false;
            return false;
        }
    }

    public void completeQuest()
    {
        plane.enabled = true;
        if (quests[0].mKind == 0)       //MAIN
        {
            plane.material = questState[2];
        }
        else
        {
            plane.material = questState[3];
        }
    }

    public int GetQuestMonsterIndex()
    {
        return quests[0].mHuntMonIdx;
    }

    public int GetQuestMonsterKind()
    {
        return quests[0].mHuntMonKind;
    }

}
