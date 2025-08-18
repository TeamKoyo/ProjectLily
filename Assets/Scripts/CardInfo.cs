using UnityEngine;
using UnityEngine.UI;

public class CardInfo : MonoBehaviour
{
    private CardData _cardInfo; // 정보

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

    public void Init(CardData cardInfo)
    {
        _cardInfo = cardInfo;
        id = _cardInfo.cardId;
    }

    private void SetCard()
    {
        // 이미지 등 자동 설정
    }
}
