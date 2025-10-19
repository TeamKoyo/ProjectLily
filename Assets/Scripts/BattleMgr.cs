using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleMgr : MonoBehaviour
{
    private (bool isEnemy, int id) recentOrder; // 현재 순서
    private int drawCnt;
    private List<Transform> range = new List<Transform>(); // target 범위
    private List<Transform> autoTarget = new List<Transform>(); // target지정을 안하는 카드들의 target

    public BattleUIMgr uiMgr;
    public BattleOrderMgr orderMgr;
    public EffectMgr effectMgr;

    private void Start()
    {
        CreateScene();
        orderMgr.OnDicePhaseEnd += StartBattle; // 코루틴 종료시 실행
    }

    private void CreateScene()
    {
        foreach(int charId in InfoMgr.Instance.GetCharIds()) // ally
        {
            foreach(Transform slot in uiMgr.allies) // 순서가 고정되므로 추후 논의
            {
                if(slot.childCount < 1)
                {
                    GameObject characterObj = Instantiate(InfoMgr.Instance.charPrefab, slot);
                    PlayableChar character = characterObj.GetComponent<PlayableChar>();

                    character.SetData(charId);
                    drawCnt += character.GetDrawCnt();
                    break;
                }
            }

            orderMgr.CreateDice(charId, false);
        }

        foreach(int monsterId in InfoMgr.Instance.GetMonsterIds()) // enemy
        {
            foreach (Transform slot in uiMgr.enemies) // 순서가 고정되므로 추후 논의
            {
                if (slot.childCount < 1)
                {
                    GameObject monsterObj = Instantiate(InfoMgr.Instance.monsterPrefab, slot);
                    Monster monster = monsterObj.GetComponent<Monster>();

                    monster.SetData(monsterId);
                    break;
                }
            }

            orderMgr.CreateDice(monsterId, true);
        }

        foreach(int cardId in InfoMgr.Instance.GetCardIds()) // card
        {
            GameObject cardObj = Instantiate(InfoMgr.Instance.cardPrefab, uiMgr.deck);
            Card card = cardObj.GetComponent<Card>();

            card.SetData(cardId);
        }

        orderMgr.RollDice();
    }

    private void StartBattle()
    {
        for (int i = 0; i < InfoMgr.Instance.GetCharIds().Count + InfoMgr.Instance.GetMonsterIds().Count; i++)
        {
            recentOrder = orderMgr.ChkOrder();
            uiMgr.CreateOrderImg(recentOrder);
        }

        // 시작연출 필요
        StartOrder();
    }

    private void StartOrder()
    {
        if(orderMgr.idx == 0) // 라운드 시작시
        {
            drawCnt = 2; // debug
            Draw(drawCnt);
        }

        recentOrder = orderMgr.ChkOrder();

        DragMgr.Instance.isPlayerTurn = !recentOrder.isEnemy;

        if (recentOrder.isEnemy)
        {
            Monster monster = GetOrderSlot().GetComponentInChildren<Monster>();
            monster.UseCard();

            EndOrder();
        }
    }

    private Transform GetOrderSlot()
    {
        Transform group = recentOrder.isEnemy ? uiMgr.enemies : uiMgr.allies;

        foreach (Transform slot in group)
        {
            if (slot.childCount == 0)
            {
                break;
            }

            Character character = recentOrder.isEnemy
                ? slot.GetComponentInChildren<Monster>()
                : slot.GetComponentInChildren<PlayableChar>();

            if (character != null && recentOrder.id == character.id)
            {
                return slot;
            }
        }

        return null;
    }

    public void EndOrder()
    {
        uiMgr.UpdateOrderImg();
        StartOrder();
    }

    public void Draw(int val) // deck -> hand
    {
        for (int i = 0; i < val; i++)
        {
            if (uiMgr.deck.childCount < 1) // deck이 없으면 리필
            {
                RefillDeck();
            }

            int idx = UnityEngine.Random.Range(0, uiMgr.deck.childCount); // 랜덤으로

            uiMgr.deck.GetChild(idx).SetParent(uiMgr.hand); // 한장 뽑음
        }

        ExceedHand();
    }

    private void RefillDeck()
    {
        while (uiMgr.graveyard.childCount > 0) // graveyard -> deck
        {
            uiMgr.graveyard.GetChild(0).SetParent(uiMgr.deck);
        }

        uiMgr.UpdateCntByChildren(uiMgr.graveyard);
        // 패널티 부과
    }

    private void ExceedHand() // 초과한만큼 hand -> graveyard
    {
        int maxHand = 10;

        while (uiMgr.hand.childCount > maxHand)
        {
            uiMgr.hand.GetChild(0).SetParent(uiMgr.graveyard); // 앞 부터
        }

        uiMgr.UpdateCntByChildren(uiMgr.graveyard);
    }

    public void ActiveTarget(string effectKey)
    {
        EffectData effectData = InfoMgr.Instance.database.effects.Find(e => e.effectKey == effectKey);
        range.Clear();
        autoTarget.Clear();

        if (Enum.TryParse<TargetFaction>(effectData.targetFaction, true, out TargetFaction faction))
        {
            switch (faction)
            {
                case TargetFaction.ally:
                    foreach (Transform slot in uiMgr.allies)
                    {
                        if (slot.childCount > 0)
                        {
                            range.Add(slot);
                        }
                    }
                    break;

                case TargetFaction.enemy:
                    foreach (Transform slot in uiMgr.enemies)
                    {
                        if (slot.childCount > 0)
                        {
                            range.Add(slot);
                        }
                    }
                    break;

                case TargetFaction.all:
                    foreach (Transform slot in uiMgr.allies)
                    {
                        if (slot.childCount > 0)
                        {
                            range.Add(slot);
                        }
                    }
                    foreach (Transform slot in uiMgr.enemies)
                    {
                        if (slot.childCount > 0)
                        {
                            range.Add(slot);
                        }
                    }
                    break;
            }
        }
        
        if(Enum.TryParse<TargetOper>(effectData.targetOperator, true, out TargetOper oper))
        {
            switch(oper)
            {
                case TargetOper.position:
                    int pos = effectData.targetPos;
                    for (int i = 0; i < range.Count; i++)
                    {
                        int divisor = 1000 / (int)Mathf.Pow(10, i);
                        if (pos / divisor == 1)
                        {
                            uiMgr.ActiveSlot(range[i], true);
                            autoTarget.Add(range[i].GetChild(0));
                        }
                        pos %= divisor;
                    }
                    break;

                case TargetOper.self:
                    uiMgr.ActiveSlot(GetOrderSlot(), true);
                    autoTarget.Add(GetOrderSlot().GetChild(0));
                    break;

                case TargetOper.all:
                    foreach(Transform slot in range)
                    {
                        autoTarget.Add(slot.GetChild(0));
                    }
                    goto default;

                case TargetOper.random:
                    autoTarget.Add(range[UnityEngine.Random.Range(0, range.Count)].GetChild(0));
                    goto default;

                default:
                    foreach(Transform slot in range)
                    {
                        uiMgr.ActiveSlot(slot, true);
                    }
                    break;
            }
        }
    }

    public void InactiveTarget()
    {
        if(range.Count > 0)
        {
            foreach (Transform slot in range)
            {
                if (slot.childCount > 0)
                {
                    uiMgr.ActiveSlot(slot, false);
                }
            }
        }
    }

    public void UseCard(string effectKey, Transform selectTarget = null)
    {
        List<Transform> targets;

        if (selectTarget != null)
        {
            // 선택 타겟
            targets = effectMgr.Effect(effectKey, selectTarget);
        }
        else if (autoTarget.Count > 0)
        {
            // 자동 타겟
            foreach (Transform target in autoTarget)
            {
                effectMgr.Effect(effectKey, target);
            }

            targets = new List<Transform>(autoTarget);
        }
        else
        {
            // 타겟 없음
            effectMgr.Effect(effectKey);
            InactiveTarget();
            return;
        }

        InactiveTarget();

        string spriteRoot = recentOrder.isEnemy
            ? GetOrderSlot().GetComponentInChildren<Monster>().spriteRoot
            : GetOrderSlot().GetComponentInChildren<PlayableChar>().spriteRoot;

        uiMgr.ActiveAction(recentOrder.isEnemy, spriteRoot, targets);
    }
}