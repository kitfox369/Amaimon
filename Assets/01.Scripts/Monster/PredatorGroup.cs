using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PredatorGroup : LivingEntity
{
    // Start is called before the first frame update
    public string Name;

    [Header("[Layer]")]
    public LayerMask whatIsTarget; //추적대상 레이어

    private LivingEntity targetEntity;//추적대상
    private NavMeshAgent pathFinder; //경로 계산 AI 에이전트

    /*public ParticleSystem hitEffect; //피격 이펙트
    */
    [Header("[Sound]")]
    public AudioClip deathSound;//사망 사운드
    public AudioClip hitSound; //피격 사운드
    AudioSource AudioSrc;

    private Animator enemyAnimator;
    float animSpeed = 1.0f;
    //private AudioSource enemyAudioPlayer; //오디오 소스 컴포넌트

    public float damage = 20f; //공격력
    public float attackDelay = 1f; //공격 딜레이
    private float lastAttackTime; //마지막 공격 시점
    private float dist; //추적대상과의 거리
    private float distRange = 3f;
    private float searchRadius = 10f;
    private Vector3 distance; //대상과의 거리

    public Transform tr;

    private float attackRange = 2.3f;

    Vector3 moveDir;
    Vector3 mAttackDir;
    float mDamage;
    float moveSpeed = 2.0f;
    float searchTimer = 0f;
    float searchTime = 10.0f;
    float stunTime;

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
    private bool canAttack;

    NavMeshData navData;

    private void Awake()
    {
        //게임 오브젝트에서 사용할 컴포넌트 가져오기
        pathFinder = GetComponent<NavMeshAgent>();
        pathFinder.stoppingDistance = attackRange;
        enemyAnimator = transform.GetChild(0).GetComponent<Animator>();
        AudioSrc = GetComponent<AudioSource>();
    }

    //적 AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(float newHealth, float newDamage, float newSpeed,string name)
    {
        //체력 설정
        startingHealth = newHealth;
        mHealth = newHealth;
        //공격력 설정
        damage = newDamage;
        //네비메쉬 에이전트의 이동 속도 설정
        pathFinder.speed = newSpeed;
        mName = name;
        mID = monsterID;
        mKind = monsterKind;
    }


    void Start()
    {
        tr = GetComponent<Transform>();
        Setup(startingHealth, damage, moveSpeed, Name);
        searchTime = Random.Range(8.0f, 15.0f);
        navData = navSurface.navMeshData;
        pathFinder.enabled = true;
        SetRandomDest(navData.sourceBounds);
        pathFinder.SetDestination(transform.position + distance);
        enemyAnimator.SetInteger("EnemyState", 1);
        //게임 오브젝트 활성화와 동시에 AI의 탐지 루틴 시작
        StartCoroutine(UpdatePath());
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTarget)
        {
            //추적 대상이 존재할 경우 거리 계산은 실시간으로 해야하니 Update()
            dist = Vector3.Distance(tr.position, targetEntity.transform.position);
            if (dist > searchRadius) { targetEntity = null; }
        }
        else
        {
            if ((pathFinder.destination - transform.position).magnitude <= pathFinder.stoppingDistance)
            {
                canMove = false;
                enemyAnimator.SetInteger("EnemyState", 0);
                //임의로 자리 바꾸기
                searchTimer += Time.deltaTime;
                if (searchTimer > searchTime)
                {
                    pathFinder.speed = moveSpeed;
                    enemyAnimator.speed = 1.0f;
                    SetRandomDest(navData.sourceBounds);
                    pathFinder.isStopped = false;
                    pathFinder.SetDestination(transform.position + distance);
                    searchTimer = 0f;
                    canMove = true;
                    enemyAnimator.SetInteger("EnemyState", 1);
                }
            }
        }

        //hit 없애기
        if (bIsHit)
        {
            hitTimer += Time.deltaTime;
            if (hitTimer > 1.0f)
            {
                bIsHit = false;
            }
        }
    }

    Vector3 SetRandomDest(Bounds bounds)
    {
        var x = Random.Range(bounds.min.x, bounds.max.x);
        var z = Random.Range(bounds.min.z, bounds.max.z);

        distance = new Vector3(x, transform.position.y, z);

        //target.position = dist;
        return distance;
    }

    bool checkTargetInArea(Bounds bounds, LivingEntity livingEntity,Vector3 areaTrans)
    {
        if (livingEntity.transform.position.x>= areaTrans.x+bounds.min.x&& livingEntity.transform.position.x <= areaTrans.x +bounds.max.x
            && livingEntity.transform.position.z >= areaTrans.z + bounds.min.z && livingEntity.transform.position.z<= areaTrans.z + bounds.max.z)
            return true;
        else return false;
    }


    //추적할 대상의 위치를 주기적으로 찾아 경로 갱신
    private IEnumerator UpdatePath()
    {
        //살아 있는 동안 무한 루프
        while (!bDead)
        {
            if (onDeath)
            {
                animSpeed = 0.5f;
                enemyAnimator.SetFloat("animSpeed", animSpeed);
            }
            else if (!bIsStun)
            {
                if (hasTarget)
                {
                    Attack();
                }
                else
                {

                    //추적 대상이 없을 경우, Idle 상태 돌입
                    //pathFinder.isStopped = true;
                    canAttack = false;

                    //반지름 1f의 콜라이더로 whatIsTarget 레이어를 가진 콜라이더 검출하기
                    Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadius, whatIsTarget);

                    //모든 콜라이더를 순회하면서 살아 있는 LivingEntity 찾기
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        if (colliders[i].gameObject.tag.Equals("Player"))
                        {
                            //콜라이더로부터 LivingEntity 컴포넌트 가져오기
                            LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                            //LivingEntity 컴포넌트가 존재하며, 해당 LivingEntity가 살아 있다면
                            if (livingEntity != null && !livingEntity.bDead && checkTargetInArea(navData.sourceBounds, livingEntity, transform.parent.position))
                            {
                                //추적 대상을 해당 LivingEntity로 설정
                                targetEntity = livingEntity;
                                searchTimer = 0f;
                                //for문 루프 즉시 정지
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                searchTimer += Time.deltaTime;
                if (searchTimer > stunTime)
                {
                    bIsStun = false;
                }
            }

            //0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    //추적 대상과의 거리에 따라 공격 실행
    public virtual void Attack()
    {
        //자신이 사망X, 추적 대상과의 거리이 공격 사거리 안에 있다면
        if (!bDead && dist <= attackRange)
        {
            //공격 반경 안에 있으면 움직임을 멈춘다.
            canMove = false;

            //추적 대상 바라보기
            this.transform.LookAt(targetEntity.transform);

            //최근 공격 시점에서 attackDelay 이상 시간이 지나면 공격 가능
            if (lastAttackTime + attackDelay <= Time.time)
            {
                canAttack = true;
                OnDamageEvent();
                enemyAnimator.SetInteger("EnemyState", 2);
            }
            else   //공격 반경 안에 있지만, 딜레이가 남아있을 경우
            {
                canAttack = false;
                enemyAnimator.SetInteger("EnemyState", 0);
            }
        }
        else  //공격 반경 밖에 있을 경우 추적하기
        {
            enemyAnimator.SetInteger("EnemyState", 1);
            canMove = true;
            canAttack = false;
            //계속 추적
            pathFinder.isStopped = false; //계속 이동
            pathFinder.SetDestination(targetEntity.transform.position);
        }
    }

    //유니티 애니메이션 이벤트로 휘두를 때 데미지 적용시키기
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

            //LivingEntity의 OnDamage()를 실행하여 데미지 적용
            base.OnDamage(damage, delayTime, attackDir);

            HitAttack();
        }

        //피격 애니메이션 재생
        //enemyAnimator.SetTrigger("Hit");
    }

    public override void OnStun(float stunTime)
    {
        enemyAnimator.SetInteger("EnemyState", 4);
        this.stunTime = stunTime;
        EffectGenerator.instance.GenerateEffect(2, 0.1f, 0.0f, transform);
        EffectGenerator.instance.GenerateEffectUpSide(5, stunTime, 0.0f, transform);
        searchTimer = 0f;

        base.OnStun(stunTime);
    }

    //사망 처리
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
        //enemyAnimator.SetTrigger("Die");
        StartCoroutine("DelayedDestroy", 5.0f);

    }

    void HitAttack()
    {
        //피격 효과음 재생
        EffectGenerator.instance.GenerateEffect(1, 1.0f, 0, transform);
        EffectGenerator.instance.GenerateDamageTXT(0, 1.0f, 0, mDamage.ToString(), transform);
        AudioSrc.PlayOneShot(hitSound);
        enemyAnimator.SetInteger("EnemyState", 3);
        //밀려나기
        transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + (mAttackDir * 30f), Time.deltaTime);
        if (bDead)
        {
            enemyAnimator.SetInteger("EnemyState", 5);
            AudioSrc.PlayOneShot(deathSound);
        }
    }

    IEnumerator DelayedDestroy(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Destroy(this.gameObject);
    }
}
