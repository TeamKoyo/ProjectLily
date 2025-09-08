using UnityEngine;
using UnityEngine.UI;

public class BattleUIMgr : MonoBehaviour
{
    public Transform turnImgParent;
    public Transform allies; // 캐릭터 생성 위치
    public Transform enemies; // 적 생성 위치
    public Transform cost;
    public Transform deck;
    public Transform hand;
    public Transform graveyard;

    public void UpdateCntByChildren(Transform trans)
    {
        trans.parent.GetComponentInChildren<Text>().text = trans.childCount.ToString();
    }

    public void UpdateCost(int val)
    {
        cost.GetComponentInChildren<Text>().text = val.ToString();
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

    public void ActiveSlot(Transform slot)
    {
        Image img = slot.GetComponent<Image>();
        img.enabled = !img.IsActive();
        img.raycastTarget = img.enabled;
    }
}
