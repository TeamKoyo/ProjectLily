using System;
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
    }

    #region Card
    public GameObject cardPrefab;

    private int[] selectedCardId = new int[] { 0, 1, 2 };
    public int[] GetCardIds()
    {
        return selectedCardId;
    }
    #endregion

    #region Char
    public GameObject infoPrefab;

    #region Character
    private int[] selectedCharIds = new int[] { 0, 1, 2 };
    public int[] GetCharIds()
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