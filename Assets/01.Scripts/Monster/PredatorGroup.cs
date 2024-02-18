using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PredatorGroup : LivingEntity
{
    // Start is called before the first frame update
    public string Name;

    [Header("[Layer]")]
    public LayerMask whatIsTarget; //������� ���̾�

    private LivingEntity targetEntity;//�������
    private NavMeshAgent pathFinder; //��� ��� AI ������Ʈ

    /*public ParticleSystem hitEffect; //�ǰ� ����Ʈ
    */
    [Header("[Sound]")]
    public AudioClip deathSound;//��� ����
    public AudioClip hitSound; //�ǰ� ����
    AudioSource AudioSrc;

    private Animator enemyAnimator;
    float animSpeed = 1.0f;
    //private AudioSource enemyAudioPlayer; //����� �ҽ� ������Ʈ

    public float damage = 20f; //���ݷ�
    public float attackDelay = 1f; //���� ������
    private float lastAttackTime; //������ ���� ����
    private float dist; //���������� �Ÿ�
    private float distRange = 3f;
    private float searchRadius = 10f;
    private Vector3 distance; //������ �Ÿ�

    public Transform tr;

    private float attackRange = 2.3f;

    Vector3 moveDir;
    Vector3 mAttackDir;
    float mDamage;
    float moveSpeed = 2.0f;
    float searchTimer = 0f;
    float searchTime = 10.0f;
    float stunTime;

    //���� ����� �����ϴ��� �˷��ִ� ������Ƽ
    private bool hasTarget
    {
        get
        {
            //������ ����� �����ϰ�, ����� ������� �ʾҴٸ� true
            if (targetEntity != null && !targetEntity.bDead)
            {
                return true;
            }

            //�׷��� �ʴٸ� false
            return false;
        }
    }

    private bool canMove;
    private bool canAttack;

    NavMeshData navData;

    private void Awake()
    {
        //���� ������Ʈ���� ����� ������Ʈ ��������
        pathFinder = GetComponent<NavMeshAgent>();
        pathFinder.stoppingDistance = attackRange;
        enemyAnimator = transform.GetChild(0).GetComponent<Animator>();
        AudioSrc = GetComponent<AudioSource>();
    }

    //�� AI�� �ʱ� ������ �����ϴ� �¾� �޼���
    public void Setup(float newHealth, float newDamage, float newSpeed,string name)
    {
        //ü�� ����
        startingHealth = newHealth;
        mHealth = newHealth;
        //���ݷ� ����
        damage = newDamage;
        //�׺�޽� ������Ʈ�� �̵� �ӵ� ����
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
        //���� ������Ʈ Ȱ��ȭ�� ���ÿ� AI�� Ž�� ��ƾ ����
        StartCoroutine(UpdatePath());
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTarget)
        {
            //���� ����� ������ ��� �Ÿ� ����� �ǽð����� �ؾ��ϴ� Update()
            dist = Vector3.Distance(tr.position, targetEntity.transform.position);
            if (dist > searchRadius) { targetEntity = null; }
        }
        else
        {
            if ((pathFinder.destination - transform.position).magnitude <= pathFinder.stoppingDistance)
            {
                canMove = false;
                enemyAnimator.SetInteger("EnemyState", 0);
                //���Ƿ� �ڸ� �ٲٱ�
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

        //hit ���ֱ�
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


    //������ ����� ��ġ�� �ֱ������� ã�� ��� ����
    private IEnumerator UpdatePath()
    {
        //��� �ִ� ���� ���� ����
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

                    //���� ����� ���� ���, Idle ���� ����
                    //pathFinder.isStopped = true;
                    canAttack = false;

                    //������ 1f�� �ݶ��̴��� whatIsTarget ���̾ ���� �ݶ��̴� �����ϱ�
                    Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadius, whatIsTarget);

                    //��� �ݶ��̴��� ��ȸ�ϸ鼭 ��� �ִ� LivingEntity ã��
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        if (colliders[i].gameObject.tag.Equals("Player"))
                        {
                            //�ݶ��̴��κ��� LivingEntity ������Ʈ ��������
                            LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                            //LivingEntity ������Ʈ�� �����ϸ�, �ش� LivingEntity�� ��� �ִٸ�
                            if (livingEntity != null && !livingEntity.bDead && checkTargetInArea(navData.sourceBounds, livingEntity, transform.parent.position))
                            {
                                //���� ����� �ش� LivingEntity�� ����
                                targetEntity = livingEntity;
                                searchTimer = 0f;
                                //for�� ���� ��� ����
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

            //0.25�� �ֱ�� ó�� �ݺ�
            yield return new WaitForSeconds(0.25f);
        }
    }

    //���� ������ �Ÿ��� ���� ���� ����
    public virtual void Attack()
    {
        //�ڽ��� ���X, ���� ������ �Ÿ��� ���� ��Ÿ� �ȿ� �ִٸ�
        if (!bDead && dist <= attackRange)
        {
            //���� �ݰ� �ȿ� ������ �������� �����.
            canMove = false;

            //���� ��� �ٶ󺸱�
            this.transform.LookAt(targetEntity.transform);

            //�ֱ� ���� �������� attackDelay �̻� �ð��� ������ ���� ����
            if (lastAttackTime + attackDelay <= Time.time)
            {
                canAttack = true;
                OnDamageEvent();
                enemyAnimator.SetInteger("EnemyState", 2);
            }
            else   //���� �ݰ� �ȿ� ������, �����̰� �������� ���
            {
                canAttack = false;
                enemyAnimator.SetInteger("EnemyState", 0);
            }
        }
        else  //���� �ݰ� �ۿ� ���� ��� �����ϱ�
        {
            enemyAnimator.SetInteger("EnemyState", 1);
            canMove = true;
            canAttack = false;
            //��� ����
            pathFinder.isStopped = false; //��� �̵�
            pathFinder.SetDestination(targetEntity.transform.position);
        }
    }

    //����Ƽ �ִϸ��̼� �̺�Ʈ�� �ֵθ� �� ������ �����Ű��
    public void OnDamageEvent()
    {
        //���� ó��
        targetEntity.OnDamage(damage, 0, moveDir);

        //�ֱ� ���� �ð� ����
        lastAttackTime = Time.time;
    }


    //�������� �Ծ��� �� ������ ó��
    public override void OnDamage(float damage, float delayTime, Vector3 attackDir)
    {
        //������� ���� ���¿����� �ǰ� ȿ�� ���
        if (!bDead)
        {
            //���� ���� ������ �������� �ǰ� ȿ�� ���
            //hitEffect.transform.position = hitPoint;
            //hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            //hitEffect.Play();
            mAttackDir = attackDir;
            mDamage = damage;

            //LivingEntity�� OnDamage()�� �����Ͽ� ������ ����
            base.OnDamage(damage, delayTime, attackDir);

            HitAttack();
        }

        //�ǰ� �ִϸ��̼� ���
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

    //��� ó��
    public override void Die()
    {
        //LivingEntity�� DIe()�� �����Ͽ� �⺻ ��� ó�� ����
        base.Die();

        //�ٸ� AI�� �������� �ʵ��� �ڽ��� ��� �ݶ��̴��� ��Ȱ��ȭ
        Collider[] enemyColliders = GetComponents<Collider>();
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = false;
        }

        //AI������ �����ϰ� �׺�޽� ������Ʈ�� ��Ȱ��ȭ
        pathFinder.isStopped = true;
        pathFinder.enabled = false;

        //��� �ִϸ��̼� ���
        //enemyAnimator.SetTrigger("Die");
        StartCoroutine("DelayedDestroy", 5.0f);

    }

    void HitAttack()
    {
        //�ǰ� ȿ���� ���
        EffectGenerator.instance.GenerateEffect(1, 1.0f, 0, transform);
        EffectGenerator.instance.GenerateDamageTXT(0, 1.0f, 0, mDamage.ToString(), transform);
        AudioSrc.PlayOneShot(hitSound);
        enemyAnimator.SetInteger("EnemyState", 3);
        //�з�����
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
