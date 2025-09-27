using UnityEngine;
using UnityEngine.UI;

public class BattleUIMgr : MonoBehaviour
{
    public Transform turnImgParent;
    public Transform allies; // 캐릭터 생성 위치
    public Transform enemies; // 적 생성 위치
    public Transform deck;
    public Transform hand;
    public Transform graveyard;
    public Transform cardInfo;

    public void UpdateCntByChildren(Transform trans)
    {
        trans.parent.GetComponentInChildren<Text>().text = trans.childCount.ToString();
    }

    public void CreateOrderImg((bool isEnemy, int id) order)
    {
        GameObject turnImg = new GameObject("TurnImg", typeof(RectTransform), typeof(Image));
        turnImg.transform.SetParent(turnImgParent, false);

        Image img = turnImg.GetComponent<Image>();
        img.raycastTarget = false;
        //if(order.isEnemy)
        //{
        //    img.sprite = ;
        //}
        //else
        //{
        //    img.sprite = InfoMgr.Instance.database;
        //}
    }

    public void UpdateOrderImg()
    {
        turnImgParent.GetChild(0).SetSiblingIndex(turnImgParent.childCount);
    }

    public void ActiveCardInfo(bool isActive, Transform card)
    {
        if(isActive) // 최적화 필요
        {
            cardInfo.GetChild(0).GetComponent<Image>().sprite = card.GetComponent<Card>().img.sprite;
            cardInfo.GetChild(1).GetComponent<Text>().text = card.GetComponent<Card>().cardName.text;
            cardInfo.GetChild(2).GetComponent<Text>().text = card.GetComponent<Card>().description.text;
        }

        cardInfo.gameObject.SetActive(isActive);
    }

    public void ActiveSlot(Transform slot)
    {
        Image img = slot.GetChild(0).GetComponent<Image>();
        img.enabled = !img.IsActive();
    }
}
