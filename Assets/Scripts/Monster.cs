using UnityEngine;
using System.Linq;

public class Monster : Character
{
    private BattleMgr battleMgr;
    private MonsterData data;
    private MonsterSequence seqData;
    private (int val, int idx)[] sortedPriority; // 우선순위에 따른 cardIds의 인덱스
    private int currentIdx = 0; // 사용할 카드 순서

    private void Start()
    {
        battleMgr = FindFirstObjectByType<BattleMgr>();
    }

    public override void SetData(int monsterId)
    {
        data = InfoMgr.Instance.database.monsters.Find(m => m.monsterId == monsterId);
        seqData = InfoMgr.Instance.database.sequences.Find(s => s.sequenceId == data.sequenceId);
        id = data.monsterId;
        spriteRoot = data.MonsterName + '_';
        status.hp = data.maxHp;

        SortPriority();

        SetSprite("Idle");
        UpdateHp();
    }

    private void SortPriority()
    {
        sortedPriority = new (int, int)[seqData.cardIds.Count];
        sortedPriority = seqData.priorities.Select((v, i) => (val: v, idx: i)).OrderBy(e => e.val).ToArray();
    }

    protected override int GetMaxHp()
    {
        return data.maxHp;
    }

    private void SelectCard(int idx)
    {
        CardData data = InfoMgr.Instance.database.cards.Find(c => c.cardId == seqData.cardIds[idx]);
        battleMgr.ActiveTarget(data.effectKey);
        battleMgr.UseCard(data.effectKey);
    }

    public void UseCard()
    {
        if (!ChkPriorityAboutSame(currentIdx)) // 우선순위가 다름
        {
            SelectCard(sortedPriority[currentIdx].idx);
            NextIdx();
        }
        else
        {
            int lastIdx = currentIdx + 1;
            if (lastIdx + 1 < sortedPriority.Length)
            {
                while (ChkPriorityAboutSame(lastIdx)) // 같은 우선순위의 마지막 idx 찾기
                {
                    lastIdx++;

                    if (lastIdx + 1 < sortedPriority.Length)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            CalRate(lastIdx);

            NextIdx();
        }
    }

    private void NextIdx()
    {
        if (currentIdx + 1 < sortedPriority.Length)
        {
            currentIdx++;
        }
        else
        {
            currentIdx = 0;
        }
    }

    private bool ChkPriorityAboutSame(int idx)
    {
        if(idx + 1 == sortedPriority.Length || sortedPriority[idx].val != sortedPriority[idx + 1].val)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void CalRate(int lastIdx)
    {
        int totalRate = 0;
        int idx = currentIdx;
        while(idx <= lastIdx)
        {
            if(seqData.rates[sortedPriority[idx].idx] == 0) // null
            {
                idx++;
                continue;
            }

            totalRate += seqData.rates[sortedPriority[idx].idx];
            idx++;
        }
        idx = currentIdx;

        if (totalRate == 0) // rate없이 group만 존재
        {
            UseGroupCards(seqData.groups[idx], lastIdx);
            return;
        }

        int rateVal = Random.Range(1, totalRate + 1);
        int currentRate = seqData.rates[sortedPriority[idx].idx];

        while(currentRate <= rateVal)
        {
            idx++;
            currentRate += seqData.rates[sortedPriority[idx].idx];
        }

        int selectedIdx = sortedPriority[idx].idx;
        SelectCard(selectedIdx);

        if(seqData.groups[selectedIdx] != 0) // notNull
        {
            UseGroupCards(seqData.groups[selectedIdx], lastIdx);
        }
    }

    private void UseGroupCards(int group, int lastIdx)
    {
        int idx = currentIdx;

        while(idx <= lastIdx)
        {
            int selectedIdx = sortedPriority[idx].idx;

            if (seqData.groups[selectedIdx] == group && seqData.rates[selectedIdx] == 0) // 확률이 없는 것들만 사용
            {
                SelectCard(selectedIdx);
            }

            idx++;
        }
    }
}