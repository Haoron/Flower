using UnityEngine;

public class FlowerFace : FlowerDraggable
{
	protected override void OnPick()
	{
		flower.PlaySound(FlowerSound.FaceTouch);
		flower.SetState(FlowerState.FaceTouch);
	}

	protected override void OnRelease()
	{
		flower.PlaySound(FlowerSound.FaceDrop);
		flower.SetState(FlowerState.None);
	}

	protected override void OnStartDrag()
	{
		flower.PlaySound(FlowerSound.FaceDrag);
		flower.SetState(FlowerState.FaceDrag);
	}

	protected override void OnDrag(Vector3 newPos)
	{
		flower.MoveFace(newPos - offset);
	}
}