using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private BattleUIMgr uiMgr;
    private int idx; // hand index

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
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //if(DragMgr.Instance.EnterCardUse())
        //{
        //    if(_cardMgr.Use())
        //    {
        //        transform.SetParent(BattleUIMgr.graveyard); // hand -> trash
        //    }
        //    else
        //    {
        //        Restore();
        //    }
        //}
        //else
        //{
        //    Restore();
        //}
        Restore();
        DragMgr.Instance.EndDrag();
        Hover();
    }
}
