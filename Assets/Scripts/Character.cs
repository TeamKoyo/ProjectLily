using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public abstract class Character : MonoBehaviour
{
    protected CharStatus status = new CharStatus();
    protected AsyncOperationHandle<Sprite> handle;

    public int id;
    public string spriteRoot;
    public Image img;
    public RectTransform hp;
    public int orderIdx; // turnImg 구분용

    public static event Action<Character> OnCharDeath;

    public abstract void SetData(int id);

    public virtual async void SetSprite(string key)
    {
        handle = Addressables.LoadAssetAsync<Sprite>(spriteRoot + key);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            AdjustRatio(handle.Result);
        }
    }

    protected virtual void AdjustRatio(Sprite sprite) // coyote기준
    {
        float w = sprite.rect.width;
        float h = sprite.rect.height;
        float ratio = h / w;

        float targetW = img.rectTransform.sizeDelta.x;
        float targetH = targetW * ratio;

        img.sprite = sprite;
        img.rectTransform.sizeDelta = new Vector2(targetW, targetH);

        RectTransform imgTrans = img.rectTransform;
        RectTransform parentTrans = img.transform.parent.GetComponent<RectTransform>();
        Vector3 pos = imgTrans.anchoredPosition;

        float targetY = (imgTrans.sizeDelta.y - parentTrans.sizeDelta.y) / 2;

        if (targetY < 0) // targetUI를 발에 맞춤
        {
            pos.y = targetY;
        }
        else
        {
            pos.y = -targetY;
        }

        imgTrans.anchoredPosition = pos;
    }

    protected abstract int GetMaxHp();

    public void Damage(int val)
    {
        status.hp -= val;

        if (status.hp <= 0)
        {
            status.hp = 0;
            OnCharDeath?.Invoke(this);
        }

        UpdateHp();
    }

    public void Heal(int val)
    {
        status.hp = Mathf.Min(status.hp + val, GetMaxHp());

        UpdateHp();
    }

    public void UpdateHp()
    {
        Slider hpBar = hp.GetComponent<Slider>();
        hpBar.value = (float)status.hp / GetMaxHp();

        Text hpTxt = hp.GetComponentInChildren<Text>();
        hpTxt.text = status.hp + " / " + GetMaxHp();
    }

    protected virtual void OnDestroy()
    {
        Addressables.Release(handle);
    }
}