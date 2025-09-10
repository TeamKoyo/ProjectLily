using System;
using UnityEngine;

public struct EffectData
{
    public string targetType; // 카드 대상
    public string targetPos; // 대상 범위
    public int cost;
    public string effectKey; // effect 테이블 키
    public int effectVal;
    public int round; // 지속 라운드 // 발동 라운드 포함
    public int cnt; // 지속 횟수

    public EffectData(CardData data)
    {
        targetType = data.targetType;
        targetPos = data.targetPos;
        cost = data.cost;
        effectKey = data.effectKey;
        effectVal = data.effectVal;
        round = data.round;
        cnt = data.cnt;
    }
    //public EffectData(Monster data)
    //{
    //    targetType = data.targetType;
    //    targetPos = data.targetPos;
    //    effectKey = data.effectKey;
    //    effectVal = data.effectVal;
    //    round = data.round;
    //    cnt = data.cnt;
    //}
}

public enum TargetType
{ 
    Enemy,
    Ally,
    Self
}

public enum EffectType
{
    Damage,
    Heal
}

public class EffectMgr : MonoBehaviour
{
    public void Effect(EffectData data, Transform target) // monster도 같이 활용할 방법 필요
    {
        if(Enum.TryParse(data.effectKey, true, out EffectType type))
        {
            switch (type)
            {
                case EffectType.Damage:
                    Damage(target.GetComponent<Character>(), data.effectVal);
                    break;

                case EffectType.Heal:
                    Heal(target.GetComponent<Character>(), data.effectVal);
                    break;

            }
        }
    }

    public void Damage(IEffect target, int val)
    {
        target.Damage(val);
    }

    public void Heal(IEffect target, int val)
    {
        target.Heal(val);
    }
}
