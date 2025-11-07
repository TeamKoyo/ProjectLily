using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private BattleMgr battleMgr;
    private CardData data; // 정보
    private int idx; // hand index
    private bool isSelect; // 타겟선택 필요여부

    public int id;
    public Image img;
    public Text cardName;
    public Text description; // 효과 설명

    private void Start()
    {
        battleMgr = FindFirstObjectByType<BattleMgr>();
    }

    public void SetData(int cardId)
    {
        data = InfoMgr.Instance.database.cards.Find(c => c.cardId == cardId);

        id = data.cardId;
        //img.sprite = 
        cardName.text = data.cardName;
        description.text = data.desc;

        GetIsSelect();
    }

    private void GetIsSelect()
    {
        EffectData effectData = InfoMgr.Instance.database.effects.Find(e => e.effectKey == data.effectKey);

        if(Enum.TryParse<TargetFaction>(effectData.targetFaction, true, out TargetFaction faction))
        {
            switch (faction)
            {
                case TargetFaction.all:
                    isSelect = false;
                    break;

                default:
                    if (Enum.TryParse<TargetOper>(effectData.targetOperator, true, out TargetOper oper))
                    {
                        switch (oper)
                        {
                            case TargetOper.position:
                            case TargetOper.all:
                            case TargetOper.random:
                                isSelect = false;
                                break;

                            default:
                                isSelect = true;
                                break;
                        }
                    }
                    break;
            }
        }
        else
        {
            isSelect = false;
        }
    }

    private void Hover(bool isActive)
    {
        battleMgr.uiMgr.ActiveCardInfo(isActive, transform);
    }

    private void Restore() // -> hand
    {
        transform.SetParent(battleMgr.uiMgr.hand);

        if (idx != transform.GetSiblingIndex()) // 제자리
        {
            transform.SetSiblingIndex(idx);
        }

        battleMgr.InactiveTarget();
    }

    private void ChkCategory()
    {
        transform.SetParent(battleMgr.uiMgr.graveyard); // hand -> graveyard
        battleMgr.uiMgr.UpdateCntByChildren(battleMgr.uiMgr.graveyard);

        if (Enum.TryParse<CardCategory>(data.cardCategory, true, out CardCategory category))
        {
            switch(category)
            {
                case CardCategory.act:
                    battleMgr.uiMgr.OnSpriteChangeFinished += battleMgr.EndOrder; // 턴 종료
                    break;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => Hover(true);

    public void OnPointerExit(PointerEventData eventData) => Hover(false);

    public void OnPointerDown(PointerEventData eventData)
    {
        if(DragMgr.Instance.isPlayerTurn)
        {
            idx = transform.GetSiblingIndex();
            Hover(false);

            DragMgr.Instance.BeginDrag(GetComponent<RectTransform>());
            transform.SetParent(transform.parent.parent);
            transform.rotation = Quaternion.identity;

            battleMgr.ActiveTarget(data.effectKey);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(DragMgr.Instance.isDrag)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (isSelect)
            {
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.CompareTag("Char"))
                    {
                        ChkCategory();
                        battleMgr.UseCard(data.effectKey, result.gameObject.transform);

                        break;
                    }
                    else
                    {
                        Restore();
                    }
                }
            }
            else
            {
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.CompareTag("CharPanel"))
                    {
                        ChkCategory();
                        battleMgr.UseCard(data.effectKey);

                        break;
                    }
                    else
                    {
                        Restore();
                    }
                }
            }

            DragMgr.Instance.EndDrag();
        }
    }
}
