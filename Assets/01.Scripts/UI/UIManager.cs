using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : BaseBehaviour
{

    public GameManager GameManager;
    public UserPlayerCtrl playerCtrl;

    [Header("▶ Status")]
    public Image mHPGauge;
    public Image mRunGauge;

    [Header("▶ Comment")]
    [SerializeField] private CommentUI _commentUI;
    [SerializeField] private CommentTipUI _commentTipUI;


    [Header("▶ TextReader")]
    [SerializeField] private TextReader _txtReader;

    private int interactNPCIdx;

    [Header("▶ Shop")]
    // Start is called before the first frame update
    public Transform shopWindow;
    public Transform shopWindowContent;
    public GameObject stuffConPrefab;
    public GameObject stuffBuyWindow;
    public GameObject shopNoticeWindow;
    public GameObject maskImg;
    private StuffInfo stuffInCart;
    public TextMeshProUGUI myMoneyTxt;

    [Header("[ItemDB]")]
    public ItemDataList itmDB;
    public Sprite goldImg;
    private RewardPackage tempReward;

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
    private float fadeValue = 0.0f;
    private float appliedTime = 2.0f;

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



    [Header("▶ Skill Window")]
    public Transform mSkillWindow;
    public CSkillContainer[] mSkill;

    [Header("▶ Buff Window")]
    public Transform mBuffContainer;
    public GameObject mBuffPrefab;

    private void Awake()
    {
        //Skill 리셋
        for (int i = 0; i < mSkill.Length; i++)
        {
            mSkill[i].intialize();
            mSkill[i].coolTimeReset();
        }
    }
    void Start()
    {
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
        questObj.transform.GetChild(0).GetComponent<Text>().text = _txtReader.currentCom.QuestDescription;
        if (_txtReader.currentCom.Type.mKind == 1)
        {
            questObj.transform.GetChild(1).GetComponent<Text>().text = "(0/"+ _txtReader.currentCom.Type.mHuntMonNum.ToString()+")";
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
        return _commentUI.IsEndComment;
    }

    public void startNPCComment(int npcNum,Comment com,int questState)         //움직일 수 있는지 없는지 반환
    {
        _txtReader.startNPCComment(com, questState);
        _commentUI.startNPCComment();

    }

    public void startNPCComment(int npcNum, ShopComment com, int questState)         //움직일 수 있는지 없는지 반환
    {
        _txtReader.startNPCComment(com, questState);
        _commentUI.startNPCComment();
        shopWindow.gameObject.SetActive(true);
    }

    public void endNPCComment()
    {
        _commentUI.endNPCComment();           //추후 모든 말이 끝났을때로 변경해야함
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
        _commentUI.endNPCComment();
        shopWindow.gameObject.SetActive(false);
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
        _commentTipUI.activeCommentTip(npcName);
    }

    public void disableCommentTip()
    {
        interactNPCIdx = -1;
        if (_commentTipUI.gameObject.activeSelf)
            _commentTipUI.gameObject.SetActive(false);
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

#if UNITY_EDITOR
    protected override void OnBindField()
    {
        base.OnBindField();
        mMiniMapContainer = transform.GetChild(0).GetComponent<MiniMapContainer>();

        _commentUI = GameObject.FindAnyObjectByType<CommentUI>();
        _commentTipUI = GameObject.FindAnyObjectByType<CommentTipUI>();

        _txtReader = GetComponent<TextReader>();
        _txtReader.CommentUI = _commentUI;

        mMapInfoShadow = mMapInfo.GetChild(0).GetComponent<Text>();
        mMapInfoText = mMapInfo.GetChild(1).GetComponent<Text>();

        //mBossIntroTxtShadow = mBossIntroInfo.GetChild(0).GetComponent<Text>();
        mBossIntroTxt = mBossIntroInfo.GetChild(0).GetComponent<Text>();

        mBossStateName = mBossStateInfo.GetComponent<Text>();
    }

#endif
}