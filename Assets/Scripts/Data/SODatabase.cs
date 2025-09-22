using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataTable", menuName = "Scriptable Objects/DataTable")]
public class SODatabase : ScriptableObject
{
    public List<CardData> cards;
    public List<CharData> chars;
    public List<MonsterData> monsters;
    public List<MonsterSequence> sequences;
}

[System.Serializable]
public class CardData
{
    public int cardId;
    public string cardName;
    public string targetType; // 카드 대상
    public string targetPos; // 대상 범위
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
    public string effectKey;
    public string effectName;
}

[System.Serializable]
public class CharData
{
    public int charId;
    public string charName;
    //public string spriteRoot;
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
    public string spriteRoot;
    public string sequenceset; // 스킬 모음
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
