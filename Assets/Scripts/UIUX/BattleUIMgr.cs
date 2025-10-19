using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

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
        // 이미지 전부 idle상태 -> 조금 대기 -> 공격자 스프라이트 -> 애니메이션 실행 -> 종료시 피격자 스프라이트 및 이펙트 -> 종료
        actionPanel.gameObject.SetActive(true);

        string mainTag = isEnemy ? "Enemy" : "Ally";

        foreach (Transform area in actionPanel)
        {
            if (area.CompareTag(mainTag))
            {
                // 시전자
                await SetActionAsync(area.GetChild(0).GetComponent<Image>(), spriteRoot + "Idle");
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
                    await SetActionAsync(img, targetSpriteRoot + "Idle");
                }
            }
        }
    }

    private async Task SetActionAsync(Image img, string spriteKey)
    {
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(spriteKey);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            img.sprite = handle.Result;
            AdjustRatio(handle.Result, img);
        }
        else
        {
            Debug.LogWarning($"[Addressables] Sprite Load 실패: {spriteKey}");
        }
    }

    private void AdjustRatio(Sprite sprite, Image targetImg)
    {
        float w = sprite.rect.width;
        float h = sprite.rect.height;
        float ratio = h / w;

        float targetW = targetImg.rectTransform.sizeDelta.x;
        float targetH = targetW * ratio;

        targetImg.rectTransform.sizeDelta = new Vector2(targetW, targetH);
    }

    public void PlayMoveArrow(bool toRight, bool enemyFront)
    {
        //// 방향 설정 (오른쪽 true, 왼쪽 false)
        //Vector3 allyScale = allyArrow.localScale;
        //Vector3 enemyScale = enemyArrow.localScale;

        //float dir = toRight ? 1f : -1f;
        //allyScale.x = Mathf.Abs(allyScale.x) * dir;
        //enemyScale.x = Mathf.Abs(enemyScale.x) * dir;

        //allyArrow.localScale = allyScale;
        //enemyArrow.localScale = enemyScale;

        //// 겹쳤을 때 순서
        //if (!enemyFront) // last X -> 2번째자식
        //    allyArrow.SetAsLastSibling();
        //else
        //    enemyArrow.SetAsLastSibling();

        //// 애니메이션 실행
        //animator.Play("ArrowMove", -1, 0f);
    }
}
