using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventBoss : MonoBehaviour
{
    WolfBoss boss;

    private void Awake()
    {
        boss = transform.parent.GetComponent<WolfBoss>();
    }

    public void OnRoarSFX()
    {
        boss.RoarSFX();
    }

    public void OnHawlSFX()
    {
        boss.HawlSFX();
    }

    public void OnBiteSFX()
    {
        boss.BiteSFX();
    }
}
