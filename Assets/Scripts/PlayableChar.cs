public class PlayableChar : Character
{
    private CharData data;

    public void SetData(int charId)
    {
        data = InfoMgr.Instance.database.chars.Find(c => c.charId == charId);
        id = data.charId;

        LoadStatus(charId);
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
