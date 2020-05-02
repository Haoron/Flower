using UnityEngine;

public class FlowerFace : FlowerDraggable
{
    protected override void OnPick()
    {
		flower.SetState(FlowerState.FaceTouch);
    }

    protected override void OnRelease()
    {
		flower.SetState(FlowerState.None);
    }

    protected override void OnStartDrag()
    {
		flower.SetState(FlowerState.FaceDrag);
    }

    protected override void OnDrag(Vector3 newPos)
    {
		flower.MoveFace(newPos);
    }
}