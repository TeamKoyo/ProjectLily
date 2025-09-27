using System;
using UnityEngine;

public class BattleMgr : MonoBehaviour
{
    private (bool isEnemy, int id) recentOrder; // 현재 순서
    private int drawCnt;

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
        GetRecentOrder();
    }

    private void GetRecentOrder()
    {
        if(orderMgr.idx == 0)
        {
            Draw(drawCnt);
        }

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
                PlayableChar character = charTrans.GetComponentInChildren<PlayableChar>();

                if(character.id != recentOrder.id)
                {
                    continue;
                }
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

    public void ActiveTarget(string type)
    {
        if (Enum.TryParse<TargetType>(type, true, out TargetType target))
        {
            switch (target)
            {
                case TargetType.Enemy:
                    foreach (Transform slot in uiMgr.enemies)
                    {
                        if(slot.childCount > 0)
                        {
                            uiMgr.ActiveSlot(slot);
                        }
                    }
                    break;

                case TargetType.Ally:
                    foreach (Transform slot in uiMgr.allies)
                    {
                        if (slot.childCount > 0)
                        {
                            uiMgr.ActiveSlot(slot);
                        }
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
                PlayableChar character = slot.GetComponentInChildren<PlayableChar>();

                if (recentOrder.id == character.id)
                {
                    return slot;
                }
            }
        }

        return null;
    }

    //public bool UseCard(EffectData data, Transform select)
    //{
    //    effectMgr.Effect(data, select);

    //    return true;
    //}
}
