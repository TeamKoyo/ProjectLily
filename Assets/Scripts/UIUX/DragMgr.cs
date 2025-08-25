using UnityEngine;
using System.Collections;

public class DragMgr : MonoBehaviour
{
    public static DragMgr Instance { get; private set; }

    private RectTransform dragObj;

    public Canvas canvas; // cam 참조용

    [HideInInspector]
    public bool isDrag = false;

    private void Awake() => Instance = this;

    public void BeginDrag(RectTransform obj)
    {
        dragObj = obj;
        isDrag = true;

        // middle anchor (보정)
        dragObj.anchorMin = new Vector2(0.5f, 0.5f);
        dragObj.anchorMax = new Vector2(0.5f, 0.5f);

        dragObj.localRotation = Quaternion.identity; // 회전값 0

        StartCoroutine(Drag());
    }

    public void EndDrag()
    {
        StopCoroutine(Drag());

        dragObj = null;
        isDrag = false;
    }

    private IEnumerator Drag()
    {
        while (dragObj != null)
        {
            Vector2 pos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragObj.parent as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out pos
            ); // world -> rect(UI)

            dragObj.anchoredPosition = pos;

            yield return null; // 다음 프레임
        }
    }
}
