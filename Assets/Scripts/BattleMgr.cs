using UnityEngine;
using UnityEngine.UI;

public class BattleMgr : MonoBehaviour
{
    (bool isEnemy, int id) recentOrder; // 현재 순서

    public BattleUIMgr uiMgr;
    public BattleOrderMgr orderMgr;

    private void Start()
    {
        orderMgr.OnDicePhaseEnd += StartBattle; // 코루틴 종료시 실행
        CreateScene();
    }

    private void CreateScene()
    {
        foreach(int charId in InfoMgr.Instance.GetCharIds()) // ally
        {
            foreach(Transform slot in uiMgr.allies)
            {
                if(slot.childCount < 1)
                {
                    GameObject character = Instantiate(InfoMgr.Instance.infoPrefab, slot);
                    CharInfo info = character.GetComponent<CharInfo>();

                    break;
                }
            }

            orderMgr.CreateDice(charId, false);
            //info.SetData(charId);
            //uiMgr.UpdateStatus(info);
        }

        foreach(int monsterId in InfoMgr.Instance.GetMonsterIds()) // enemy
        {
            GameObject monster = Instantiate(InfoMgr.Instance.infoPrefab, uiMgr.enemies);

            orderMgr.CreateDice(monsterId, true);
        }

        foreach(int cardId in InfoMgr.Instance.GetCardIds()) // card
        {
            GameObject card = Instantiate(InfoMgr.Instance.cardPrefab, uiMgr.deck);
            CardInfo info = card.GetComponent<CardInfo>();

            //info.SetData(cardId);
        }

        uiMgr.UpdateCntByChildren(uiMgr.deck);
        orderMgr.RollDice();
    }

    private void StartBattle()
    {
        for(int i = 0; i < InfoMgr.Instance.GetCharIds().Length + InfoMgr.Instance.GetMonsterIds().Length; i++)
        {
            recentOrder = orderMgr.ChkOrder();
            uiMgr.CreateTurnImg(recentOrder);
        }

        Draw(2); // debug
        // 시작연출 필요
        GetRecentOrder();
    }

    private void GetRecentOrder()
    {
        recentOrder = orderMgr.ChkOrder();

        if(recentOrder.isEnemy)
        {
            // 적 행동, 카드사용 금지
        }
        else
        {
            // 카드사용 허가
        }
    }

    public void Draw(int val) // deck -> hand
    {
        for (int i = 0; i < val; i++)
        {
            if (uiMgr.deck.childCount < 1) // deck이 없으면 리필
            {
                RefillDeck();
            }

            int idx = Random.Range(0, uiMgr.deck.childCount); // 랜덤으로

            uiMgr.deck.GetChild(idx).SetParent(uiMgr.hand); // 한장 뽑음
            uiMgr.UpdateCntByChildren(uiMgr.deck);
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
}
