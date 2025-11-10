using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class BattleUIMgr : MonoBehaviour
{
    private int startCnt = 0; // 스프라이트 변경 코루틴 호출 수
    private int endCnt = 0; // 끝난 코루틴 수

    public Transform turnImgParent;
    public Transform allies; // 캐릭터 생성 위치
    public Transform enemies; // 적 생성 위치
    public Transform deck;
    public Transform hand;
    public Transform graveyard;
    public Transform cardInfo;
    public Transform result; // 결과창

    public event Action OnSpriteChangeHit;
    public event Action OnSpriteChangeFinished;
    public event Action OnCharDeath;

    public void UpdateCntByChildren(Transform trans)
    {
        trans.parent.GetComponentInChildren<Text>().text = trans.childCount.ToString();
    }

    public void CreateOrderImg((bool isEnemy, int id) order)
    {
        int orderIdx = turnImgParent.childCount;
        GameObject turnImg = new GameObject("TurnImg" + orderIdx, typeof(RectTransform), typeof(Image));
        turnImg.transform.SetParent(turnImgParent, false);

        Image img = turnImg.GetComponent<Image>();
        img.raycastTarget = false;

        Transform trans = order.isEnemy ? enemies : allies;
        foreach (Transform slot in trans)
        {
            if (slot.childCount < 1 && slot.gameObject.activeSelf)
            {
                break;
            }
            else if (!slot.gameObject.activeSelf)
            {
                continue;
            }

            if (order.isEnemy)
            {
                Monster monster = slot.GetComponentInChildren<Monster>();

                if (monster.id == order.id)
                {
                    StartCoroutine(CoSetSprite(img, monster.spriteRoot + "Profile"));
                    monster.orderIdx = orderIdx;
                }
            }
            else
            {
                PlayableChar character = slot.GetComponentInChildren<PlayableChar>();

                if (character.id == order.id)
                {
                    StartCoroutine(CoSetSprite(img, character.spriteRoot + "Profile"));
                    character.orderIdx = orderIdx;
                }
            }
        }
    }

    public void UpdateOrderImg()
    {
        turnImgParent.GetChild(0).SetSiblingIndex(turnImgParent.childCount);
    }

    public void DelOrderImg(int orderIdx)
    {
        foreach(Transform turnImg in turnImgParent)
        {
            if(turnImg.name.Contains(orderIdx.ToString()))
            {
                Destroy(turnImg.gameObject);
                break;
            }
        }
    }

    public void ActiveCardInfo(bool isActive, Transform card)
    {
        if(isActive) // 최적화 필요
        {
            cardInfo.GetChild(0).GetComponent<Image>().sprite = card.GetComponent<Card>().img.sprite;
            cardInfo.GetChild(1).GetComponent<Text>().text = card.GetComponent<Card>().cardName.text;
            cardInfo.GetChild(2).GetComponent<Text>().text = card.GetComponent<Card>().description.text;
        }

        cardInfo.gameObject.SetActive(isActive);
    }

    public void ActiveSlot(Transform slot, bool isactive)
    {
        Image img = slot.GetChild(0).GetComponent<Image>();

        if(img.enabled != isactive)
        {
            img.enabled = isactive;
        }
    }

    public IEnumerator CoChgSprite(Transform character, string spriteKey)
    {
        startCnt++;

        Vector2 originSize = character.GetChild(0).GetComponent<RectTransform>().sizeDelta; // Idle 크기 복구용
        Image characterImg = character.GetChild(0).GetComponent<Image>();
        Sprite idle = characterImg.sprite;

        yield return StartCoroutine(CoSetSprite(characterImg, spriteKey));
        AdjustRatio(characterImg, idle);

        yield return new WaitForSeconds(1f); // 1초 대기
        OnSpriteChangeHit?.Invoke(); // deathChar
        yield return null; // destroy 처리
        OnCharDeath?.Invoke(); // chkResult

        if (characterImg != null)
        {
            characterImg.sprite = idle;
            characterImg.rectTransform.sizeDelta = originSize;
        }

        endCnt++;
    }

    public IEnumerator CoSetSprite(Image img, string spriteKey)
    {
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(spriteKey);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            img.sprite = handle.Result;
        }
        else
        {
            Debug.LogWarning($"[Addressables] Sprite Load 실패: {spriteKey}");
        }
    }

    private void AdjustRatio(Image img, Sprite before)
    {
        RectTransform imgTrans = img.rectTransform;
        Vector2 originSize = imgTrans.sizeDelta;

        float ratioW = img.sprite.rect.width / before.rect.width;
        float ratioH = img.sprite.rect.height / before.rect.height;

        float targetW = originSize.x * ratioW;
        float targetH = originSize.y * ratioH;

        imgTrans.sizeDelta = new Vector2(targetW, targetH);
    }

    public IEnumerator CoWaitChgSprite()
    {
        yield return new WaitUntil(() => endCnt == startCnt);

        ChkResult();
        OnSpriteChangeFinished?.Invoke(); // endOrder
    }

    public void ChkResult()
    {
        OnCharDeath -= ChkResult;

        if(allies.GetChild(0).childCount > 0 && enemies.GetChild(0).childCount < 1)
        {
            result.gameObject.SetActive(true);
            result.GetComponentInChildren<Text>().text = "승리!";
        }
        else if(allies.GetChild(0).childCount < 1 && enemies.GetChild(0).childCount > 0)
        {
            result.gameObject.SetActive(true);
            result.GetComponentInChildren<Text>().text = "패배..";
        }
    }
}