using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private BattleMgr battleMgr;
    private CardData data; // 정보
    private int idx; // hand index

    public GameObject detailForm;
    public int id;
    public Image[] imgs;
    public Text[] costs;
    public Text cardName;
    public Text description; // 효과 설명
    #region Exclusive
    public GameObject exclusive; // 전용 obj 부모
    public Image charImg;
    public Text charName;
    #endregion

    private void Start()
    {
        battleMgr = Object.FindFirstObjectByType<BattleMgr>();
        detailForm.SetActive(false);
    }

    public void SetData(int cardId)
    {
        data = InfoMgr.Instance.database.cards.Find(c => c.cardId == cardId);

        id = data.cardId;
        //foreach(Image img in imgs)
        //{

        //}
        foreach (Text cost in costs)
        {
            cost.text = data.cost.ToString();
        }
        cardName.text = data.cardName;
        //description.text = 

        if (!data.charName.Equals(""))
        {
            exclusive.SetActive(true);
            //charImg.sprite = 
            charName.text = data.charName;
        }
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
        transform.SetParent(battleMgr.uiMgr.hand);

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

        battleMgr.ActiveTarget(data.targetType, true);
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
                //    transform.SetParent(uiMgr.graveyard); // hand -> trash
                //    uiMgr.UpdateCntByChildren(uiMgr.graveyard);
                //}
                //else
                //{
                //    Restore();
                //}

                transform.SetParent(battleMgr.uiMgr.graveyard); // hand -> trash
                battleMgr.uiMgr.UpdateCntByChildren(battleMgr.uiMgr.graveyard);
            }
            else
            {
                Restore();
            }
        }

        DragMgr.Instance.EndDrag();
        battleMgr.ActiveTarget(data.targetType, false);
        Hover();
    }
}
