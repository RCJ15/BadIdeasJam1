using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableReceiver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDraggableReceiver
{
    public ComponentCollection<IDraggableReceiver> Receivers => draggableReceivers;
    [CacheComponent(AlwaysCache = true)]
    [SerializeField]
    private ComponentCollection<IDraggableReceiver> draggableReceivers;

    private void OnDisable()
    {
        DraggableManager.ActiveReceivers.Remove(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        DraggableManager.ActiveReceivers.Add(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DraggableManager.ActiveReceivers.Remove(this);
    }

    public void OnDrop(DraggableObject draggableObj)
    {
        foreach (IDraggableReceiver draggableReceiver in draggableReceivers)
        {
            draggableReceiver.OnDrop(draggableObj);
        }
    }
}
