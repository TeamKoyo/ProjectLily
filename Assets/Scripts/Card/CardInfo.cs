using UnityEngine;
using UnityEngine.UI;

public class CardInfo : MonoBehaviour
{
    private CardData data; // 정보

    public int id;
    public Image[] img;
    public Text[] cost;
    public Text cardName;
    public Text description; // 효과 설명
    #region Exclusive
    public GameObject exclusive; // 전용 obj 부모
    public Image charImg;
    public Text charName;
    #endregion

    public void SetData(int cardId)
    {
        data = InfoMgr.Instance.database.cards.Find(c => c.cardId == cardId);
        id = data.cardId;
    }

    public string GetTargetType()
    {
        return data.targetType;
    }
}
