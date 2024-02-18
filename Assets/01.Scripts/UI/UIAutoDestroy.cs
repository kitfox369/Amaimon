using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAutoDestroy : MonoBehaviour
{
    public float activeTime;
    float activeTimer;
    bool isTimerOn = false;
    public float delayTime;
    public float delayedActiveTime;
    bool bGlitterEffect;
    float glitterParam=1f;
    Image mImg;
    TextMeshProUGUI mText;

    [System.Serializable]
    public enum EffectMode
    {
        GENERAL, FADING
    }
    public EffectMode mode;

    public enum Type
    {
        IMAGE,TEXTMESH
    }
    public Type type;

    public void setInfo(float particleTime, float delayTime,bool bGlitterEffect)
    {
        this.activeTime = particleTime;
        this.delayTime = delayTime;
        this.bGlitterEffect= bGlitterEffect;
        this.enabled = true;
    }

    private void Update()
    {
        if (isTimerOn)
            activeTimer += Time.deltaTime;
    }

    private void OnEnable()
    {
        if (this.GetComponent<Image>() != null) mImg = this.GetComponent<Image>();
        if (this.GetComponent<TextMeshProUGUI>() != null) mText = this.GetComponent<TextMeshProUGUI>();
        StartCoroutine(CoCheckAlive());
    }

    IEnumerator CoCheckAlive()
    {
        while (true)
        {
            yield return new WaitForSeconds(delayTime);
            isTimerOn = true;
            if(mode== EffectMode.FADING&& type== Type.TEXTMESH)
            {
                if (activeTime - activeTimer < 1f)
                {
                    glitterParam -= Time.deltaTime;
                    mText.color = new Color(mText.color.r, mText.color.g, mText.color.b, glitterParam);
                }
            }
            if (bGlitterEffect)
            {
                if (activeTime-activeTimer<5f)
                {
                    if (glitterParam > 0.5f) glitterParam -= Time.deltaTime;
                    else glitterParam = 1f;
                    mImg.color = new Color(mImg.color.r, mImg.color.g, mImg.color.b, glitterParam);
                }
            }
            if (activeTime <= activeTimer)
            {
                Destroy(this.gameObject);
                break;
            }
        }
    }

}
