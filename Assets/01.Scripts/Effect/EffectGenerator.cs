using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EffectGenerator : MonoBehaviour
{
    public GameObject[] effectPrefab;
    public GameObject[] damageEffectsPrefab;
    public static EffectGenerator instance = null;

    public struct Effect
    {
        public int effectIndex;
        public float effectTime;
        public float delayTime;
        public Transform parent;
        public float effectScale;

        public Effect(int Index, float Time, float delay, Transform par,float effectScl = 1f)
        {
            effectIndex = Index;
            effectTime = Time;
            delayTime = delay;
            parent = par;
            effectScale = effectScl;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void GenerateEffect(int effectIndex,float effectTime,float delayTime,Transform parent,float effectScale=1f)
    {
        Effect effect = new Effect(effectIndex, effectTime, delayTime, parent, effectScale);
        StartCoroutine(StartEffect(effect));
    }

    public void GenerateEffectUpSide(int effectIndex, float effectTime, float delayTime, Transform parent, float effectScale = 1f)
    {
        Effect effect = new Effect(effectIndex, effectTime, delayTime, parent, effectScale);
        GameObject effectObj = Instantiate(effectPrefab[effect.effectIndex], effect.parent);
        effectObj.transform.position += Vector3.up;
        effectObj.transform.localScale = Vector3.one * effect.effectScale;
        effectObj.AddComponent<ParticleAutoDestroy>();
        effectObj.GetComponent<ParticleAutoDestroy>().setInfo(effect.effectTime, effect.delayTime);
    }

    public void GenerateDamageTXT(int effectIndex, float effectTime, float delayTime,string text, Transform parent)
    {
        GameObject effect = Instantiate(damageEffectsPrefab[effectIndex], parent);
        effect.transform.position += Vector3.up;
        effect.transform.rotation = Quaternion.LookRotation(parent.transform.position-Camera.main.transform.position);
        effect.GetComponent<TextMeshPro>().text = text; ;
        effect.AddComponent<ParticleAutoDestroy>();
        effect.GetComponent<ParticleAutoDestroy>().setInfo(effectTime, delayTime);
    }

    private IEnumerator StartEffect(Effect effectStruct)
    {
        yield return new WaitForSeconds(effectStruct.delayTime);
        GameObject effect = Instantiate(effectPrefab[effectStruct.effectIndex], effectStruct.parent);
        effect.transform.localScale = Vector3.one * effectStruct.effectScale;
        effect.AddComponent<ParticleAutoDestroy>();
        effect.GetComponent<ParticleAutoDestroy>().setInfo(effectStruct.effectTime, effectStruct.delayTime);
    }

}
