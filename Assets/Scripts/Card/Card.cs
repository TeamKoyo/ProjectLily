using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private BattleUIMgr uiMgr;
    private int idx; // hand index

    public CardInfo info;

    private void Start()
    {
        uiMgr = Object.FindFirstObjectByType<BattleUIMgr>();
        Hover(); // prefab -> detail 상태
    }

    private void Hover() // mini <-> detail
    {
        if (DragMgr.Instance.isDrag)
        {
            return;
        }

        idx = transform.GetSiblingIndex();

        foreach (Transform cardForm in transform)
        {
            cardForm.gameObject.SetActive(!cardForm.gameObject.activeSelf);
        }
    }

    private void Restore() // -> hand
    {
        transform.SetParent(uiMgr.hand);

        if (idx != transform.GetSiblingIndex()) // 제자리
        {
            transform.SetSiblingIndex(idx);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => Hover();

    public void OnPointerExit(PointerEventData eventData) => Hover();

    public void OnPointerDown(PointerEventData eventData)
    {
        Hover();

        DragMgr.Instance.BeginDrag(GetComponent<RectTransform>());

        transform.SetParent(transform.parent.parent);
        transform.rotation = Quaternion.identity;

        //uiMgr.ActiveTarget(info.GetTargetType(), true);
        uiMgr.ActiveTarget("Enemy", true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("CharSlot"))
            {
                //if (카드 사용이 가능한지)
                //{
                //    transform.SetParent(BattleUIMgr.graveyard); // hand -> trash
                //}
                //else
                //{
                //    Restore();
                //}
                transform.SetParent(uiMgr.graveyard); // hand -> trash
                uiMgr.UpdateCntByChildren(uiMgr.graveyard);
            }
            else
            {
                Restore();
            }
        }

        DragMgr.Instance.EndDrag();
        //uiMgr.ActiveTarget(info.GetTargetType(), false);
        uiMgr.ActiveTarget("Enemy", false);
        Hover();
    }
}
