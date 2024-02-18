using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMapInfo : MonoBehaviour
{
    public bool completed = false;
    public BossEntity mBossInfo;
    public int BossIdx;
    public Transform bossStartPos;
    public Transform bossInitialPos;
    public Transform playerInitialPos;
    public GameObject blockade;
    public LivingEntity player;
    public GameManager gm;
    public AudioClip bossFieldBGM;
    public RewardPackage reward;
    // Start is called before the first frame update

    public void startBattle(LivingEntity player)
    {
        mBossInfo.StartBattle(player);
        gm.ChangeBGM(bossFieldBGM);
        gm.SetActiveBGM(true);
    }

    public void EndBattle()
    {
        completed = true;
        blockade.SetActive(false);
        gm.BackToPriorBGM();
        gm.SetActiveBGM(true);
    }

    public RewardPackage GetRewardPakage()
    {
        return reward;
    }

}
