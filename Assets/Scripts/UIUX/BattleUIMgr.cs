using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class BattleUIMgr : MonoBehaviour
{
    public Transform turnImgParent;
    public Transform allies; // 캐릭터 생성 위치
    public Transform enemies; // 적 생성 위치
    public Transform deck;
    public Transform hand;
    public Transform graveyard;
    public Transform cardInfo;
    public Transform actionPanel; // 연출UI

    public void UpdateCntByChildren(Transform trans)
    {
        trans.parent.GetComponentInChildren<Text>().text = trans.childCount.ToString();
    }

    public void CreateOrderImg((bool isEnemy, int id) order)
    {
        GameObject turnImg = new GameObject("TurnImg", typeof(RectTransform), typeof(Image));
        turnImg.transform.SetParent(turnImgParent, false);

        Image img = turnImg.GetComponent<Image>();
        img.raycastTarget = false;
        //if(order.isEnemy)
        //{
        //    img.sprite = ;
        //}
        //else
        //{
        //    img.sprite = InfoMgr.Instance.database;
        //}
    }

    public void UpdateOrderImg()
    {
        turnImgParent.GetChild(0).SetSiblingIndex(turnImgParent.childCount);
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

    public async void ActiveAction(bool isEnemy, string spriteRoot, List<Transform> targets)
    {
        float posY = 0f;
        Vector2 originSize = Vector2.zero; // 초기설정 복구용

        actionPanel.gameObject.SetActive(true);

        string mainTag = isEnemy ? "Enemy" : "Ally";

        foreach (Transform area in actionPanel)
        {
            if (area.CompareTag(mainTag))
            {
                // 시전자
                Image attacker = area.GetChild(0).GetComponent<Image>();
                posY = attacker.rectTransform.position.y;
                originSize = attacker.rectTransform.sizeDelta;

                attacker.enabled = true;
                await SetActionAsync(attacker, spriteRoot + "Idle");
            }
            else
            {
                // 피격자
                for (int i = 0; i < targets.Count; i++)
                {
                    string targetSpriteRoot = isEnemy
                        ? targets[i].GetComponent<PlayableChar>().spriteRoot
                        : targets[i].GetComponent<Monster>().spriteRoot;

                    Image img = area.GetChild(i).GetComponent<Image>();
                    img.enabled = true;
                    await SetActionAsync(img, targetSpriteRoot + "Idle");
                }
            }
        }

        await Task.Delay(1000); // 1초 대기

        foreach (Transform area in actionPanel)
        {
            if (area.CompareTag(mainTag))
            {
                Image attacker = area.GetChild(0).GetComponent<Image>();
                await SetActionAsync(attacker, spriteRoot + "Attack"); // 공격자 스프라이트
            }
        }
        // 애니 실행
        // 애니 종료 콜백 받으면 피격자 스프라이트
        await Task.Delay(1000);
        // 초기화 / position w/h 원상복구
        foreach (Transform area in actionPanel)
        {
            if (area.CompareTag(mainTag))
            {
                Image attacker = area.GetChild(0).GetComponent<Image>();
                attacker.enabled = false;
                attacker.sprite = null;
                attacker.rectTransform.position = new Vector2(attacker.rectTransform.position.x, posY);
                attacker.rectTransform.sizeDelta = originSize;
            }
        }

        actionPanel.gameObject.SetActive(false);
    }

    private async Task SetActionAsync(Image img, string spriteKey)
    {
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(spriteKey);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            img.sprite = handle.Result;

            if(spriteKey.Contains("Idle"))
            {
                AdjustRatio(handle.Result, img, true);
            }
            else
            {
                AdjustRatio(handle.Result, img, false);
            }
        }
        else
        {
            Debug.LogWarning($"[Addressables] Sprite Load 실패: {spriteKey}");
        }
    }

    private void AdjustRatio(Sprite sprite, Image targetImg, bool isIdle)
    {
        RectTransform imgTrans = targetImg.rectTransform;
        Vector2 originSize = imgTrans.sizeDelta;
        float originW = originSize.x;
        float originH = originSize.y;

        float ratioW = sprite.rect.width / 1000f;
        float ratioH = sprite.rect.height / 1000f;

        float targetW = originW * ratioW;
        float targetH = isIdle ? originW * ratioH : originH;
        imgTrans.sizeDelta = new Vector2(targetW, targetH);

        if(isIdle)  // 캐릭터별 높이 맞추기
        {
            Vector2 pos = imgTrans.anchoredPosition;
            float offsetY = Mathf.Abs(targetH - originH) * 0.5f;

            pos.y += (targetH > originH) ? offsetY : -offsetY;
            imgTrans.anchoredPosition = pos;
        }
    }
}
