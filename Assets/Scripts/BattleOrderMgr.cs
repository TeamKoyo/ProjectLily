using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleOrderMgr : MonoBehaviour
{
    private Dictionary<Transform, int> diceToOrderIndex = new Dictionary<Transform, int>(); // id 매핑
    private List<(int val, int idx, bool isEnemy, int id)> order; // (dice값, 자식인덱스, 피아식별, 정보ID)
    private float rollDuration; // 굴리는 시간
    private float rollInterval; // sprite 변경 간격
    private int coCnt; // coroutine 동기화용
    private int idx; // 현재 order 인덱스

    public Sprite[] rollSprites; // 회전
    public Sprite[] diceSprites; // 1~20
    public GameObject dicePanel; // DicePhase UI
    public Transform allies;
    public Transform enemies;
    public GameObject dicePrefab;

    public event Action OnDicePhaseEnd; // 코루틴 종료 이벤트

    private void Start()
    {
        order = new List<(int val, int idx, bool isEnemy, int id)>();
        rollDuration = 1.5f;
        rollInterval = 0.05f;
        coCnt = 0;
        idx = 0;
    }

    public void CreateDice(int id, bool isEnemy)
    {
        GameObject slot;
        if (isEnemy)
        {
            slot = Instantiate(dicePrefab, enemies);
        }
        else
        {
            slot = Instantiate(dicePrefab, allies);
        }
        Image img = slot.transform.GetChild(0).GetComponent<Image>(); // id를 통해 스프라이트 탐색 필요

        order.Add((0, 0, isEnemy, id));
        diceToOrderIndex[slot.transform] = order.Count - 1;
    }

    public void RollDice()
    {
        RollDice(allies);
        RollDice(enemies);
    }

    private void RollDice(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            int idx = i; // closure issue 방지
            foreach (Transform dice in parent.GetChild(i))
            {
                int orderIndex = diceToOrderIndex[parent.GetChild(i)];

                if (dice.CompareTag("Dice"))
                {
                    StartCoroutine(Roll(rollDuration, rollInterval, dice.GetComponent<Image>(), (val) =>
                    {
                        (int val, int idx, bool isEnemy, int id) item = order[orderIndex];
                        order[orderIndex] = (val, idx, item.isEnemy, item.id);

                        dice.GetComponentInChildren<Text>().text = val.ToString();
                        ChkFinishCo();
                    }));

                    break;
                }
            }
        }
    }

    private IEnumerator Roll(float duration, float interval, Image dice, System.Action<int> onComplete)
    {
        int idx = 0; // roll 전용 변수
        float time = 0;
        int val;

        while (time < duration)
        {
            dice.sprite = rollSprites[idx];
            idx = (idx + 1) % rollSprites.Length; // rollSprite 순차적용

            time += interval;
            yield return new WaitForSeconds(interval);
        }

        val = UnityEngine.Random.Range(1, diceSprites.Length + 1);
        dice.sprite = diceSprites[val - 1];

        onComplete?.Invoke(val);
    }

    private void ChkFinishCo()
    {
        coCnt++;

        if (coCnt >= allies.childCount + enemies.childCount)
        {
            coCnt = 0;
            StartCoroutine(EndDicePhase());
        }
    }

    private void SortOrder() // 순서 결정
    {
        order.Sort((a, b) =>
        {
            int cmp = b.val.CompareTo(a.val); // val descending
            if (cmp != 0) // 같은 val면 다음 조건
            {
                return cmp;
            }

            if (a.isEnemy != b.isEnemy) // 같은 그룹이면 다음 조건
            {
                return a.isEnemy ? -1 : 1; // true = 앞으로(-1) false = 뒤로(1) -> 적 우선권
            }

            return a.idx.CompareTo(b.idx); // 전열부터
        });
    }

    private IEnumerator EndDicePhase()
    {
        float inactiveTime = 1.5f;

        // 보정값 관련 코드 필요
        SortOrder();

        yield return new WaitForSeconds(inactiveTime);
        dicePanel.SetActive(false);
        OnDicePhaseEnd?.Invoke();
    }

    public (bool isEnemy, int id) ChkOrder()
    {
        (bool isEnemy, int id) recentOrder;

        recentOrder = (order[idx].isEnemy, order[idx].id);
        idx = (idx + 1) % order.Count;

        return recentOrder;
    }
}
