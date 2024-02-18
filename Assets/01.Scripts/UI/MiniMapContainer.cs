using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapContainer : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera mMiniMapCam;
    public Transform mRootMap;
    Transform[] mChildMap;
    public RenderTexture mRenderTexture;
    public Sprite mMinimapSprite;
    public Image mMiniMapImg;
    public Transform player;
    public Transform mPlayerTrans;
    public Image mPlayerImg;
    public NPCManager npcM;
    public Transform mQuestIconParent;
    public GameObject mQuestPrefab;
    public Sprite[] mQuestImg;
    public Transform mShopIconParent;
    public GameObject mShopPrefab;
    public Sprite[] mShopImg;

    public Transform mQuestAreaParent;
    public GameObject mQuestAreaPrefab;

    float MaxZoom, MinZoom;

    public Vector2 margin;

    private void Awake()
    {
        mChildMap = new Transform[mRootMap.childCount];
        for (int i = 0; i < mRootMap.childCount; i++)
        {
            mChildMap[i] = mRootMap.GetChild(i);
        }
    }

    void Start()
    {
        MaxZoom = 5.0f;
        MinZoom = 1.0f;
        mMiniMapCam.Render();

        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = mMiniMapCam.targetTexture;

        Texture2D Image = new Texture2D(mMiniMapCam.targetTexture.width, mMiniMapCam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, mMiniMapCam.targetTexture.width, mMiniMapCam.targetTexture.height), 0, 0);
        Image.Apply();

        var Bytes = Image.EncodeToPNG();
        

        File.WriteAllBytes(Application.dataPath + "/05.Images/Minimap.png", Bytes);

        mMinimapSprite = Sprite.Create(Image,new Rect(0,0, mMiniMapCam.targetTexture.width, mMiniMapCam.targetTexture.height),Vector2.zero);

        mMiniMapImg.sprite = mMinimapSprite;

        Destroy(activeRenderTexture);

        margin = new Vector2(mPlayerTrans.transform.localPosition.x/player.transform.position.x , mPlayerTrans.transform.localPosition.y/player.transform.position.z);
    }

    private void Update()
    {
        mPlayerTrans.transform.localPosition = new Vector2(player.transform.position.x, player.transform.position.z)*margin;
        mPlayerTrans.transform.localRotation = Quaternion.Euler(0, 0, -player.transform.eulerAngles.y);
    }

    public void InitializeShopIcon()
    {
        for (int i = 0; i < npcM.ShopNPCCount; i++)
        {
            GameObject shop = Instantiate(mShopPrefab, Vector3.zero, Quaternion.identity);
            shop.GetComponent<Image>().sprite = mShopImg[npcM.GetShopKind(i)];

            shop.transform.parent = mShopIconParent;

            Vector2 npcXY = new Vector2(npcM.getShopNPCTrans(i).localPosition.x, npcM.getShopNPCTrans(i).localPosition.z);
            //위치 설정
            shop.transform.localPosition = npcXY * margin;


        }
    }

    public void InitializeQuestIcon()
    {
        for(int i = 0; i < npcM.NpcCount; i++)
        {
            GameObject questIcon = Instantiate(mQuestPrefab, Vector3.zero, Quaternion.identity);
            if (npcM.GetQuestState(i) == 0)
            {
                questIcon.GetComponent<Image>().sprite = mQuestImg[0];
            }
            else
            {
                questIcon.GetComponent<Image>().sprite = mQuestImg[1];
            }
            questIcon.transform.parent = mQuestIconParent;

            Vector2 npcXY = new Vector2(npcM.getNPCTrans(i).localPosition.x, npcM.getNPCTrans(i).localPosition.z);
            //위치 설정
            questIcon.transform.localPosition = npcXY * margin;


        }
    }

    public void UpdateQuestIcon(int index)
    {
        int questState = npcM.GetQuestState(index);
        if (questState == 0)
        {
            mQuestIconParent.GetChild(index).GetComponent<Image>().sprite = mQuestImg[0];
        }
        else if (questState == 1)
        {
            mQuestIconParent.GetChild(index).gameObject.SetActive(false);
        }
        else if (questState == 2)
        {
            mQuestIconParent.GetChild(index).gameObject.SetActive(true);
            mQuestIconParent.GetChild(index).GetComponent<Image>().sprite = mQuestImg[1];
        }
    }

    public void AddQuestIcon(int idx)
    {
        GameObject questIcon = Instantiate(mQuestPrefab, Vector3.zero, Quaternion.identity);
        if (npcM.GetQuestState(idx) == 0)
        {
            questIcon.GetComponent<Image>().sprite = mQuestImg[0];
        }
        else
        {
            questIcon.GetComponent<Image>().sprite = mQuestImg[1];
        }
        questIcon.transform.parent = mQuestIconParent;

        Vector2 npcXY = new Vector2(npcM.getNPCTrans(idx).localPosition.x, npcM.getNPCTrans(idx).localPosition.z);
        //위치 설정
        questIcon.transform.localPosition = npcXY * margin;

    }

    public void DisableQuestIcon(int idx)
    {
        mQuestIconParent.GetChild(idx).gameObject.SetActive(false);
    }

    public void AddQuestArea(Transform areaTrans,Vector3 scale)
    {
        GameObject questIcon = Instantiate(mQuestAreaPrefab, Vector3.zero, Quaternion.identity);
        
        questIcon.transform.parent = mQuestAreaParent;
        Vector2 areaXY = new Vector2(areaTrans.localPosition.x, areaTrans.localPosition.z);
        //위치 설정
        questIcon.transform.localPosition = areaXY * margin;
        questIcon.transform.localScale = scale;
    }

    public void DisableQuestArea(int idx)
    {
        Destroy(mQuestAreaParent.GetChild(idx).gameObject);
    }

    public void ZoomInOutMap(float scroll)
    {
        Vector3 xyScale = Vector3.one - Vector3.forward;
        float scrolledScale = mMiniMapImg.transform.localScale.x + scroll;
        if (scrolledScale < MaxZoom && scrolledScale >= MinZoom)
        {
            mMiniMapImg.transform.localScale += xyScale * scroll;
        }
        else if (scrolledScale >= MaxZoom) { mMiniMapImg.transform.localScale = xyScale * MaxZoom; }
        else if (scrolledScale < MinZoom) { mMiniMapImg.transform.localScale = xyScale; }
    }
}
