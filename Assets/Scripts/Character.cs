using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour, IEffect
{
    private BattleMgr battleMgr;
    private CharData data;
    private CharStatus status = new CharStatus();

    public int id;
    public Image img;
    public RectTransform hp;

    private void Start()
    {
        battleMgr = Object.FindFirstObjectByType<BattleMgr>();
    }

    public void SetData(int charId)
    {
        data = InfoMgr.Instance.database.chars.Find(c => c.charId == charId);
        id = data.charId;

        LoadStatus(charId);
    }

    public int GetDrawCnt()
    {
        return data.drawPoint;
    }

    public void SaveStatus()
    {

    }

    public void LoadStatus(int charId)
    {
        CharStatus loadStatus = InfoMgr.Instance.LoadStatus(charId);

        if (loadStatus != null)
        {
            status = loadStatus;
        }
        else
        {
            status.charId = data.charId;
            status.hp = data.maxHp;
        }

        UpdateStatus();
    }

    public void UpdateStatus()
    {
        Slider hpBar = hp.GetComponent<Slider>();
        hpBar.value = (float)status.hp / data.maxHp;

        Text hpTxt = hp.GetComponentInChildren<Text>();
        hpTxt.text = status.hp + " / " + data.maxHp;
    }

    public void Damage(int val)
    {
        status.hp -= val;
        UpdateStatus();
    }

    public void Heal(int val)
    {
        if (status.hp + val < data.maxHp)
        {
            status.hp += val;
        }
        else
        {
            status.hp = data.maxHp;
        }

        UpdateStatus();
    }
}
