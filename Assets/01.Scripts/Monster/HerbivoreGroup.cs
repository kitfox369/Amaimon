using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HerbivoreGroup : LivingEntity
{
    // Start is called before the first frame update
    public string Name;

    [Header("[Layer]")]
    public LayerMask whatIsTarget; //추적대상 레이어

    private Transform target;
    private NavMeshAgent pathFinder; //경로 계산 AI 에이전트

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
    private Vector3 dist; //대상과의 거리
    private float distRange=5f;

    public Transform tr;

    private float attackRange = 0.1f;

    Vector3 moveDir;
    Vector3 mAttackDir;
    float mDamage;
    float moveSpeed=0.5f;
    float searchTimer = 0f;
    float searchTime = 10.0f;

    private bool canMove;
    private bool canAttack;
    bool isHitted = false;

    NavMeshData navData;

    private void Awake()
    {
        pathFinder = GetComponent<NavMeshAgent>();
        pathFinder.stoppingDistance = attackRange;
        //게임 오브젝트에서 사용할 컴포넌트 가져오기
        enemyAnimator = transform.GetChild(0).GetComponent<Animator>();
        AudioSrc = GetComponent<AudioSource>();
    }

    //적 AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(float newHealth, float newDamage, float newSpeed, string name)
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
        //게임 오브젝트 활성화와 동시에 AI의 탐지 루틴 시작
        tr = GetComponent<Transform>();
        Setup(startingHealth, damage, moveSpeed,Name);
        searchTime = Random.Range(8.0f, 15.0f);
        navData = navSurface.navMeshData;
        pathFinder.enabled = true;
        SetRandomDest(navData.sourceBounds);
        pathFinder.SetDestination(dist);
    }

    // Update is called once per frame
    void Update()
    {
        enemyAnimator.SetBool("CanMove", canMove);

        if (onDeath)
        {
            animSpeed = 0.5f;
            enemyAnimator.SetFloat("animSpeed", animSpeed);
        }
        else if (isHitted)
        {
            RunAway();
        }
        else
        {
            if ((pathFinder.destination - transform.position).magnitude <= pathFinder.stoppingDistance)
            {
                canMove = false;
                //임의로 자리 바꾸기
                searchTimer += Time.deltaTime;
                if (searchTimer > searchTime)
                {
                    pathFinder.speed = moveSpeed;
                    enemyAnimator.speed = 1.0f;
                    SetRandomDest(navData.sourceBounds);
                    pathFinder.isStopped = false;
                    pathFinder.SetDestination(transform.position + dist);
                    searchTimer = 0f;
                    searchTime = Random.Range(8.0f, 15.0f);
                    canMove = true;
                }
            }
        }

        //hit 없애기
        if (bIsHit)
        {
            hitTimer+=Time.deltaTime;
            if (hitTimer > 5.0f)
            {
                bIsHit = false;
            }
        }
    }

    //추적 대상과의 거리에 따라 공격 실행
    public virtual void RunAway()
    {
        canMove = true;
        //attack 받은 방향 반대편으로 달아난다.
        if (pathFinder.enabled)
        {
            pathFinder.isStopped = false; //계속 이동
            pathFinder.SetDestination(transform.position + mAttackDir * 1.5f);
            pathFinder.speed = moveSpeed*4.0f;
            enemyAnimator.speed = 2.0f;
            isHitted = false;
        }
        //canMove = false;
    }

    Vector3 SetRandomDest(Bounds bounds)
    {
        var x = Random.Range(bounds.min.x, bounds.max.x);
        var z = Random.Range(bounds.min.z, bounds.max.z);

        dist = new Vector3(x, transform.position.y, z);

        //target.position = dist;
        return dist;
    }

    //데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage,float delayTime, Vector3 attackDir)
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
            isHitted = true;
            canMove = false;
            StartCoroutine("DelayedHitAttack", delayTime);
        }

        //피격 애니메이션 재생
        //enemyAnimator.SetTrigger("Hit");

        //LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage, delayTime,attackDir);
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
        StartCoroutine("DelayedDestroy", 3.0f);
        /*//사망 효과음 재생
        enemyAudioPlayer.PlayOnShot(deathSound);
        */
    }

    IEnumerator DelayedHitAttack(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        //피격 효과음 재생
        EffectGenerator.instance.GenerateEffect(1, 1.0f, 0, transform);
        EffectGenerator.instance.GenerateDamageTXT(0,1.0f,0, mDamage.ToString(), transform);
        AudioSrc.PlayOneShot(hitSound);
        //밀려나기
        transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + (mAttackDir * 30f), Time.deltaTime);
        if(bDead)
        {
            transform.rotation = Quaternion.Euler(Vector3.right * 90f);
            AudioSrc.PlayOneShot(deathSound);
        }
    }

    IEnumerator DelayedDestroy(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Destroy(this.gameObject);
    }
}
