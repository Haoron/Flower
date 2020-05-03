using UnityEngine;

public class FlowerFace : FlowerDraggable
{
	protected override void OnPick()
	{
		flower.SetState(FlowerFaceState.FaceTouch);
	}

	protected override void OnRelease()
	{
		flower.SetState(FlowerFaceState.None);
	}

	protected override void OnStartDrag()
	{
		flower.SetState(FlowerFaceState.FaceDrag);
	}

	protected override void OnDrag(Vector3 newPos)
	{
		flower.MoveFace(newPos - offset);
	}
}