using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class WolfBoss : BossEntity
{
    // Start is called before the first frame update

    public Transform initialPos;
    Transform goalPos;
    public Rigidbody rBody;
    public LayerMask whatIsTarget; //추적대상 레이어

    private LivingEntity targetEntity;//추적대상
    private NavMeshAgent pathFinder; //경로 계산 AI 에이전트

    [Header("[Sound]")]
    public AudioClip deathSound;//사망 사운드
    public AudioClip hitSound; //피격 사운드
    public AudioClip roarSound;
    public AudioClip howlSound;
    public AudioClip biteSound;
    AudioSource AudioSrc;

    private Animator enemyAnimator;
    float animSpeed = 1.0f;

    private Collider[] enemies;

    public float damage = 20f; //공격력
    public float attackDelay = 100f; //공격 딜레이
    private float lastAttackTime; //마지막 공격 시점
    private float dist; //추적대상과의 거리
    private float searchRadius = 50f;

    public Transform tr;

    private float attackRange = 3f;
    float mDamage;
    float moveSpeed = 10.0f,rotSpeed = 30.0f,rushSpeed=8.0f;
    float searchTimer = 0f;
    float searchTime = 10.0f;

    float stunTime;

    [Header("[Skill]")]
    public int rushParam=7;
    bool isRushPos = false;
    Vector3 rushPos;
    float generalDamage=10f;

    float electricSkillTime = 3f;
    bool isSpawn = false;
    float skillRangeRadius = 20f;

    public GameObject knockBackFX;

    Vector3 moveDir;
    Vector3 mAttackDir;

    enum animationState
    {
        IDLE, RUN, ATTACK, HIT,WAIT, SKILL, RUSH, KNOCKBACK, STUN, DEATH
    };
    animationState animState = animationState.IDLE;
    public enum BossState
    {
        WAIT, SKILL, RUSH,ATTACk, HIT,STUN,KNOCKBACK, DEATH
    };
    public BossState mBossState = BossState.WAIT;


    [Header("▶ Skill")]
    public GameObject electricPrefab;

    //추적 대상이 존재하는지 알려주는 프로퍼티
    private bool hasTarget
    {
        get
        {
            //추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            if (targetEntity != null && !targetEntity.bDead)
            {
                return true;
            }

            //그렇지 않다면 false
            return false;
        }
    }

    private bool canMove;
    private bool canAttack=true;

    private void Awake()
    {
        pathFinder = GetComponent<NavMeshAgent>();
        pathFinder.stoppingDistance = attackRange-1f;
        pathFinder.speed = moveSpeed;
        enemyAnimator = transform.GetChild(0).GetComponent<Animator>();
        rBody = transform.GetChild(0).GetComponent<Rigidbody>();
        AudioSrc = this.GetComponent<AudioSource>();
        mID = monsterID;
        mKind = monsterKind;
    }

    void Start()
    {
        tr = transform.GetChild(0).GetComponent<Transform>();
        rushPos = tr.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (mIntro){introAction();}
        else
        {
            if (onDeath)
            {
                animSpeed = 0.5f;
                enemyAnimator.SetFloat("animSpeed", animSpeed);
            }
            else if (!bIsStun) {
                if (pathFinder.enabled)
                {
                    searchTimer += Time.deltaTime;
                    if (mBossState == BossState.WAIT)
                    {
                        animState = animationState.WAIT;
                        //타겟과 너무 가까우면 넉백 시전
                        if (checkDistance(targetEntity.transform.position) <= attackRange*1.5f)
                        {
                            knockback();
                        }
                        else
                        {
                            if (searchTime < searchTimer) { skillSelect(); }
                            if (targetEntity != null)
                            {
                                //추적 대상 바라보기
                                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetEntity.transform.position - transform.position), Time.deltaTime * rotSpeed);
                            }
                        }
                    }
                    else
                    {
                        if (mBossState == BossState.RUSH) { rush(); }
                        else if (mBossState == BossState.SKILL) { skill(); }
                        else if (mBossState == BossState.KNOCKBACK) { StartCoroutine("BackToIdleState", 5.0f); }
                    }
                    enemyAnimator.SetInteger("EnemyState", (int)animState);
                }
            }
            else
            {
                searchTimer += Time.deltaTime;
                if (searchTimer > stunTime)
                {
                    bIsStun = false;
                    mBossState = BossState.WAIT;
                    animState = animationState.WAIT;
                }
            }
        }
    }

    void introAction()
    {
        enemyAnimator.SetInteger("EnemyState", -1);
        enemyAnimator.SetBool("bIntro", true);
        if (Vector3.Distance(transform.position, initialPos.position) < 1.0f)
        {
            transform.rotation = Quaternion.LookRotation(initialPos.right);
            enemyAnimator.SetInteger("Intro", 1);
        }
        else
        {
            enemyAnimator.SetInteger("Intro", 0);
            moveDir = initialPos.position - transform.position;
            moveDir.Normalize();
            transform.localPosition += moveDir * Time.deltaTime * moveSpeed;
            transform.rotation = Quaternion.LookRotation(moveDir);
        }
    }

    public void RoarSFX()
    {
        AudioSrc.PlayOneShot(roarSound);
    }

    public void HawlSFX()
    {
        AudioSrc.PlayOneShot(howlSound);
    }

    public void BiteSFX()
    {
        AudioSrc.PlayOneShot(biteSound);
    }

    void skillSelect()
    {
        int action = Random.Range(0, 10);
        if (action <= rushParam) { mBossState = BossState.RUSH; }
        else { mBossState = BossState.SKILL; }
        searchTimer = 0f;
    }

    void rush()
    {
        if (!isRushPos)     //대쉬가 아닐때
        {
            rushPos = targetEntity.transform.position;
            moveDir = rushPos - transform.position;
            moveDir.Normalize();
            isRushPos = true;

        }
        else
        {
            dist = checkDistance(rushPos);

            if (dist <= attackRange)
            {
                if (canAttack)
                {
                    canAttack = false;
                    animState = animationState.ATTACK;
                    mBossState = BossState.ATTACk;
                    enemyAnimator.SetInteger("EnemyState", (int)animState);
                    OnDamageFront();
                    StartCoroutine("BackToIdleState", 2.0f);
                }
            }
            else if (dist > attackRange)
            {
                animState = animationState.RUSH;
                rushToward();
            }
        }
    }

    void skill()
    {
        animState = animationState.SKILL;
        if (!isSpawn)
        {
            for (int i = 0; i < 5; i++)
            {
                float x = Random.Range(-1.0f, 1.0f);
                float temp = Mathf.Pow(1.0f, 2) - Mathf.Pow(x, 2);
                float z = Mathf.Sqrt(temp);
                Vector3 randomVec = new Vector3(x, 0, z) * Random.Range(0.0f, skillRangeRadius);

                GameObject electricSpace = Instantiate(electricPrefab, targetEntity.transform.position + randomVec, Quaternion.identity);
            }
            isSpawn = true;
        }
        if (electricSkillTime < searchTimer)
        {
            mBossState = BossState.WAIT;
            isSpawn = false;
        }
    }

    void knockback()
    {
        knockBackFX.SetActive(true);
        mBossState = BossState.KNOCKBACK;
        animState = animationState.KNOCKBACK;
        enemyAnimator.SetInteger("EnemyState", (int)animState);
        searchTimer = 0f;
    }

    void rushToward()
    {
        pathFinder.SetDestination(rushPos);
    }

   float checkDistance(Vector3 target)
    {
        return Vector3.Distance(transform.position, target);
    }

    void DisableRagdoll()
    {
        rBody.isKinematic = true;
        rBody.useGravity = false;
        rBody.useGravity = true;
        rBody.GetComponent<Collider>().enabled = false;
        rBody.gameObject.transform.localPosition = Vector3.zero;
    }

    public override void StartBattle(LivingEntity player)
    {
        moveSpeed = 100.0f;
        pathFinder.enabled = true;
        DisableRagdoll();
        targetEntity = player;

        base.StartBattle(player);
    }

    public override void OnStun(float stunTime)
    {
        mBossState = BossState.STUN;
        animState = animationState.STUN;
        enemyAnimator.SetInteger("EnemyState", (int)animState);
        this.stunTime = stunTime;
        EffectGenerator.instance.GenerateEffect(2, 0.1f, 0.0f, transform);
        EffectGenerator.instance.GenerateEffectUpSide(5, stunTime, 0.0f, transform.GetChild(1),2f);
        searchTimer = 0f;
        if (knockBackFX.activeInHierarchy) knockBackFX.SetActive(false);

        base.OnStun(stunTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // 보스 캐릭터를 고정시키기
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // X 와 Y 축만 constraints 를 풀어줍니다.
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        }
    }

    public void OnDamageFront()
    {
        enemies = Physics.OverlapBox(transform.position + transform.forward*attackRange, GetComponent<BoxCollider>().size/2
            , Quaternion.identity, whatIsTarget);

        if (enemies.Length > 0)
        {
            LivingEntity livingEntity = enemies[0].GetComponent<LivingEntity>();
            if (livingEntity != null && !livingEntity.bDead)
            {
                livingEntity.OnDamage(generalDamage, 0.1f, transform.forward);
            }
        }
        else
            Debug.Log("NO enemies");

    }

    public void OnDamageEvent()
    {
        //공격 처리
        targetEntity.OnDamage(damage, 0, moveDir);

        //최근 공격 시간 갱신
        lastAttackTime = Time.time;
    }


    //데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage, float delayTime, Vector3 attackDir)
    {
        //사망하지 않을 상태에서만 피격 효과 재생
        if (!bDead)
        {
            //공격 받은 지점과 방향으로 피격 효과 재생
            //hitEffect.transform.position = hitPoint;
            //hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            //hitEffect.Play();
            mAttackDir = attackDir;
            mDamage = damage;
            StartCoroutine("DelayedHitAttack", delayTime);
        }

        //LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage, delayTime, attackDir);
    }

    //보스 사망 처리
    public override void Die()
    {
        //LivingEntity의 DIe()를 실행하여 기본 사망 처리 실행
        base.Die();

        //다른 AI를 방해하지 않도록 자신의 모든 콜라이더를 비활성화
        Collider[] enemyColliders = GetComponents<Collider>();
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = false;
        }

        //AI추적을 중지하고 네비메쉬 컴포넌트를 비활성화
        pathFinder.isStopped = true;
        pathFinder.enabled = false;

        //사망 애니메이션 재생
        StartCoroutine("DelayedDestroy", 5.0f);

    }

    IEnumerator BackToIdleState(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        animState = animationState.WAIT;
        mBossState = BossState.WAIT;
        isRushPos = false;
        canAttack = true;
        if (knockBackFX.activeInHierarchy) knockBackFX.SetActive(false);
    }

    IEnumerator DelayedHitAttack(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        //피격 효과음 재생
        EffectGenerator.instance.GenerateEffect(1, 1.0f, 0, transform,3f);
        EffectGenerator.instance.GenerateDamageTXT(0, 3.0f, 0, mDamage.ToString(), transform);
        AudioSrc.PlayOneShot(hitSound);
        enemyAnimator.SetInteger("EnemyState", 3);
        //밀려나기
        transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + (mAttackDir * 30f), Time.deltaTime);
        if (bDead)
        {
            animState = animationState.DEATH;
            enemyAnimator.SetInteger("EnemyState", (int)animState);
            AudioSrc.PlayOneShot(deathSound);
        }
    }

    IEnumerator DelayedDestroy(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Destroy(this.gameObject);
    }

}
