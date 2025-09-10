using System;
using UnityEngine;

public class BattleMgr : MonoBehaviour
{
    private (bool isEnemy, int id) recentOrder; // 현재 순서
    private int cost = 0; // 현재 코스트

    public BattleUIMgr uiMgr;
    public BattleOrderMgr orderMgr;
    public EffectMgr effectMgr;

    private void Start()
    {
        orderMgr.OnDicePhaseEnd += StartBattle; // 코루틴 종료시 실행
        CreateScene();
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
                    Character Character = characterObj.GetComponent<Character>();

                    Character.SetData(charId);
                    break;
                }
            }

            orderMgr.CreateDice(charId, false);
        }

        foreach(int monsterId in InfoMgr.Instance.GetMonsterIds()) // enemy
        {
            GameObject monster = Instantiate(InfoMgr.Instance.charPrefab, uiMgr.enemies);

            orderMgr.CreateDice(monsterId, true);
        }

        foreach(int cardId in InfoMgr.Instance.GetCardIds()) // card
        {
            GameObject cardObj = Instantiate(InfoMgr.Instance.cardPrefab, uiMgr.deck);
            Card card = cardObj.GetComponent<Card>();

            card.SetData(cardId);
        }

        uiMgr.UpdateCntByChildren(uiMgr.deck);
        orderMgr.RollDice();
    }

    private void StartBattle()
    {
        for (int i = 0; i < InfoMgr.Instance.GetCharIds().Count + InfoMgr.Instance.GetMonsterIds().Length; i++)
        {
            recentOrder = orderMgr.ChkOrder();
            uiMgr.CreateOrderImg(recentOrder);
        }

        int cnt = 3; // 첫턴에 지급할 카드 매수
        Draw(cnt);
        // 시작연출 필요
        GetRecentOrder();
    }

    private void GetRecentOrder()
    {
        recentOrder = orderMgr.ChkOrder();

        if(recentOrder.isEnemy)
        {
            // 카드사용 금지
            //foreach (Monster monster in uiMgr.enemies)
            //{
            //    if (monster.id != recentOrder.id)
            //    {
            //        continue;
            //    }

            //    Debug.Log("Monster Action");
            //}
            Debug.Log("Monster Action");
            EndOrder();
        }
        else
        {
            // 카드사용 허가
            foreach(Transform charTrans in uiMgr.allies)
            {
                Character character = charTrans.GetComponentInChildren<Character>();

                if(character.id != recentOrder.id)
                {
                    continue;
                }

                character.StartOrder();
            }
        }
    }

    public void EndOrder()
    {
        uiMgr.UpdateOrderImg();

        if(orderMgr.idx == 0)
        {
            Debug.Log("end turn");
            
        }

        GetRecentOrder();
    }

    public void SetCost(int val)
    {
        cost = val;

        uiMgr.UpdateCost(cost);
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
        uiMgr.UpdateCntByChildren(uiMgr.deck);

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

    public void ActiveTarget(string type)
    {
        if (Enum.TryParse<TargetType>(type, true, out TargetType target))
        {
            switch (target)
            {
                case TargetType.Enemy:
                    foreach (Transform child in uiMgr.enemies)
                    {
                        uiMgr.ActiveSlot(child);
                    }
                    break;

                case TargetType.Ally:
                    foreach (Transform child in uiMgr.allies)
                    {
                        uiMgr.ActiveSlot(child);
                    }
                    break;

                case TargetType.Self:
                    uiMgr.ActiveSlot(GetRecentAlly());
                    break;
            }
        }
    }

    private Transform GetRecentAlly()
    {
        foreach (Transform slot in uiMgr.allies)
        {
            if (slot.childCount > 0)
            {
                Character character = slot.GetComponentInChildren<Character>();

                if (recentOrder.id == character.id)
                {
                    return slot;
                }
            }
        }

        return null;
    }

    public bool UseCard(EffectData data, Transform select)
    {
        if(data.cost <= cost)
        {
            cost -= data.cost;
            uiMgr.UpdateCost(cost);

            effectMgr.Effect(data, select);

            return true;
        }
        else
        {
            return false;
        }
    }
}
