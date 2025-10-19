using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class EffectMgr : MonoBehaviour
{
    private Dictionary<string, Action<EffectData>> effectDic;
    private Dictionary<string, Action<EffectData, Transform>> selectEffectDic;
    private Action<int> draw;

    private EffectMgr()
    {
        RegisterEffects();
    }

    private void Start()
    {
        BattleMgr battleMgr = GetComponent<BattleMgr>();
        draw = battleMgr.Draw;
    }

    private void RegisterEffects()
    {
        effectDic = new Dictionary<string, Action<EffectData>>();
        selectEffectDic = new Dictionary<string, Action<EffectData, Transform>>();
        MethodInfo[] methods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        
        foreach (MethodInfo method in methods)
        {
            string key = method.Name.ToLower(); // 엑셀 키와 동일하게
            ParameterInfo[] parameters = method.GetParameters();

            try
            {
                if (parameters.Length == 1 &&
                    parameters[0].ParameterType == typeof(EffectData))
                {
                    Action<EffectData> action =
                        (Action<EffectData>)Delegate.CreateDelegate(typeof(Action<EffectData>), this, method);
                    effectDic[key] = action;
                }
                else if (parameters.Length == 2 &&
                         parameters[0].ParameterType == typeof(EffectData) &&
                         parameters[1].ParameterType == typeof(Transform))
                {
                    Action<EffectData, Transform> action =
                        (Action<EffectData, Transform>)Delegate.CreateDelegate(typeof(Action<EffectData, Transform>), this, method);
                    selectEffectDic[key] = action;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"⚠️ '{method.Name}' 매핑 실패: {e.Message}");
            }
        }
    }

    public void Effect(string effectKey)
    {
        EffectData effectData = InfoMgr.Instance.database.effects.Find(e => e.effectKey == effectKey);

        if(effectDic.TryGetValue(effectData.type, out Action<EffectData> action))
        {
            action.Invoke(effectData);
        }
    }

    public List<Transform> Effect(string effectKey, Transform target)
    {
        EffectData effectData = InfoMgr.Instance.database.effects.Find(e => e.effectKey == effectKey);
        List<Transform> targets = new List<Transform>();

        if (selectEffectDic.TryGetValue(effectData.type, out Action<EffectData, Transform> effect))
        {
            // 여기에 타입별 분기(single / next)
            effect.Invoke(effectData, target);
            targets.Add(target);
        }

        return targets;
    }

    private void Draw(EffectData data)
    {
        draw.Invoke(data.val);
    }

    private void Damage(EffectData data, Transform target)
    {
        if(target.TryGetComponent<PlayableChar>(out PlayableChar componentC))
        {
            componentC.Damage(data.val);
        }
        else if(target.TryGetComponent<Monster>(out Monster componentM))
        {
            componentM.Damage(data.val);
        }
    }
}