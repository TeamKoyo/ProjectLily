public class PlayableChar : Character
{
    private CharData data;

    public override void SetData(int charId)
    {
        data = InfoMgr.Instance.database.chars.Find(c => c.charId == charId);
        id = data.charId;
        spriteRoot = data.charName + '_';

        LoadStatus(charId);

        SetSprite("Idle");
        UpdateHp();
    }

    protected override int GetMaxHp()
    {
        return data.maxHp;
    }

    public int GetDrawCnt()
    {
        return data.drawPoint;
    }

    public void SaveStatus()
    {
        InfoMgr.Instance.SaveStatus(status);
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
            status.hp = data.maxHp;
        }
    }
}
