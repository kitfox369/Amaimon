using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;

public class MonsterManager : MonoBehaviour
{
    public Transform[] monsterParentTrans;
    Transform[] mTempMonsters;

    public GameObject[] monsterPrefab;

    public List<MonsterGroup> monsters;
    public Vector3[] mMonsterZone;
    public NavMeshSurface[] navMeshSurfaces;

    public BossMapInfo[] bossMap;
    public Transform[] bossParentTrans;
    public Vector3[] mBossZone;

    BossMapInfo currentBoss;

    public UIManager uiM;

    public float spawnTime;
    float spawnTimer;

    RewardPackage bossPackage;

    [Header("¢º Enemy Info")]
    public GameObject mEnemyInfo;
    public Transform mEnemyHPParent;

    // Start is called before the first frame update
    void Start()
    {
        monsters =new List<MonsterGroup>() ;
        mMonsterZone = new Vector3[monsterParentTrans.Length];
        for (int i=0;i<monsterParentTrans.Length;i++)
        {
            mTempMonsters = new Transform[monsterParentTrans[i].childCount-1];
            for (int j = 1; j <= mTempMonsters.Length; j++) { mTempMonsters[j-1] = monsterParentTrans[i].GetChild(j); }
            monsters.Add(new MonsterGroup(mTempMonsters));
            mMonsterZone[i] = monsterParentTrans[i].GetChild(0).localScale;
        }

        mBossZone = new Vector3[bossParentTrans.Length];
        for (int i = 0; i < bossParentTrans.Length; i++)
        {
            mBossZone[i] = bossParentTrans[i].GetChild(0).localScale;
        }


    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer > spawnTime)
        {
            spawnTimer = 0;
            spawnMonsters();
        }

        if(currentBoss!=null&&currentBoss.mBossInfo.bDead && !currentBoss.completed)
        {
            bossPackage = currentBoss.GetRewardPakage();
            currentBoss.EndBattle();
            currentBoss = null;
            uiM.DeactiveBossState();
            uiM.ActiveRewardUI(bossPackage);
        }

        updateMonsterHPBar();
    }

    private void spawnMonsters()
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            for (int j = 0; j < monsters[i].monsters.Length; j++)
            {
                if (monsters[i].monsters[j] == null)
                {
                    GameObject questObj = Instantiate(monsterPrefab[i], Vector3.zero, Quaternion.identity);
                    questObj.transform.parent = monsterParentTrans[i];
                    monsters[i].SetOriginPosition(j,questObj);
                    monsters[i].monsters[j] = questObj.transform;
                    monsters[i].monsterEntity[j] = questObj.GetComponent<LivingEntity>();
                    monsters[i].monsterEntity[j].navSurface = navMeshSurfaces[monsters[i].monsterEntity[j].mID];
                }
            }
        }
    }

    void updateMonsterHPBar()
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            for (int j = 0; j < monsters[i].monsters.Length; j++)
            {
                if (monsters[i].monsters[j] != null&&monsters[i].monsterHPBar[j] != null)
                {
                    if (monsters[i].monsterEntity[j].bIsHit)
                    {
                        monsters[i].monsterHP[j].position = Camera.main.WorldToScreenPoint(monsters[i].monsters[j].position) + Vector3.up * 100f;
                        monsters[i].UpdateHPImage(j);
                    }
                    else
                    {
                        Destroy(monsters[i].monsterHP[j].gameObject);
                        monsters[i].RemoveHPImage(j);
                    }
                }
                else if(monsters[i].monsters[j] == null&&monsters[i].monsterHPBar[j] != null)
                {
                    Destroy(monsters[i].monsterHP[j].gameObject);                    
                }
                else if(monsters[i].monsters[j] != null && monsters[i].monsterHPBar[j] == null)
                {
                    if (monsters[i].monsterEntity[j].bIsHit)
                    {
                        GameObject hpBar = Instantiate(mEnemyInfo, Vector3.zero, Quaternion.identity);
                        hpBar.transform.parent = mEnemyHPParent;
                        monsters[i].AddHPImage(j, hpBar);
                    }
                }
            }
        }
    }

    public void EnterBossZone(int idx)
    {
        currentBoss = bossMap[idx];
    }
}

[System.Serializable]
public class MonsterGroup
{
    public Transform[] monsters;
    Vector3[] originPos;
    public LivingEntity[] monsterEntity;
    public Transform[] monsterHP;
    public Image[] monsterHPBar;
    public bool bIsBoss;

    public MonsterGroup(Transform[] mon, bool bIsBoss=false)
    {
        monsters = mon;
        originPos = new Vector3[mon.Length];
        monsterEntity = new LivingEntity[mon.Length];
        monsterHP = new Transform[mon.Length];
        monsterHPBar = new Image[mon.Length];
        for (int i=0;i< mon.Length; i++)
        {
            originPos[i] = mon[i].localPosition;
            monsterEntity[i] = mon[i].GetComponent<LivingEntity>();
        }
        this.bIsBoss = bIsBoss;
    }

    public void SetOriginPosition(int idx,GameObject prefab)
    {
        prefab.transform.localPosition = originPos[idx];
    }

    public void UpdateHPImage(int idx)
    {
        monsterHPBar[idx].fillAmount = monsterEntity[idx].mHealth / (float)monsterEntity[idx].startingHealth;
    }

    public void AddHPImage(int idx, GameObject hpBar)
    {
        monsterHP[idx] = hpBar.transform;
        monsterHP[idx].GetChild(1).GetComponent<TextMeshProUGUI>().text = monsterEntity[idx].mName;
        monsterHPBar[idx] = hpBar.transform.GetChild(0).GetChild(0).GetComponent<Image>() ;

    }

    public void RemoveHPImage(int idx)
    {
        monsterHPBar[idx] = null;
        monsterHP[idx] = null;
    }
}
