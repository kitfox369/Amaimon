using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : BaseBehaviour
{
    // Start is called before the first frame update
    public PlayerCamera playerCam;
    public UserPlayerCtrl mPlayer;
    private LivingEntity mPlayerEntity;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private NPCManager _npcManager;

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
        
        mPlayerEntity = mPlayer.GetComponent<LivingEntity>();
        mPlayer.settingOfManager(_uiManager);
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

            _uiManager.UpdateRunGuage(mPlayer.getFastRunGage());
            _uiManager.UpdateHPGuage(mPlayer.getHPGage());

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (mPlayer.getInteractMode() == 1 || mPlayer.getInteractMode() == 2)
                {
                    mPlayer.bIsMove = _uiManager.GetIsEndComment();
                    int interactNPCID = mPlayer.interactNPCID;

                    if (mPlayer.getInteractMode() == 1 && !mPlayer.bIsMove)
                    {
                        _uiManager.disableCommentTip();
                        if (mPlayer.InteractNPCKind == 0)
                        {
                            mPlayer.setInteractMode(2);
                            _uiManager.startNPCComment(interactNPCID, _npcManager.GetComment(interactNPCID), _npcManager.GetQuestState(interactNPCID));
                            mPlayer.mCam.setCameraMode(1, _npcManager.getNPCTrans(mPlayer.interactNPCID));
                        }
                        else
                        {
                            mPlayer.setInteractMode(3);
                            _uiManager.startNPCComment(interactNPCID, _npcManager.GetShopComment(interactNPCID), _npcManager.GetShopKind(interactNPCID));
                            _uiManager.StartOfShop(_npcManager.getShopNPCCon(interactNPCID));
                            mPlayer.mCam.setCameraMode(1, _npcManager.getShopNPCTrans(mPlayer.interactNPCID));
                        }

                        _npcManager.InteractiveWithPlayer(mPlayer.interactNPCID, mPlayer.transform.position, mPlayer.InteractNPCKind);
                        mPlayer.RemoveOutLineMaterials();
                    }
                    else if (mPlayer.bIsMove)
                    {
                        mPlayer.setInteractMode(0);
                        mPlayer.mCam.setCameraMode(0, mPlayer.transform.GetChild(1));
                        _npcManager.UninteractiveWithPlayer(mPlayer.interactNPCID, mPlayer.transform.position, mPlayer.InteractNPCKind);
                        if (_npcManager.GetQuestState(interactNPCID)==2)
                        {
                            int questIdx = getQuestIndex(_npcManager.GetQuestIdxWithNPCID(interactNPCID));
                            _uiManager.removeQuestContent(questIdx);
                            mPlayer.UnpackReward(_npcManager.GiveReward(interactNPCID));         //보상 지급
                            questList.RemoveAt(questIdx);
                            //퀘스트가 또 있는지 확인
                        }
                        else if(_npcManager.GetQuestState(interactNPCID) == 0)
                        {
                            questList.Add(_npcManager.acceptQuest(mPlayer.interactNPCID));
                            _uiManager.addQuestContent();
                        }

                        _uiManager.endNPCComment();
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (mPlayer.getInteractMode() == 3)
                {
                    mPlayer.setInteractMode(0);
                    mPlayer.mCam.setCameraMode(0, mPlayer.transform.GetChild(1));
                    _npcManager.UninteractiveWithPlayer(mPlayer.interactNPCID, mPlayer.transform.position, mPlayer.InteractNPCKind);
                    _uiManager.EndOfShop();
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
                _uiManager.UpdateQuestState(i,hunt.mHuntedNum, hunt.mHuntMonNum, hunt.CheckConstraint());
                if (hunt.CheckConstraint()) { _npcManager.completeQuest(i,hunt.mNPCID); }
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

#if UNITY_EDITOR

    protected override void OnBindField()
    {
        base.OnBindField();
        _npcManager = FindAnyObjectByType<NPCManager>();
        _uiManager = FindAnyObjectByType<UIManager>();
    }

#endif
}
