using System.Collections.Generic;
using UnityEngine;

public class InfoMgr : MonoBehaviour
{
    public static InfoMgr Instance { get; private set; } // 읽기 전용 싱글톤 패턴
    public SODatabase database;

    private void Awake() // 싱글톤 초기화
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SelectCard();
        SelectChar();
    }

    #region Card
    public GameObject cardPrefab;

    private List<int> selectedCardId = new List<int>();

    private void SelectCard() // DB에서 가져오는게 아니라 변형 필요
    {
        foreach(CardData data in database.cards)
        {
            selectedCardId.Add(data.cardId);
        }
    }
    public List<int> GetCardIds()
    {
        return selectedCardId;
    }
    #endregion

    #region Character
    public GameObject charPrefab;

    private List<int> selectedCharIds = new List<int>();

    private void SelectChar() // DB에서 가져오는게 아니라 변형 필요
    {
        foreach (CharData data in database.chars)
        {
            selectedCharIds.Add(data.charId);
        }
    }
    public List<int> GetCharIds()
    {
        return selectedCharIds;
    }
    #endregion

    #region Enemy
    private int[] monsterIds = new int[] { 0, 1, 2 };
    public int[] GetMonsterIds()
    {
        return monsterIds;
    }
    #endregion

    #region Status
    private List<CharStatus> statuses = new List<CharStatus>();

    public CharStatus LoadStatus(int charId)
    {
        CharStatus status = statuses.Find(state => state.charId == charId);

        if(status != null)
        {
            return status;
        }
        else
        {
            return null;
        }
    }
    #endregion
}

public class CharStatus
{
    public int charId;
    public int hp;
}