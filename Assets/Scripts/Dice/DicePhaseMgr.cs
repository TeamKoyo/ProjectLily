using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DicePhaseMgr : MonoBehaviour
{
    private List<(int val, int idx, bool isEnemy)> diceVal; // 순서 정렬을 위한 list
    private float rollDuration; // 굴리는 시간
    private float rollInterval; // sprite 변경 간격
    private int coCnt; // coroutine 동기화용

    public Sprite[] rollSprites; // 회전
    public Sprite[] diceSprites; // 1~20
    public GameObject dicePanel; // DicePhase UI
    public Transform allies;
    public Transform enemies;
    public GameObject dicePrefab;

    private void Start()
    {
        diceVal = new List<(int val, int idx, bool isEnemy)>();
        rollDuration = 1.5f;
        rollInterval = 0.05f;
        coCnt = 0;
    }

    public void CreateSlot(int id, bool isEnemy)
    {
        if(isEnemy)
        {
            GameObject slot = Instantiate(dicePrefab, enemies);
            Image img = slot.transform.GetChild(0).GetComponent<Image>();
        }
        else
        {
            GameObject slot = Instantiate(dicePrefab, allies);
            Image img = slot.transform.GetChild(0).GetComponent<Image>(); // 스프라이트 탐색 필요
        }
    }

    public void RollDice()
    {
        diceVal.Clear();

        for (int i = 0; i < allies.childCount; i++)
        {
            int idx = i; // closure issue 방지
            foreach(Transform dice in allies.GetChild(i))
            {
                if(dice.CompareTag("Dice"))
                {
                    StartCoroutine(Roll(rollDuration, rollInterval, dice.GetComponent<Image>(), (val) =>
                    {
                        diceVal.Add((val, idx, false));
                        dice.GetComponentInChildren<Text>().text = val.ToString();
                        ChkFinishCo();
                    }));

                    break;
                }
            }

        }

        for (int i = 0; i < enemies.childCount; i++)
        {
            int idx = i;
            foreach (Transform dice in enemies.GetChild(i))
            {
                if (dice.CompareTag("Dice"))
                {
                    StartCoroutine(Roll(rollDuration, rollInterval, dice.GetComponent<Image>(), (val) =>
                    {
                        diceVal.Add((val, idx, true));
                        dice.GetComponentInChildren<Text>().text = val.ToString();
                        ChkFinishCo();
                    }));

                    break;
                }
            }
        }
    }

    public IEnumerator Roll(float duration, float interval, Image dice, System.Action<int> onComplete)
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

        val = Random.Range(1, diceSprites.Length + 1);
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
        diceVal.Sort((a, b) =>
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

            return a.idx.CompareTo(b.idx); // idx ascending
        });
    }

    public IEnumerator EndDicePhase()
    {
        float inactiveTime = 1.5f;

        // 보정값 관련 코드 필요
        SortOrder();

        yield return new WaitForSeconds(inactiveTime);
        dicePanel.SetActive(false);
    }
}
