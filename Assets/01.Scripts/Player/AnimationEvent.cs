using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    UserPlayerCtrl player;

    private void Awake()
    {
        player = transform.parent.GetComponent<UserPlayerCtrl>();
    }

    public void OnDamageFront()
    {
        player.OnDamageFront();
    }

    public void OnDamageRange()
    {
        player.OnDamageRange();
    }

    public void OnDamageStun()
    {
        player.OnDamageStun();
    }

    public void OnBuffSkill()
    {

    }

    public void DetectEnemies(int type)
    {
        player.DetectEnemies(type);
    }

    public void BackToIdle()
    {
        player.BackToIdle();
    }

}
