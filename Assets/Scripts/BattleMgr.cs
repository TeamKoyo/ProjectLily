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
        Character.OnCharDeath += DeathChar;
    }

    private void CreateScene()
    {
        foreach(int charId in InfoMgr.Instance.GetCharIds()) // ally
        {
            foreach(Transform slot in uiMgr.allies)
            {
                if(slot.childCount < 1)
                {
                    GameObject characterObj = Instantiate(InfoMgr.Instance.charPrefab, slot);
                    PlayableChar character = characterObj.GetComponent<PlayableChar>();

                    character.SetData(charId);
                    orderMgr.CreateDice(charId, false, character.spriteRoot); // 초상화 추가후 변경
                    drawCnt += character.GetDrawCnt();

                    break; // 하나 생성하면 다음 id
                }
            }
        }

        bool isBlank = false;
        foreach (int monsterId in InfoMgr.Instance.GetMonsterIds()) // enemy
        {
            foreach (Transform slot in uiMgr.enemies)
            {
                if (isBlank)
                {
                    slot.gameObject.SetActive(false);
                    isBlank = false;
                    break;
                }

                if (slot.gameObject.activeSelf && slot.childCount < 1 && !isBlank)
                {
                    GameObject monsterObj;

                    if (!InfoMgr.Instance.database.monsters.Find(m => m.monsterId == monsterId).isBig)
                    {
                        monsterObj = Instantiate(InfoMgr.Instance.monsterPrefab, slot);
                    }
                    else
                    {
                        monsterObj = Instantiate(InfoMgr.Instance.bigMonsterPrefab, slot);
                        isBlank = true;
                    }
                    Monster monster = monsterObj.GetComponent<Monster>();

                    monster.SetData(monsterId);
                    orderMgr.CreateDice(monsterId, true, monster.spriteRoot);

                    if(!isBlank)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            } 
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
        do
        {
            uiMgr.CreateOrderImg(orderMgr.ChkOrder());
        } while (orderMgr.idx != 0);

        // 시작연출 필요
        StartOrder();
    }

    private void StartOrder()
    {
        if (orderMgr.idx == 0) // 라운드 시작시
        {
            drawCnt = 2; // debug
            Draw(drawCnt);
        }

        recentOrder = orderMgr.ChkOrder();

        DragMgr.Instance.isPlayerTurn = !recentOrder.isEnemy;

        if (recentOrder.isEnemy)
        {
            Monster monster = GetOrderSlot().GetComponentInChildren<Monster>();

            uiMgr.OnSpriteChangeFinished += EndOrder;
            monster.UseCard();
        }
    }

    private Transform GetOrderSlot()
    {
        Transform group = recentOrder.isEnemy ? uiMgr.enemies : uiMgr.allies;

        foreach (Transform slot in group)
        {
            if (slot.childCount < 1 && slot.gameObject.activeSelf)
            {
                break;
            }
            else if(!slot.gameObject.activeSelf)
            {
                continue;
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
        uiMgr.OnSpriteChangeFinished -= EndOrder;

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
        List<int> blankIdx = new List<int>();
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
                        else if (!slot.gameObject.activeSelf)
                        {
                            blankIdx.Add(slot.GetSiblingIndex());
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
                    int idx = 0;
                    for (int i = 0; i < range.Count + blankIdx.Count; i++)
                    {
                        int divisor = 1000 / (int)Mathf.Pow(10, i);

                        if (pos / divisor == 1)
                        {
                            if (blankIdx.Contains(i))
                            {
                                uiMgr.ActiveSlot(range[idx - 1], true);
                                autoTarget.Add(range[idx - 1].GetChild(0));
                            }
                            else
                            {
                                uiMgr.ActiveSlot(range[idx], true);
                                autoTarget.Add(range[idx].GetChild(0));
                            }
                        }

                        idx++;
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

    public void UseCard(string effectKey, Transform selectTarget = null) // 카드를 연속으로 사용해서 계속 호출되지 않게
    {
        Transform order = GetOrderSlot().GetChild(0);
        string key = null;

        if (selectTarget != null)
        {
            // 선택 타겟
            key = effectMgr.Effect(effectKey, selectTarget);
        }
        else if (autoTarget.Count > 0)
        {
            // 자동 타겟
            foreach (Transform target in autoTarget)
            {
                key = effectMgr.Effect(effectKey, target);
            }
        }
        else
        {
            // 타겟 없음
            effectMgr.Effect(effectKey);
            return;
        }

        if (!string.IsNullOrEmpty(key))
        {
            StartCoroutine(uiMgr.CoChgSprite(order, order.GetComponent<Character>().spriteRoot + key));
            StartCoroutine(uiMgr.CoWaitChgSprite());
        }

        InactiveTarget();

        if(!recentOrder.isEnemy && uiMgr.hand.childCount < 1)
        {
            Draw(drawCnt);
            // 패널티 추가
        }
    }

    private void DeathChar(Character character)
    {
        uiMgr.OnSpriteChangeHit += Delete;
        uiMgr.OnCharDeath += uiMgr.ChkResult;

        void Delete()
        {
            uiMgr.OnSpriteChangeHit -= Delete;
            Destroy(character.gameObject);
            character.gameObject.SetActive(false);
            orderMgr.DelOrder(character.id);
            uiMgr.DelOrderImg(character.orderIdx);

            Transform slot = character.transform.parent;
            int idx = slot.GetSiblingIndex();
            int blank = 0;

            for (; idx < slot.parent.childCount - 1; idx++)
            {
                if (idx + 1 + blank < slot.parent.childCount)
                {
                    Transform nextSlot = slot.parent.GetChild(idx + 1 + blank);
                    Transform nextChar;
                    Vector2 localPos;

                    if (nextSlot.childCount > 0)
                    {
                        nextChar = nextSlot.GetChild(0);
                        localPos = nextChar.localPosition;

                        nextChar.SetParent(slot);
                        nextChar.localPosition = localPos;

                        slot = nextSlot;
                    }
                    else if (!nextSlot.gameObject.activeSelf)
                    {
                        if(slot.childCount < 1) // 2칸몹이 움직일 경우
                        {
                            slot.gameObject.SetActive(false);
                            nextSlot.gameObject.SetActive(true);
                            slot = nextSlot;

                            continue;
                        }

                        nextSlot.gameObject.SetActive(true);
                        blank++;

                        if(idx + 1 + blank < slot.parent.childCount)
                        {
                            Transform blankNextSlot = slot.parent.GetChild(idx + 1 + blank);

                            if(blankNextSlot.childCount > 0)
                            {
                                nextChar = blankNextSlot.GetChild(0);
                                localPos = nextChar.localPosition;

                                nextChar.SetParent(slot);
                                nextChar.localPosition = localPos;

                                slot = nextSlot;
                            }
                        }
                    }
                }
            }
        }
    }
}