using UnityEngine;

public class DraggableCommand : MonoBehaviour, IDraggable
{
    public Command Command { get; set; }

    [SerializeField] private bool instantAnim;

    public void OnClick()
    {

    }

    public void OnDrag()
    {
        if (Command == null) return;

        CommandDragVisual.SetCommand(Command, instantAnim);
    }

    public void OnDrop(DraggableReceiver receiver)
    {
        CommandDragVisual.SetCommand(null, CommandQueueUI.InstantlyDestroyNextDraggable);
        CommandQueueUI.InstantlyDestroyNextDraggable = false;
    }
}
