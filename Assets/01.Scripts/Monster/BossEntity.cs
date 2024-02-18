using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BossEntity : LivingEntity
{
    public bool mIntro { get; set; }

    public virtual void StartBattle(LivingEntity player)
    {
        mIntro = false;
    }
}
