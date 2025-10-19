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
        InstanceMonster();
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
        for(int i = 0; i < 3; i++)
        {
            selectedCharIds.Add(database.chars[i].charId);
        }
    }

    public List<int> GetCharIds()
    {
        return selectedCharIds;
    }
    #endregion

    #region Enemy
    public GameObject monsterPrefab;

    private List<int> monsterIds = new List<int>();

    private void InstanceMonster() // DB에서 가져오는게 아니라 변형 필요
    {
        foreach(MonsterData data in database.monsters)
        {
            monsterIds.Add(data.monsterId);
        }
    }

    public List<int> GetMonsterIds()
    {
        return monsterIds;
    }
    #endregion

    #region Status
    private List<CharStatus> statuses = new List<CharStatus>();

    public void SaveStatus(CharStatus status)
    {
        statuses.Add(status);
    }

    public CharStatus LoadStatus(int charId)
    {
        CharStatus status = statuses.Find(state => state.id == charId);

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
    public int id;
    public int hp;
}