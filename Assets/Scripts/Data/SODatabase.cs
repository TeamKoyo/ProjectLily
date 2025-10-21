using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataTable", menuName = "Scriptable Objects/DataTable")]
public class SODatabase : ScriptableObject
{
    public List<CardData> cards;
    public List<EffectData> effects;
    public List<CharData> chars;
    public List<MonsterData> monsters;
    public List<MonsterSequence> sequences;
}

[System.Serializable]
public class CardData
{
    public int cardId;
    public string cardName;
    public string cardClass; // 전용 카드
    public string cardCategory; // 카드 유형 (즉발, 패널티 ...)
    public string cardGroup; // 테마
    public string rarity;
    public string cardKeyword;
    public string desc;
    public string effectKey;
    public string rarityIcon;
    public string classIcon;
    public string categoryIcon;
    public string vfx;
    public string sfx;
    public int duration;
}

public enum CardCategory
{
    act
}

[System.Serializable]
public class EffectData
{
    public string effectKey;
    public string type; // dmg, heal ...
    public string targetFaction; // 진영
    public string targetOperator; // 지정 방식
    public int targetPos;
    public int val;
    public int pushVal;
    public string refOperator;
    public string refTarget;
    public string refType;
    public int conditionVal; // 조건값
    public string conditionMore; // bool
    public string repeatTarget; // bool
    public int repeatCnt;
    public int refVal; // 수치보정 합연산
    public string searchRange;
    public string searchType;
    public int searchId;
}

public enum TargetFaction
{ 
    ally,
    enemy,
    all
}

public enum TargetOper
{
    position,
    self,
    single,
    all,
    random,
    nextr,
    nextl,
    bothside, // 본인 미포함
    bothside2 // 본인 포함
}

[System.Serializable]
public class CharData
{
    public int charId;
    public string charName;
    public int maxHp;
    public int maxMp; // 무력화
    public int maxSp; // 방어도
    public int drawPoint;
}

[System.Serializable]
public class MonsterData
{
    public int monsterId;
    public string MonsterName;
    public int maxHp;
    public int maxStagger; // 무력화
    public string vfxStagger;
    public string breakStagger;
    public string startBuff;
    public string sequenceId; // 스킬 모음
    public string triggerKey; // 페이즈 변경 조건
    public string vfx;
}

[System.Serializable]
public class MonsterSequence
{
    public string sequenceId;
    public string nextSequence;
    public List<int> cardIds;
    public List<int> priorities; // 우선순위 낮은순
    public List<int> rates; // 우선순위가 같을 때 발동 비율
    public List<int> groups; // 같은 그룹일 경우 연속 발동
}
