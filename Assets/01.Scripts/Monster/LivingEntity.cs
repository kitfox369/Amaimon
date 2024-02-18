using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.AI.Navigation;

public class LivingEntity : MonoBehaviour
{
    // Start is called before the first frame update
    public float startingHealth = 100f; //체력
    public float coolTime = 0f; //쿨 타임
    public int monsterID;
    public int monsterKind;
    public float mHealth { get; protected set; } //현재 체력
    public float mCoolTime { get; protected set; } //현재 쿨타임
    public bool bDead { get; protected set; } //사망 상태
    public int mID { get; protected set; }
    public int mKind { get; protected set; }

    public string mName { get; set; }

    public bool bIsHit { get; set; }

    protected float hitTimer;

    public bool bIsStun { get; set; }

    public bool onDeath;

    public NavMeshSurface navSurface;

    //생명체가 활성화될 떄 상태를 리셋
    protected void OnEnable()
    {
        //사망하지 않은 상태로 시작
        bDead = false;
        //체력을 시작 체력으로 초기화
        mHealth = startingHealth;
        mCoolTime = coolTime;
        mID = monsterID;
        bIsHit = false;
    }


    //피해를 받는 기능
    public virtual void OnDamage(float damage, float delayTime, Vector3 attackDir)
    {
        bIsHit = true;
        hitTimer = 0;
        //데미지만큼 체력 감소
        mHealth -= damage; // health = health - damage;
        //체력이 0 이하 && 아직 죽지 않았다면 사망 처리 실행
        if (mHealth <= 0 && !bDead)
        {
            Die();
        }
    }

    public virtual void OnNuckBack(float damage, Vector3 attackDir)
    {
        bIsHit = true;
        hitTimer = 0;
        //데미지만큼 체력 감소
        mHealth -= damage; // health = health - damage;
        //체력이 0 이하 && 아직 죽지 않았다면 사망 처리 실행
        if (mHealth <= 0 && !bDead)
        {
            Die();
        }
    }

    public virtual void OnStun(float stunTime)
    {
        bIsStun = true;
        bIsHit = true;
        hitTimer = 0;
    }

    //체력을 회복 하는 기능은 책에 있는데 나는 플레이어 스텟 스크립트에서 적용할 것임

    //사망 처리
    public virtual void Die()
    {
        bDead = true;
    }

}
