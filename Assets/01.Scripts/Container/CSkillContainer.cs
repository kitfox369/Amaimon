using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CSkillContainer
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
        else if (comboStack == 1)
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
