using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.AI.Navigation;

public class LivingEntity : MonoBehaviour
{
    // Start is called before the first frame update
    public float startingHealth = 100f; //ü��
    public float coolTime = 0f; //�� Ÿ��
    public int monsterID;
    public int monsterKind;
    public float mHealth { get; protected set; } //���� ü��
    public float mCoolTime { get; protected set; } //���� ��Ÿ��
    public bool bDead { get; protected set; } //��� ����
    public int mID { get; protected set; }
    public int mKind { get; protected set; }

    public string mName { get; set; }

    public bool bIsHit { get; set; }

    protected float hitTimer;

    public bool bIsStun { get; set; }

    public bool onDeath;

    public NavMeshSurface navSurface;

    //����ü�� Ȱ��ȭ�� �� ���¸� ����
    protected void OnEnable()
    {
        //������� ���� ���·� ����
        bDead = false;
        //ü���� ���� ü������ �ʱ�ȭ
        mHealth = startingHealth;
        mCoolTime = coolTime;
        mID = monsterID;
        bIsHit = false;
    }


    //���ظ� �޴� ���
    public virtual void OnDamage(float damage, float delayTime, Vector3 attackDir)
    {
        bIsHit = true;
        hitTimer = 0;
        //��������ŭ ü�� ����
        mHealth -= damage; // health = health - damage;
        //ü���� 0 ���� && ���� ���� �ʾҴٸ� ��� ó�� ����
        if (mHealth <= 0 && !bDead)
        {
            Die();
        }
    }

    public virtual void OnNuckBack(float damage, Vector3 attackDir)
    {
        bIsHit = true;
        hitTimer = 0;
        //��������ŭ ü�� ����
        mHealth -= damage; // health = health - damage;
        //ü���� 0 ���� && ���� ���� �ʾҴٸ� ��� ó�� ����
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

    //ü���� ȸ�� �ϴ� ����� å�� �ִµ� ���� �÷��̾� ���� ��ũ��Ʈ���� ������ ����

    //��� ó��
    public virtual void Die()
    {
        bDead = true;
    }

}
