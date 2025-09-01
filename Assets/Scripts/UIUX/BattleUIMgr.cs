using System;
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

    public void UpdateStatus(CharInfo info)
    {
        Text hpTxt = info.hp.GetComponentInChildren<Text>();
        hpTxt.text = info.GetStatus().hp + " / " + info.GetData().maxHp;
    }

    public void UpdateCntByChildren(Transform trans)
    {
        trans.parent.GetComponentInChildren<Text>().text = trans.childCount.ToString();
    }

    public void CreateTurnImg((bool isEnemy, int id) recentOrder)
    {
        GameObject turnImg = new GameObject("TurnImg", typeof(RectTransform), typeof(Image));
        turnImg.transform.SetParent(turnImgParent, false);

        Image img = turnImg.GetComponent<Image>();
        img.raycastTarget = false;
        //if(recentOrder.isEnemy)
        //{
        //    img.sprite = ;
        //}
        //else
        //{
        //    img.sprite = ;
        //}
    }

    public void UpdateTurnImg()
    {
        turnImgParent.GetChild(0).SetSiblingIndex(turnImgParent.childCount);
    }

    public enum TargetType
    {
        None,
        Enemy,
        Ally,
        All
    }

    public void ActiveTarget(string targetType, bool isActive)
    {
        if (Enum.TryParse(targetType, out TargetType type) && Enum.IsDefined(typeof(TargetType), type))
        {
            switch(type)
            {
                case TargetType.None:

                    break;
                case TargetType.Enemy:
                    ActiveImg(enemies, isActive);
                    break;
                case TargetType.Ally:
                    ActiveImg(allies, isActive);
                    break;
                case TargetType.All:
                    ActiveImg(enemies, isActive);
                    ActiveImg(allies, isActive);
                    break;
            }
        }
    }

    private void ActiveImg(Transform parent, bool isActive)
    {
        foreach (Transform child in parent)
        {
            Image img = child.GetComponent<Image>();
            img.enabled = isActive;
            img.raycastTarget = isActive;
        }
    }
}
