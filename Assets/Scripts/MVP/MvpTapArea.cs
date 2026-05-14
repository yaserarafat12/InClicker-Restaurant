using UnityEngine;
using UnityEngine.EventSystems;

namespace InUniverse.InResto
{
    public class MvpTapArea : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (RushHourManager.Instance != null)
            {
                RushHourManager.Instance.OnPlayerTap(eventData.position);
            }
        }
    }
}

