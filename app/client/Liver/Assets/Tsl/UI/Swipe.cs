using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tsl.UI
{

[RequireComponent(typeof(EventTrigger))]
public class Swipe : MonoBehaviour
{
    public bool EnableScrollDirectionX = false;
    public bool EnableScrollDirectionY = false;
    // Swipe中に毎フレーム呼び出されるコールバック。第二引数がtrueの場合、スワイプ終了
    public System.Action<Vector3, bool> OnSwiping  = null;
    public Rect DragRange = new Rect(0, 0, 720, 1280); // ドラッグ可能範囲

    private Vector3 originalPos;
    private Vector2 dragPos;

    void Start()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        trigger.triggers = new List<EventTrigger.Entry>();
        EventTrigger.Entry entryBeginDrag = new EventTrigger.Entry();
        EventTrigger.Entry entryEndDrag = new EventTrigger.Entry();
        EventTrigger.Entry entryDrag = new EventTrigger.Entry();
        entryBeginDrag.eventID = EventTriggerType.BeginDrag;
        entryEndDrag.eventID = EventTriggerType.EndDrag;
        entryDrag.eventID = EventTriggerType.Drag;
        entryBeginDrag.callback.AddListener(ed =>
        {
            var pd = ed as PointerEventData;
            Debug.Log("BeginDrag: " + pd.ToString());
            this.dragPos = pd.position;
            this.originalPos = this.transform.localPosition;
            this.OnSwiping(this.originalPos, false);
        });
        entryEndDrag.callback.AddListener(ed =>
        {
            var pd = ed as PointerEventData;
            Debug.Log("EndDrag: " + pd.ToString());

            if (this.OnSwiping != null)
            {
                this.OnSwiping(this.transform.localPosition, true);
            }
        });
        entryDrag.callback.AddListener(ed =>
        {
            var pd = ed as PointerEventData;
            Vector3 delta = pd.position - this.dragPos;

            if (!this.EnableScrollDirectionX) { delta.x = 0.0f; }

            if (!this.EnableScrollDirectionY) { delta.y = 0.0f; }

            delta.z = 0.0f;
            var pos = this.originalPos + delta;
            pos.x = Mathf.Clamp(pos.x, this.DragRange.xMin, this.DragRange.xMax);
            pos.y = Mathf.Clamp(pos.y, this.DragRange.yMin, this.DragRange.yMax);

            if (this.OnSwiping != null)
            {
                this.OnSwiping(pos, false);
            }
            else
            {
                this.transform.localPosition = pos;
            }
        });
        trigger.triggers.Add(entryBeginDrag);
        trigger.triggers.Add(entryEndDrag);
        trigger.triggers.Add(entryDrag);
    }


    // Update is called once per frame
    void Update()
    {
    }
}


}
