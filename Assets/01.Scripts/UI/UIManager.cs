using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameManager gm;
    public UserPlayerCtrl playerCtrl;

    [Header("▶ Status")]
    public Image mHPGauge;
    public Image mRunGauge;

    [Header("▶ Comment")]
    // Start is called before the first frame update
    public Transform comment;
    private Text commentTxt;

    public Transform commentTip;
    private Text commentTipTxt;

    private bool isComment = false;
    private bool isEndComment = false;

    private int interactNPCIdx;

    [Header("▶ Shop")]
    // Start is called before the first frame update
    public Transform shopWindow;
    public Transform shopWindowContent;
    public GameObject stuffConPrefab;
    public GameObject stuffBuyWindow;
    public GameObject shopNoticeWindow;
    public GameObject maskImg;
    StuffInfo stuffInCart;
    public TextMeshProUGUI myMoneyTxt;

    [Header("[ItemDB]")]
    public ItemDataList itmDB;
    public Sprite goldImg;
    RewardPackage tempReward;

    [Header("▶ Quest")]
    public Transform quest;
    public Transform content;
    public GameObject questFramePrefab;

    [Header("▶ Canvas")]
    public RectTransform mCanvasRect;
    public GameObject InGameCanvas;
    public GameObject GameOverCanvas;

    [Header("▶ Minimap")]
    public Image mMiniMapImg;
    private MiniMapContainer mMiniMapContainer;
    public MapManager mMapManager;

    [Header("▶ Map Info")]
    public Transform mMapInfo;
    public Text mMapInfoText;
    public Text mMapInfoShadow;
    public Material mMapInfoMaterial;
    private float mapInfoAnimTime = 3.0f;
    float fadeValue = 0.0f;
    float appliedTime = 2.0f;

    [Header("▶ Boss Info")]
    public Transform mBossIntroInfo;
    private Text mBossIntroTxt;
    private Text mBossIntroTxtShadow;
    public Transform mBossStateInfo;
    private Text mBossStateName;
    public Image mBossHPGauge;
    private LivingEntity mBossEntity;

    [Header("▶ Reward")]
    public Transform mBossRewardInfo;
    public Transform mBossRewardContent;
    public GameObject mBossRewardPrefab;
    public Transform mRewardNoticeContent;
    public GameObject mRewardNoticePrefab;

    [Header("▶ TextReader")]
    TextReader txtManager;

    [Header("▶ Skill Window")]
    public Transform mSkillWindow;
    public SkillContainer[] mSkill;

    [Header("▶ Buff Window")]
    public Transform mBuffContainer;
    public GameObject mBuffPrefab;

    private void Awake()
    {
        mMiniMapContainer = transform.GetChild(0).GetComponent<MiniMapContainer>();
        //Skill 리셋
        for (int i = 0; i < mSkill.Length; i++)
        {
            mSkill[i].intialize();
            mSkill[i].coolTimeReset();
        }
    }
    void Start()
    {
        commentTxt = comment.transform.GetChild(0).GetComponent<Text>();
        commentTipTxt = commentTip.transform.GetChild(1).GetComponent<Text>();

        txtManager = this.GetComponent<TextReader>();
        txtManager.commentTxt = commentTxt;
        txtManager.nameTxt = comment.GetChild(1).GetChild(0).GetComponent<Text>();

        mMapInfoShadow= mMapInfo.GetChild(0).GetComponent<Text>();
        mMapInfoText=mMapInfo.GetChild(1).GetComponent<Text>();

        mBossIntroTxtShadow = mBossIntroInfo.GetChild(0).GetComponent<Text>();
        mBossIntroTxt = mBossIntroInfo.GetChild(1).GetComponent<Text>();

        mBossStateName = mBossStateInfo.GetComponent<Text>();

        tempReward = new RewardPackage();

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < mSkill.Length; i++)
        {
            mSkill[i].updateSkillContainer();
        }

        if (mBossEntity)
        {
            mBossHPGauge.fillAmount = mBossEntity.mHealth / (float)mBossEntity.startingHealth;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!mMiniMapImg.gameObject.activeSelf)
            {
                mMiniMapImg.gameObject.SetActive(true);
            }
            else
            {
                mMiniMapImg.gameObject.SetActive(false);
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            mMiniMapContainer.ZoomInOutMap(scroll);
        }
    }

    public void addQuestContent()
    {
        GameObject questObj = Instantiate(questFramePrefab, Vector3.zero, Quaternion.identity);
        questObj.transform.GetChild(0).GetComponent<Text>().text = txtManager.currentCom.QuestDescription;
        if (txtManager.currentCom.Type.mKind == 1)
        {
            questObj.transform.GetChild(1).GetComponent<Text>().text = "(0/"+ txtManager.currentCom.Type.mHuntMonNum.ToString()+")";
        }

        questObj.transform.parent = content;

    }

    public void removeQuestContent(int index)
    {
        Destroy(content.GetChild(index).gameObject);
    }

    public void UpdateQuestState(int index,int updateNum,int huntMonNum,bool isComplete)
    {
        content.GetChild(index).GetChild(1).GetComponent<Text>().text = "("+ updateNum + "/" + huntMonNum.ToString() + ")";
        if (isComplete) content.GetChild(index).GetComponent<Image>().color = Color.yellow;
    }

    public bool GetIsEndComment()
    {
        return isEndComment;
    }

    public void startNPCComment(int npcNum,Comment com,int questState)         //움직일 수 있는지 없는지 반환
    {
        comment.gameObject.SetActive(true);
        txtManager.startNPCComment(com, questState);
        isEndComment = true;
    }

    public void startNPCComment(int npcNum, ShopComment com, int questState)         //움직일 수 있는지 없는지 반환
    {
        comment.gameObject.SetActive(true);
        shopWindow.gameObject.SetActive(true);
        txtManager.startNPCComment(com, questState);
        isEndComment = true;
    }

    public void endNPCComment()
    {
        comment.gameObject.SetActive(false);
        isEndComment = false;               //추후 모든 말이 끝났을때로 변경해야함
    }

    void updateMyGold()
    {
        myMoneyTxt.text = playerCtrl.gold.ToString();
    }

    public void StartOfShop(ShopNPCContainer shopNPCContainer)
    {
        //gold 업데이트
        updateMyGold();

        for (int i = 0; i < shopNPCContainer.itm.Length; i++)
        {
            GameObject stuffObj = Instantiate(stuffConPrefab, Vector3.zero, Quaternion.identity);
            int itmIdx = shopNPCContainer.itm[i];
            int cost = shopNPCContainer.cost[i];
            string name = itmDB.GetItmName(itmIdx);
            stuffObj.transform.GetChild(0).GetComponent<Image>().sprite = itmDB.GetItmImg(itmIdx);
            stuffObj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
            stuffObj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = cost + "G";
            stuffObj.GetComponent<StuffInfo>().intialize(itmIdx, cost, name);
            stuffObj.GetComponent<Button>().onClick.AddListener(delegate { BuyItm(stuffObj.GetComponent<StuffInfo>()); });
            stuffObj.transform.parent = shopWindowContent;
        }
    }

    public void EndOfShop()
    {
        comment.gameObject.SetActive(false);
        shopWindow.gameObject.SetActive(false);
        isEndComment=false;
    }

    public void BuyItm(StuffInfo stuff)
    {
        stuffBuyWindow.SetActive(true);
        maskImg.SetActive(true);
        stuffInCart = stuff;
    }

    public void CancelBuyItm()
    {
        stuffBuyWindow.SetActive(false);
        maskImg.SetActive(false);
    }

    public void CheckOutOfBudget()
    {
        stuffBuyWindow.SetActive(false);
        if (playerCtrl.gold > stuffInCart.GetItmCost())
        {
            maskImg.SetActive(false);
            playerCtrl.UnpackReward(stuffInCart.BuyStuff());
            updateMyGold();
        }
        else
        {
            shopNoticeWindow.SetActive(true);
        }
        stuffInCart = null;
    }

    public void activeCommentTip(int npcIdx,string npcName)
    {
        interactNPCIdx = npcIdx;
        commentTipTxt.text = "'" + npcName + "'과(와) 대화하기";
        commentTip.gameObject.SetActive(true);
    }

    public void disableCommentTip()
    {
        interactNPCIdx = -1;
        if (commentTip.gameObject.activeSelf)
            commentTip.gameObject.SetActive(false);
    }

    public void ActiveBossState(LivingEntity bossEntity)
    {
        mBossEntity = bossEntity;
        mBossStateInfo.gameObject.SetActive(true);
    }

    public void DeactiveBossState()
    {
        mBossStateInfo.gameObject.SetActive(false);
    }

   public void UpdateRunGuage(float guaugeValue)
    {
        mRunGauge.fillAmount = guaugeValue;
    }

    public void UpdateHPGuage(float guaugeValue)
    {
        mHPGauge.fillAmount = guaugeValue;
    }

    public void InitializeItmInfo(List<InGameItem> itm)
    {
        for(int i=0;i<mSkill.Length;i++)
        {
            if (mSkill[i].mSkillKind == 1)
            {
                mSkill[i].UpdateItmNum(itm[i]);
            }
        }
    }

    public Potal getOutPotalInfo(Vector3 potalInPos)
    {
        Potal potal = mMapManager.enterPotal(potalInPos);
        mMapInfoShadow.text = potal.name;
        mMapInfoText.text = potal.name;
        return potal;
    }
    
    public void AddPassiveUI(int idx)
    {
        GameObject buff = Instantiate(mBuffPrefab, Vector3.zero,Quaternion.identity);
        buff.transform.parent = mBuffContainer.transform;
        buff.GetComponent<UIAutoDestroy>().setInfo(mSkill[idx].mMaintainTime, 0, true);
    }

    public void PassRewardToPlayer()
    {
        playerCtrl.UnpackReward(tempReward);
    }

    public void ActiveRewardUI(RewardPackage reward)
    {
        tempReward = reward;
        mBossRewardInfo.gameObject.SetActive(true);
        GameObject goldObj = Instantiate(mBossRewardPrefab, Vector3.zero, Quaternion.identity);
        goldObj.transform.GetChild(0).GetComponent<Image>().sprite = goldImg;
        goldObj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "골드";
        goldObj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = reward.gold.ToString()+"G";
        goldObj.transform.parent = mBossRewardContent;
        GameObject itmObj = Instantiate(mBossRewardPrefab, Vector3.zero, Quaternion.identity);
        itmObj.transform.GetChild(0).GetComponent<Image>().sprite = itmDB.GetItmImg(reward.itemIds);
        itmObj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = reward.itemName;
        itmObj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "X"+reward.itemNum.ToString();
        itmObj.transform.parent = mBossRewardContent;

    }

    public void NoticeReward(RewardPackage reward)
    {
        GameObject goldObj = Instantiate(mRewardNoticePrefab, Vector3.zero, Quaternion.identity);
        string goldTxt="";
        if (reward.gold > 0) { goldTxt+="골드 획득(+" + reward.gold.ToString() + ")"; }
        else { goldTxt += "골드 잃음(" + reward.gold.ToString() + ")"; }
        goldObj.GetComponent<TextMeshProUGUI>().text = goldTxt;
        goldObj.transform.parent = mRewardNoticeContent;
        GameObject itmObj = Instantiate(mRewardNoticePrefab, Vector3.zero, Quaternion.identity);
        itmObj.GetComponent<TextMeshProUGUI>().text = "아이템 획득(" + reward.itemName+ "x"+reward.itemNum.ToString()+")";
        itmObj.transform.parent = mRewardNoticeContent;
    }

    public void GameOverUI()
    {
        InGameCanvas.SetActive(false);
        GameOverCanvas.gameObject.SetActive(true);
    }

    public void SetActiveUI(bool setactive)
    {
        InGameCanvas.SetActive(setactive);
    }

    public void StartMapInfoAnimation()
    {
        StartCoroutine(MapInfoAnimation());
    }

    IEnumerator MapInfoAnimation()
    {
        float elapsedTime = 0.0f;
        appliedTime = mapInfoAnimTime / 2;
        while (elapsedTime < appliedTime)
        {
            elapsedTime += Time.deltaTime;

            fadeValue = elapsedTime / appliedTime;
            mMapInfoMaterial.SetColor("_Color", new Color(1,1,1,fadeValue));
            yield return null;
        }

        yield return new WaitForSeconds(mapInfoAnimTime);

        while (elapsedTime > 0)
        {
            elapsedTime -= Time.deltaTime;

            fadeValue = elapsedTime / appliedTime;
            mMapInfoMaterial.SetColor("_Color", new Color(1, 1, 1, fadeValue));
            yield return null;
        }
        yield return null;
    }
}

[System.Serializable]
public class SkillContainer
{
    public Transform mTransform;
    public uint mIndex;
    public Image mSkillImg;
    public Image mMaskImg;
    public string keyMapping;
    public float mRemainTime;
    public TextMeshProUGUI mRemainTxt;
    public TextMeshProUGUI mRemainItmNumTxt;
    public float mReloadTime;
    public float mMaintainTime;
    public int mSkillKind;      //0:skill , 1:itm
    public int mItmIdx;

    public void intialize()
    {
        mSkillImg = mTransform.GetChild(0).GetComponent<Image>();
        mMaskImg = mTransform.GetChild(1).GetComponent<Image>();
        mRemainTxt = mTransform.GetChild(2).GetComponent<TextMeshProUGUI>();
        keyMapping = mTransform.GetChild(3).GetComponent<TextMeshProUGUI>().text;
        mRemainItmNumTxt = mTransform.GetChild(4).GetComponent<TextMeshProUGUI>();
    }

    public void coolTimeReset()
    {
        mRemainTime = 0;
        mMaskImg.fillAmount = 0;
        mRemainTxt.gameObject.SetActive(false);
    }

    public void coolTimeReduce(int reduceValue)
    {
        if (mRemainTime - reduceValue > 0)
        {
            mRemainTime -= reduceValue;
            mRemainTxt.text = ((int)mRemainTime).ToString();
            mMaskImg.fillAmount = mRemainTime / mReloadTime;
        }
        else coolTimeReset();
    }

    public void SkillImgReset()
    {
        mSkillImg.color = Color.white;
    }

    public void UseCombo(int comboStack)
    {
        if (comboStack == 0)
            mSkillImg.color = Color.red;
        else if (comboStack==1)
            mSkillImg.color = Color.blue;
    }

    public bool IsUseSkill() { return mRemainTime == 0; }

    public bool UseSkill()
    {
        mRemainTime = mReloadTime;
        mRemainTxt.text = mRemainTime.ToString();
        mRemainTxt.gameObject.SetActive(true);
        mMaskImg.fillAmount = 1;
        return (mMaintainTime > 0);
    }

    public void updateSkillContainer()
    {
        if (mRemainTime > 0)
        {
            mRemainTime -= Time.deltaTime;
            mRemainTxt.text = ((int)mRemainTime).ToString();
            mMaskImg.fillAmount -= 1 / mReloadTime * Time.deltaTime;
        }
        else
        {
            coolTimeReset();
        }
    }

    public void UpdateItmNum(InGameItem itm)
    {
        mRemainItmNumTxt.text = (itm.count).ToString();
    }

}