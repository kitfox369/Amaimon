using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAutoDestroy : MonoBehaviour
{
    public float particleTime;
    float particleTimer;
    bool isTimerOn = false;
    public float delayTime;
    public float delayedActiveTime;
    public GameObject delayedActiveObj;
    private CollisionSkill collisionSkill;

    public void setInfo(float particleTime, float delayTime)
    {
        this.particleTime = particleTime; 
        this.delayTime = delayTime;
        this.enabled= true;
    }

    private void Update()
    {
        if(isTimerOn)
            particleTimer += Time.deltaTime;
    }

    private void OnEnable()
    {
        if(this.GetComponent<CollisionSkill>()!=null) collisionSkill = this.GetComponent<CollisionSkill>();
        StartCoroutine(CoCheckAlive());
    }

    IEnumerator CoCheckAlive()
    {
        while (true)
        {
            yield return new WaitForSeconds(delayTime);
            isTimerOn = true;
            if (delayedActiveObj)
            {
                if (delayedActiveTime <= particleTimer)
                {
                    delayedActiveObj.SetActive(true);
                    if (collisionSkill != null) collisionSkill.ActiveCheckColllision();
                }
            }
            if (particleTime<= particleTimer)
            {
                Destroy(this.gameObject);

                break;
            }
        }
    }
}
