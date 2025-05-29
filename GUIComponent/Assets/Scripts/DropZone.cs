using UnityEngine;
using UnityEngine.EventSystems;

namespace Bang
{
    public class DropZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;
            
            var card = eventData.pointerDrag.GetComponent<Card>();
            
            if (card)
                card.OverDropZone();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;
            
            var card = eventData.pointerDrag.GetComponent<Card>();
            
            if (card)
                card.OverDropZone();
        }
    }
}
