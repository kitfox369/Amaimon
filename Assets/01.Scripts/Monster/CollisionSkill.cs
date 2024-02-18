using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using static UserPlayerCtrl;

public class CollisionSkill : MonoBehaviour
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
            LivingEntity targetEntity = collision.GetComponent<LivingEntity>();
            if ((int)palyer.mPlayerState != 4)      //Hit°¡ ¾Æ´Ò¶§
            {
                targetEntity.OnDamage(damage, 0, Vector3.zero);
                skillCollider.enabled = false;
            }
        }
    }
}
