using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using static UserPlayerCtrl;

public class UserPlayerCtrl : LivingEntity
{
    // Start is called before the first frame update
    public GameManager gm;
    MonsterManager monsterM;

    public LayerMask whatIsTarget; //������� ���̾�
    private float searchRadius = 3f;
    private Collider[] enemies;

    [Header("[Sound]")]
    public AudioClip attackSound;//��� ����
    public AudioClip hitSound; //�ǰ� ����
    public AudioClip rangeAttackSound; //�ǰ� ����
    public AudioClip dashSound; //�ǰ� ����
    AudioSource AudioSrc;

    [Header("State")]
    Rigidbody plyRigidbody;
    float mDashScalar = 5.0f;
    int mFastRunGaugeMax = 300;
    int mFastRunGauge = 300;
    float mRunGaugeCoolTime = 0.1f;
    Transform teleportPos;
    float moveSpeed = 2.0f, rotSpeed = 30.0f;
    public AnimationClip attackAnimClip;
    float attackAnimTime;
    public AnimationClip powerupAnimClip;
    float powerupAnimTime;
    float playerDamage = 20;
    float animSpeed = 1.0f;

    Vector3 startPosition;
    Vector3 arrivalPosition;

    enum animationState
    {
        IDLE, SLOW_RUN, FAST_RUN, ATTACK, ROLL, POWERUP, COMBO, STUNATTACK, RANGERATTACK,HIT, KNOCKBACK, DEATH
    };
    animationState animState = animationState.IDLE;
    public enum InteractState
    {
        NOTHING, READY, TALKING, BUYING
    };
    public InteractState mInteractState = InteractState.NOTHING;
    public enum PlayerState
    {
        IDLE, MOVE,FORCE_MOVE, ATTACK, HIT,KNOCKBACK,STANDUP, SKILL, DEATH,POTAL,INTRO, KILLED
    };
    public PlayerState mPlayerState = PlayerState.IDLE;

    [Header("[Skill]")]
    int comboState=0;
    public AnimationClip[] comboAnimClip;
    float[] comboAnimTime;
    public AnimationClip[] skillAnimClip;
    float[] skillAnimTime;


    [Header("[Boolean]")]
    bool bIsAttack = false;
    public bool bIsMove = true;

    [Header("[Timer]")]
    float mRunGaugeTimer;
    float mComboTimer;
    float animTimer;

    [Header("[Animations]")]
    public Animator anim;

    [Header("[Interactions]")]
    public Material mOutlineMaterial;
    public int interactNPCID;
    Collider interactCollider;

    Transform  playerObj;

    public PlayerCamera mCam;

    //mouse drag rotation
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 0F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = 0F;
    public float maximumY = 0F;
    public float h = -180;

    float rotationY = 0F;
    float rotationX = 0F;


    [Header("[NPC Interaction]")]
    public bool isCommentReady = false;
    public int InteractNPCKind = -1;
    UIManager uiM;
    Vector3 moveDir;

    [Header("[Ingame itm]")]
    public int gold;
    private List<InGameItem> ingameItm;

    private void Awake()
    {
        playerObj = this.GetComponent<Transform>();
        anim = this.GetComponentInChildren<Animator>();
        mCam.setPlayerTrans(this.transform.GetChild(1));
        plyRigidbody = this.GetComponent<Rigidbody>();
        AudioSrc = this.GetComponent<AudioSource>();
    }

    void Start()
    {
        gold = 80;

        comboAnimTime = new float[comboAnimClip.Length];
        for (int i = 0; i < comboAnimClip.Length; i++) comboAnimTime[i] = comboAnimClip[i].length;
        skillAnimTime = new float[skillAnimClip.Length];
        for (int i = 0; i < skillAnimClip.Length; i++) skillAnimTime[i] = skillAnimClip[i].length;
        attackAnimTime = attackAnimClip.length;
        powerupAnimTime = powerupAnimClip.length;

        ingameItm = new List<InGameItem>();
        ingameItm.Add(new InGameItem(0,"ü�� ����", 5));
        ingameItm.Add(new InGameItem(1, "��Ÿ�� ����", 5));
        ingameItm.Add(new InGameItem(2, "����", 5));

        uiM.InitializeItmInfo(ingameItm);

        monsterM = gm.transform.GetChild(2).GetComponent<MonsterManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (bDead) mPlayerState = PlayerState.DEATH;
        if (mPlayerState != PlayerState.DEATH)
        {
            if (mPlayerState==PlayerState.IDLE||mPlayerState==PlayerState.MOVE)
            {
                moveDir = Vector3.zero;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
                {
                    InputMove();
                }
                else
                {
                    if (mFastRunGauge < mFastRunGaugeMax)
                    {
                        mRunGaugeTimer += Time.deltaTime;
                        if (mRunGaugeTimer > mRunGaugeCoolTime) { mRunGaugeTimer = 0f; mFastRunGauge++; }
                    }
                    moveDir = Vector3.zero;
                    animState = animationState.IDLE;
                    anim.SetInteger("State", (int)animationState.IDLE);
                    mPlayerState = PlayerState.IDLE;
                }

                InputSkill();

               

            }
            else if(mPlayerState != PlayerState.INTRO)
            {
                animTimer += Time.deltaTime;
                if (animState == animationState.COMBO){comboStack();}
                else if (animState ==animationState.ROLL)
                {
                    transform.position += moveDir * mDashScalar * Time.deltaTime;

                    if (animTimer > 1.0f){BackToIdleAnimation();}
                }
                else if (animState == animationState.POWERUP)
                {
                    if (animTimer >= skillAnimTime[1]){BackToIdleAnimation();}
                }
                else if (animState == animationState.STUNATTACK)
                {
                    if (animTimer >= skillAnimTime[2]){BackToIdleAnimation();}
                }
                else if (animState == animationState.HIT)
                {
                    if (animTimer >= 0.5f){BackToIdleAnimation();}
                }
                else if (mPlayerState == PlayerState.KNOCKBACK)
                {
                    //knockback �����ǿ� �����ϸ�
                    float distance = Vector3.Distance(arrivalPosition, transform.position);
                    if (distance <= 2.0f)
                    {
                        if (mPlayerState != PlayerState.STANDUP)
                        {
                            anim.SetInteger("KnockBack", 1);
                            mPlayerState = PlayerState.STANDUP;
                            animTimer = 0f;
                            plyRigidbody.velocity = Vector3.zero;
                        }
                    }
                }
            }
            moveDir.Normalize();
        }
        else
        {
            mPlayerState = PlayerState.DEATH;
            animState = animationState.DEATH;
            anim.SetInteger("State", (int)animationState.DEATH);
        }
    }

    private void FixedUpdate()
    {
        if (moveDir != Vector3.zero) {
            moveDir.y = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(moveDir),Time.deltaTime* rotSpeed);

            plyRigidbody.MovePosition(transform.position + moveDir * moveSpeed * Time.deltaTime);
        }
    }

    public override void OnDamage(float damage, float delayTime, Vector3 attackDir)
    {
        if (mPlayerState != PlayerState.SKILL)
        {
            //������� ���� ���¿����� �ǰ� ȿ�� ���
            if (!bDead)
            {
                AudioSrc.PlayOneShot(hitSound);
                animState = animationState.HIT;
                anim.SetInteger("State", (int)animationState.HIT);
                mPlayerState = PlayerState.HIT;
                moveDir = Vector3.zero;
            }

            //LivingEntity�� OnDamage()�� �����Ͽ� ������ ����
            base.OnDamage(damage, delayTime, attackDir);
        }
    }

    void InputMove()
    {
        if (Input.GetKey(KeyCode.W)) { moveDir += mCam.transform.forward; }
        if (Input.GetKey(KeyCode.S)) { moveDir += -mCam.transform.forward; }
        if (Input.GetKey(KeyCode.D)) { moveDir += mCam.transform.right; }
        if (Input.GetKey(KeyCode.A)) { moveDir += -mCam.transform.right; }
        if (Input.GetKey(KeyCode.LeftShift) && mFastRunGauge > 0)
        {
            moveSpeed = 5.0f;
            mFastRunGauge -= 1;
            animState = animationState.FAST_RUN;
            anim.SetInteger("State", (int)animationState.FAST_RUN);
        }
        else
        {
            moveSpeed = 2.0f;
            animState = animationState.SLOW_RUN;
            anim.SetInteger("State", (int)animationState.SLOW_RUN);
        }

        mPlayerState = PlayerState.MOVE;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animState = animationState.ROLL;
            anim.SetInteger("State", (int)animationState.ROLL);
            AudioSrc.PlayOneShot(dashSound);
            mPlayerState = PlayerState.FORCE_MOVE;
        }
    }

    void InputSkill()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                bIsAttack = true;
                moveDir = Vector3.zero;
                animState = animationState.ATTACK;
                anim.SetInteger("State", (int)animationState.ATTACK);
                mPlayerState = PlayerState.ATTACK;
            }
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            if (uiM.mSkill[0].IsUseSkill() && ingameItm[0].count > 0)
            {
                if (mHealth + 50 < startingHealth) mHealth += 50;
                else mHealth = startingHealth;
                EffectGenerator.instance.GenerateEffect(4, powerupAnimTime, 0, transform);
                uiM.mSkill[0].UseSkill();
                ingameItm[0].UseItm();
                uiM.mSkill[0].UpdateItmNum(ingameItm[0]);
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            if (uiM.mSkill[1].IsUseSkill())
            {
                uiM.mSkill[1].UseSkill();
                ingameItm[1].UseItm();
                uiM.mSkill[1].UpdateItmNum(ingameItm[0]);
                for (int i = 0; i < uiM.mSkill.Length; i++) uiM.mSkill[i].coolTimeReduce(10);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            if (uiM.mSkill[2].IsUseSkill())
            {
                uiM.mSkill[2].UseSkill();
                ingameItm[2].UseItm();
                uiM.mSkill[2].UpdateItmNum(ingameItm[0]);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (uiM.mSkill[3].IsUseSkill())
            {
                animState = animationState.COMBO;
                anim.SetInteger("State", (int)animationState.COMBO);
                mPlayerState = PlayerState.SKILL;
                moveDir = Vector3.zero;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (uiM.mSkill[4].IsUseSkill())
            {
                animState = animationState.POWERUP;
                anim.SetInteger("State", (int)animationState.POWERUP);
                EffectGenerator.instance.GenerateEffect(0, powerupAnimTime, 0, transform);
                mPlayerState = PlayerState.SKILL;
                if (uiM.mSkill[4].UseSkill())
                {
                    uiM.AddPassiveUI(4);
                }
                moveDir = Vector3.zero;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (uiM.mSkill[5].IsUseSkill())
            {
                animState = animationState.STUNATTACK;
                anim.SetInteger("State", (int)animationState.STUNATTACK);
                mPlayerState = PlayerState.SKILL;
                uiM.mSkill[5].UseSkill();
                moveDir = Vector3.zero;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (uiM.mSkill[6].IsUseSkill())
            {
                animState = animationState.RANGERATTACK;
                anim.SetInteger("State", (int)animationState.RANGERATTACK);
                mPlayerState = PlayerState.SKILL;
                uiM.mSkill[6].UseSkill();
                moveDir = Vector3.zero;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            mPlayerState = PlayerState.DEATH;
            animState = animationState.DEATH;
            anim.SetInteger("State", (int)animationState.DEATH);
        }
    }

    void skillReset(int animCurrentState)
    {
        int skillConIdx = -1;
        if (animCurrentState == 6)      //Combo
        {
            comboState = 0;
            mComboTimer = 0f;
            skillConIdx = 3;
        }
        else if(animCurrentState == 7)
        {
            skillConIdx = 5;
        }
        else if(animCurrentState == 8)
        {
            skillConIdx = 6;
        }

        if (skillConIdx != -1)
        {
            uiM.mSkill[skillConIdx].UseSkill();
            uiM.mSkill[skillConIdx].SkillImgReset();
        }
    }

    public override void OnNuckBack(float damage, Vector3 attackDir)
    {
        moveDir = Vector3.zero;
        attackDir.y = 0;        //���� ���̷� ���� y ���̰� �� �� ����
        if (mPlayerState == PlayerState.SKILL)
        {
            //��ų ĵ�� �� �ʱ�ȭ
            skillReset((int)animState); 
        }
        mPlayerState = PlayerState.KNOCKBACK;
        animState = animationState.KNOCKBACK;
        anim.SetInteger("State", (int)animationState.KNOCKBACK);
        startPosition = transform.localPosition;
        arrivalPosition = transform.localPosition + attackDir;
        //���󰡱�
        PredictTrajectory(startPosition, attackDir);
        base.OnNuckBack(damage, attackDir);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (mInteractState != InteractState.TALKING)
        {
            if (collision.tag == "NPC")
            {
                NPCContainer npc = collision.transform.GetComponent<NPCContainer>();
                if (npc.quests.Count > 0)
                {
                    uiM.activeCommentTip(npc.ID,npc.NPCName);
                    mInteractState = InteractState.READY;
                    isCommentReady = true;
                    InteractNPCKind = 0;
                    AddOutLineMaterials(collision);
                    interactNPCID = npc.ID;
                }
            }
            else if (collision.tag == "ShopNPC")
            {
                ShopNPCContainer npc = collision.transform.GetComponent<ShopNPCContainer>();
               
                uiM.activeCommentTip(npc.ID, npc.NPCName);
                mInteractState = InteractState.READY;
                isCommentReady = true;
                InteractNPCKind = 1;
                AddOutLineMaterials(collision);
                interactNPCID = npc.ID;
            }
            else if (collision.tag == "Portal")
            {
                //��Ż�� �߽ɰ� �������� ���� �۵��ϵ��� ����
                if (Vector3.Distance(transform.position,collision.transform.position)<=2.0f)
                {
                    Potal potal = uiM.getOutPotalInfo(collision.transform.position);
                    teleportPos = potal.potalTrans;
                    mPlayerState = PlayerState.POTAL;
                    animState = animationState.IDLE;
                    anim.SetInteger("State", (int)animationState.IDLE);
                    moveDir = Vector3.zero;
                    StartCoroutine("DelayedTeleport", mCam.fadeSpeed);
                    mCam.FadeINOUTEffect();
                    gm.SetActiveBGM(false);
                    gm.ChangeBGM(potal.bgmNum);
                }
            }
            else if (collision.tag == "BossMap")
            {
                BossMapInfo bossMapInfo = collision.GetComponent<BossMapInfo>();
                mCam.IntroCamera(bossMapInfo);
                bossMapInfo.mBossInfo.mIntro = true;
                bossMapInfo.player = this.GetComponent<LivingEntity>();
                uiM.SetActiveUI(false);
                transform.position = bossMapInfo.playerInitialPos.position;
                transform.rotation = bossMapInfo.playerInitialPos.rotation;
                collision.enabled = false;
                bossMapInfo.blockade.SetActive(true);
                mPlayerState = PlayerState.INTRO;
                moveDir=Vector3.zero;
                gm.SetActiveBGM(false);
                monsterM.EnterBossZone(bossMapInfo.BossIdx);
            }
        }
    }

    void AddOutLineMaterials(Collider collision)
    {
        SkinnedMeshRenderer renderer = collision.gameObject.transform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>();
        List<Material> list = new List<Material>();
        list.AddRange(renderer.sharedMaterials);
        list.Add(mOutlineMaterial);
        renderer.materials = list.ToArray();
        interactCollider = collision;
    }

    public void RemoveOutLineMaterials()
    {
        SkinnedMeshRenderer renderer = interactCollider.gameObject.transform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>();
        if (renderer != null)
        {
            List<Material> list = new List<Material>();
            list.AddRange(renderer.sharedMaterials);
            list.Remove(mOutlineMaterial);
            renderer.materials = list.ToArray();
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.tag == "Enemy"|| collision.tag == "Boss")
        {
            if(animState==animationState.ATTACK&&bIsAttack)
            {
                bIsAttack = false;
            }
        }

    }

    private void OnTriggerExit(Collider collision)
    {
        if (mInteractState != InteractState.TALKING)
        {
            if (collision.tag == "NPC"|| collision.tag == "ShopNPC")
            {
                uiM.disableCommentTip();
                mInteractState = InteractState.NOTHING;
                isCommentReady = false;
                RemoveOutLineMaterials();
            }
        }
    }

    void PredictTrajectory(Vector3 startPos, Vector3 vel)
    {
        float distance;
        distance = Mathf.Pow((vel.x) * (vel.x) + (vel.z) * (vel.z), 0.5f); //���򵵴ްŸ� ���
        float angle_degr = 45;

        Vector3 BombInitVelocity = new Vector3(vel.x, distance * Mathf.Tan(angle_degr * Mathf.Deg2Rad), vel.z).normalized; //���� ���������� �� ������ �������� ���
        BombInitVelocity = BombInitVelocity * Mathf.Sqrt(Mathf.Abs(Physics.gravity.y) * distance / Mathf.Sin(2 * angle_degr * Mathf.Deg2Rad)); //������ �ӵ��� �߷¿� ����ϱ� ������ *�߷� ������*

        plyRigidbody.velocity = BombInitVelocity;
    }

    public void UnpackReward(RewardPackage reward)
    {
        gold += reward.gold;
        int idx = checkSameItm(reward.itemIds);
        if (idx != -1) {
            ingameItm[idx].AddItm(reward.itemNum);
            uiM.mSkill[idx].UpdateItmNum(ingameItm[idx]);
            uiM.NoticeReward(reward);
        }
    }

    private int checkSameItm(int idx)
    {
        for (int i = 0; i < ingameItm.Count; i++)
        {
            if (ingameItm[i].idx == idx)
            {
                return i;
            }
        }
        return -1;
    }

    public void settingOfManager(UIManager uiM)
    {
        this.uiM = uiM;
    }

    public int getInteractMode() { return (int)mInteractState; }

    public void setInteractMode(int modeNum) { 
        mInteractState=(InteractState)modeNum;
        animState = animationState.IDLE;
        anim.SetInteger("State", (int)animationState.IDLE);
    }

    public float getFastRunGage() { return mFastRunGauge/ (float)mFastRunGaugeMax; }
    public float getHPGage() { return mHealth / (float)startingHealth; }
    IEnumerator DelayedTeleport(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        transform.position = teleportPos.position+teleportPos.forward*5f;
        yield return new WaitForSeconds(delayTime);
        mPlayerState = PlayerState.IDLE;
        gm.SetActiveBGM(true);
        //������ ���
        uiM.StartMapInfoAnimation();
    }
   
    void comboStack()
    {
        bool isCombo = false;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isCombo = true;
        }
        mComboTimer += Time.deltaTime;
        anim.SetInteger("ComboState", comboState);
        uiM.mSkill[3].UseCombo(comboState);
        if (mComboTimer > comboAnimTime[comboState] + 0.5f)
        {
            BackToIdleAnimation();
            comboState = 0;
            mComboTimer = 0f;
            uiM.mSkill[3].UseSkill();
            uiM.mSkill[3].SkillImgReset();
        }
        else if (mComboTimer > comboAnimTime[comboState])
        {
            if (isCombo)
            {
                comboState++;
                mComboTimer = 0f;
                moveDir = Vector3.zero;
                if (comboState == 3)
                {
                    comboState = 0;
                    uiM.mSkill[3].UseSkill();
                    uiM.mSkill[3].SkillImgReset();
                    BackToIdleAnimation();
                }
            }
            
        }
    }

    public void DetectEnemies(int type)
    {
        if (type == 1)      //Front
        {
            enemies = Physics.OverlapBox(transform.position + transform.forward, GetComponent<BoxCollider>().size / 2
            , Quaternion.identity, whatIsTarget);
        }
        else if (type == 2)    //Range
        {
            playerDamage = 100;
            enemies = Physics.OverlapSphere(transform.position, searchRadius, whatIsTarget);
        }

        //��� �ݶ��̴��� ��ȸ�ϸ鼭 ��� �ִ� LivingEntity ã��
        for (int i = enemies.Length; i > 0; i--)
        {
            //�ݶ��̴��κ��� LivingEntity ������Ʈ ��������
            LivingEntity livingEntity = enemies[i - 1].GetComponent<LivingEntity>();
            if (livingEntity == null) { livingEntity = enemies[i - 1].transform.parent.GetComponent<LivingEntity>(); }
            //LivingEntity ������Ʈ�� �����ϸ�, �ش� LivingEntity�� ��� �ִٸ�
            if (livingEntity != null && !livingEntity.bDead)
            {
                //Effect
                if (livingEntity.mHealth - playerDamage <= 0){
                    livingEntity.onDeath = true;
                    killEnemy(livingEntity.mKind,livingEntity.mID);
                }
            }
        }
    }

    public void OnDamageFront()
    {
        if (enemies.Length > 0)
        {
            LivingEntity livingEntity = enemies[0].GetComponent<LivingEntity>();
            if (livingEntity == null) { livingEntity = enemies[0].transform.parent.GetComponent<LivingEntity>(); }
            if (livingEntity != null && !livingEntity.bDead)
            {
                livingEntity.OnDamage(playerDamage, 0.0f, transform.forward);
            }
            AudioSrc.PlayOneShot(attackSound);
        }
        else
            Debug.Log("NO enemies");
    }

    public void OnDamageRange()
    {
        EffectGenerator.instance.GenerateEffect(3, skillAnimTime[3] / 4, 0.0f, transform);

        //������ 1f�� �ݶ��̴��� whatIsTarget ���̾ ���� �ݶ��̴� �����ϱ�
       
        //��� �ݶ��̴��� ��ȸ�ϸ鼭 ��� �ִ� LivingEntity ã��
        for (int i = enemies.Length; i >0 ; i--)
        {
            //�ݶ��̴��κ��� LivingEntity ������Ʈ ��������
            LivingEntity livingEntity = enemies[i-1].GetComponent<LivingEntity>();
            if (livingEntity == null) { livingEntity = enemies[i - 1].transform.parent.GetComponent<LivingEntity>(); }
            //LivingEntity ������Ʈ�� �����ϸ�, �ش� LivingEntity�� ��� �ִٸ�
            if (livingEntity != null && !livingEntity.bDead)
            {
                livingEntity.OnDamage(playerDamage, 0.0f, transform.forward);
            }
            AudioSrc.PlayOneShot(rangeAttackSound);
        }
        playerDamage = 20;
        enemies =null;
    }

    public void OnDamageStun()
    {
        enemies = Physics.OverlapBox(transform.position + transform.forward, GetComponent<BoxCollider>().size / 2
           , Quaternion.identity, whatIsTarget);

        if (enemies.Length > 0)
        {
            LivingEntity livingEntity = enemies[0].GetComponent<LivingEntity>();
            if (livingEntity == null) { livingEntity = enemies[0].transform.parent.GetComponent<LivingEntity>(); }
            if (livingEntity != null && !livingEntity.bDead)
            {
                livingEntity.OnStun(3.0f);
            }
        }
        else
            Debug.Log("NO enemies");
    }


    void killEnemy(int monKind,int monIdx)
    {
        mPlayerState = PlayerState.KILLED;
        animSpeed = 0.5f;
        anim.SetFloat("animSpeed", animSpeed);
        mCam.ZoomInOut();
        mCam.ShakingCamera(skillAnimTime[3] / 3 * (1 / anim.speed));
        gm.UpdateHuntQuestState(monKind,monIdx);
    }

    public void BackToIdleAnimation()
    {
        animState = animationState.IDLE;
        mPlayerState = PlayerState.IDLE;
        animTimer = 0f;
    }

    public void BackToIdle()
    {
        mPlayerState = PlayerState.IDLE;
        animSpeed = 1.0f;
        anim.SetFloat("animSpeed", animSpeed);
        anim.SetInteger("KnockBack", 0);
    }

}
