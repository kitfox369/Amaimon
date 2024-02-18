using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using static UserPlayerCtrl;

public class CollisionNuckBackSkill : MonoBehaviour
{
    public float damage;
    private bool isCheck = false;
    public CapsuleCollider skillCollider;

    public void ActiveCheckColllision()
    {
        isCheck = true;
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.tag == "Player"&& isCheck)
        {
            UserPlayerCtrl palyer = collision.GetComponent<UserPlayerCtrl>();
            Vector3 moveDir = (collision.transform.position - transform.position).normalized;
            palyer.OnNuckBack(damage, moveDir * 10.0f);
            //skillCollider.enabled = false;
            isCheck = false;
            skillCollider.gameObject.SetActive(false);
            this.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(CoCheckAlive());
        //skillCollider.enabled = true;
    }

    IEnumerator CoCheckAlive()
    {
        yield return new WaitForSeconds(2.0f);
        ActiveCheckColllision();
    }
}
