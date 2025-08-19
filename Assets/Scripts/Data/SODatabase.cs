using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataTable", menuName = "Scriptable Objects/DataTable")]
public class SODatabase : ScriptableObject
{
    public List<CardData> cards;
    public List<CharData> chars;
}

[System.Serializable]
public class CardData
{
    public int cardId;
    public string cardName;
    public string targetType; // 카드 대상
    public string targetPos; // 대상 범위
    public int cost;
    public string charName; // character 테이블 키 // 전용 카드
    public string animationKey; // 캐릭터_클립
    public string cardImg; // 리소스명
    public string cardDisc; // string 테이블 키
    public string sound; // 리소스명
    public string enemyHit; // 대상자 effect 리소스명
    public string atkHit; // 시전자 effect 리소스명
    public string effectKey; // effect 테이블 키
    public int effectVal;
    public int round; // 지속 라운드 // 발동 라운드 포함
    public int cnt; // 지속 횟수
}

[System.Serializable]
public class Effect
{
    public int effectId;
    public string effectKey;
    public string effectName;
}

[System.Serializable]
public class CharData
{
    public int charId;
    public string charName;
    public int maxHp;
    public int maxCost;
    public int drawCnt; // 자신의 턴에 드로우 횟수
}