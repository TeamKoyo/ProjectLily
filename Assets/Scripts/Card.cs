using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private BattleMgr battleMgr;
    private CardData data; // 정보
    private EffectData effectData; // 효과 관련정보만 포함
    private int idx; // hand index

    public GameObject miniForm;
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
        CreateEffectData();
        FormChgMini();
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

    public void CreateEffectData()
    {
        effectData.targetType = data.targetType;
        effectData.targetPos = data.targetPos;
        effectData.cost = data.cost;
        effectData.effectKey = data.effectKey;
        effectData.effectVal = data.effectVal;
        effectData.round = data.round;
        effectData.cnt = data.cnt;
    }

    private void FormChgMini()
    {
        miniForm.SetActive(true);
        detailForm.SetActive(false);
    }

    private void FormChgDetail()
    {
        miniForm.SetActive(false);
        detailForm.SetActive(true);
    }

    private void Restore() // -> hand
    {
        transform.SetParent(battleMgr.uiMgr.hand);

        if (idx != transform.GetSiblingIndex()) // 제자리
        {
            transform.SetSiblingIndex(idx);
        }
    }

    private void ChkTargetType()
    {
        if (!data.targetPos.Equals("self"))
        {
            battleMgr.ActiveTarget(data.targetType);
        }
        else
        {
            battleMgr.ActiveTarget(data.targetPos);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => FormChgDetail();

    public void OnPointerExit(PointerEventData eventData) => FormChgMini();

    public void OnPointerDown(PointerEventData eventData)
    {
        idx = transform.GetSiblingIndex();
        FormChgMini();

        DragMgr.Instance.BeginDrag(GetComponent<RectTransform>());

        transform.SetParent(transform.parent.parent);
        transform.rotation = Quaternion.identity;

        ChkTargetType();
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
                if (battleMgr.UseCard(effectData, result.gameObject.transform.GetChild(0)))
                {
                    transform.SetParent(battleMgr.uiMgr.graveyard); // hand -> trash
                    battleMgr.uiMgr.UpdateCntByChildren(battleMgr.uiMgr.graveyard);
                }
                else
                {
                    Restore();
                }
            }
            else
            {
                Restore();
            }
        }

        DragMgr.Instance.EndDrag();
        ChkTargetType();
    }
}
