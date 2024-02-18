using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public PlayerCamera playerCam;
    public UserPlayerCtrl mPlayer;
    private LivingEntity mPlayerEntity;
    UIManager uiM;
    NPCManager npcM;

    [Header("World Time")]
    float mWorldClock=50f;
    public float mDaySpeed=3.0f;
    public Transform Sun;

    public List<QuestType> questList;

    AudioSource audioSrc;
    public AudioClip[] bgms;
    int pirorBGMNum;

    void Start()
    {
        npcM = this.transform.GetChild(0).GetComponent<NPCManager>();
        uiM = this.transform.GetChild(1).GetComponent<UIManager>();
        mPlayerEntity = mPlayer.GetComponent<LivingEntity>();
        mPlayer.settingOfManager(uiM);
        audioSrc = this.GetComponent<AudioSource>();
        audioSrc.clip = bgms[0];
        audioSrc.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if ((int)mPlayer.mPlayerState != 8) { 
             //시간 흐르게 하기
            //mWorldClock -= Time.deltaTime* mDaySpeed;
            //Sun.transform.rotation = Quaternion.Euler(mWorldClock,-100,0);
            //if (mWorldClock <= 0) mWorldClock = 180;

            uiM.UpdateRunGuage(mPlayer.getFastRunGage());
            uiM.UpdateHPGuage(mPlayer.getHPGage());

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (mPlayer.getInteractMode() == 1 || mPlayer.getInteractMode() == 2)
                {
                    mPlayer.bIsMove = uiM.GetIsEndComment();
                    int interactNPCID = mPlayer.interactNPCID;

                    if (mPlayer.getInteractMode() == 1 && !mPlayer.bIsMove)
                    {
                        uiM.disableCommentTip();
                        if (mPlayer.InteractNPCKind == 0)
                        {
                            mPlayer.setInteractMode(2);
                            uiM.startNPCComment(interactNPCID, npcM.GetComment(interactNPCID), npcM.GetQuestState(interactNPCID));
                            mPlayer.mCam.setCameraMode(1, npcM.getNPCTrans(mPlayer.interactNPCID));
                        }
                        else
                        {
                            mPlayer.setInteractMode(3);
                            uiM.startNPCComment(interactNPCID, npcM.GetShopComment(interactNPCID), npcM.GetShopKind(interactNPCID));
                            uiM.StartOfShop(npcM.getShopNPCCon(interactNPCID));
                            mPlayer.mCam.setCameraMode(1, npcM.getShopNPCTrans(mPlayer.interactNPCID));
                        }

                        npcM.InteractiveWithPlayer(mPlayer.interactNPCID, mPlayer.transform.position, mPlayer.InteractNPCKind);
                        mPlayer.RemoveOutLineMaterials();
                    }
                    else if (mPlayer.bIsMove)
                    {
                        mPlayer.setInteractMode(0);
                        mPlayer.mCam.setCameraMode(0, mPlayer.transform.GetChild(1));
                        npcM.UninteractiveWithPlayer(mPlayer.interactNPCID, mPlayer.transform.position, mPlayer.InteractNPCKind);
                        if (npcM.GetQuestState(interactNPCID)==2)
                        {
                            int questIdx = getQuestIndex(npcM.GetQuestIdxWithNPCID(interactNPCID));
                            uiM.removeQuestContent(questIdx);
                            mPlayer.UnpackReward(npcM.GiveReward(interactNPCID));         //보상 지급
                            questList.RemoveAt(questIdx);
                            //퀘스트가 또 있는지 확인
                        }
                        else if(npcM.GetQuestState(interactNPCID) == 0)
                        {
                            questList.Add(npcM.acceptQuest(mPlayer.interactNPCID));
                            uiM.addQuestContent();
                        }

                        uiM.endNPCComment();
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (mPlayer.getInteractMode() == 3)
                {
                    mPlayer.setInteractMode(0);
                    mPlayer.mCam.setCameraMode(0, mPlayer.transform.GetChild(1));
                    npcM.UninteractiveWithPlayer(mPlayer.interactNPCID, mPlayer.transform.position, mPlayer.InteractNPCKind);
                    uiM.EndOfShop();
                }
            }
        }
        else
        {
            playerCam.GameOverEffect();
        }
    }

    private int getQuestIndex(int questIdx)
    {
        for (int i = 0; i < questList.Count; i++)
        {
            if (questList[i].mID == questIdx)
                return i;
        }

        return -1;
    }

    public void UpdateHuntQuestState(int monKind,int monIdx)
    {
        for (int i=0;i< questList.Count;i++)
        {
            if (questList[i].mKind == 1&& questList[i].mHuntMonIdx == monIdx&& questList[i].mHuntMonKind == monKind)
            {
                HuntQuest hunt = (HuntQuest)questList[i];
                hunt.UpdateMonNum();
                uiM.UpdateQuestState(i,hunt.mHuntedNum, hunt.mHuntMonNum, hunt.CheckConstraint());
                if (hunt.CheckConstraint()) { npcM.completeQuest(i,hunt.mNPCID); }
            }
        }
    }

    public void ChangeBGM(AudioClip audioClip)
    {
        audioSrc.clip = audioClip;
    }

    public void ChangeBGM(int index)
    {
        audioSrc.clip = bgms[index];
        pirorBGMNum = index;
    }

    public void BackToPriorBGM()
    {
        audioSrc.clip = bgms[pirorBGMNum];
    }

    public void SetActiveBGM(bool play)
    {
        if (play) { audioSrc.Play(); }
        else { audioSrc.Stop(); }
    }
}
