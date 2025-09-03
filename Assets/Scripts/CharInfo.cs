using UnityEngine;
using UnityEngine.UI;

public class CharInfo : MonoBehaviour
{
    private CharData data;
    private CharStatus status;

    public int id;
    public Image img;
    public RectTransform hp;

    public void SetData(int charId)
    {
        //data = InfoMgr.Instance.database.chars.Find(c => c.charId == charId);
        //id = data.charId;
        id = charId;

        LoadStatus(charId);
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
            //status.charId = data.charId;
            //status.hp = data.maxHp;
        }
    }

    public void UpdateStatus()
    {
        Text hpTxt = hp.GetComponentInChildren<Text>();
        hpTxt.text = status.hp + " / " + data.maxHp;
    }

    public void SaveStatus()
    {

    }
}
