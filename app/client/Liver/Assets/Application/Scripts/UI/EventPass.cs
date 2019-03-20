using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace Liver
{

[RequireComponent(typeof(Button))]
public class EventPass : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject Target;


    public void OnBeginDrag(PointerEventData eventData)
    {
        //GetComponent<Button>().enabled = false; // クリックイベントを無効にする
        Target.SendMessage("OnBeginDrag", eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Target.SendMessage("OnDrag", eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Target.SendMessage("OnEndDrag", eventData);
        //GetComponent<Button>().enabled = true; // クリックイベントを有効にする
    }
}

}
