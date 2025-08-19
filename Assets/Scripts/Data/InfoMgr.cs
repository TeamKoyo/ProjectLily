using System.Data;
using UnityEngine;

public class InfoMgr : MonoBehaviour
{
    public static InfoMgr Instance { get; private set; } // 읽기 전용 싱글톤 패턴
    public SODatabase database;

    private void Awake() => Instance = this; // 싱글톤 초기화

    #region Card
    public GameObject cardPrefab;
    #endregion

    #region Character
    public GameObject charPrefab;
    #endregion

    #region Enemy
    public GameObject enemyPrefab;
    #endregion
}
