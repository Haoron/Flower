using System;
using UnityEngine;

public class FlowerLeaf : FlowerDraggable
{
	[SerializeField]
	private float leafSize = 2f;

	[NonSerialized, HideInInspector]
	public int leafIndex;

	private Vector3 scale;
	private Quaternion rotation;

	void Awake()
	{
		scale = anchor.localScale;
		rotation = anchor.localRotation;
	}

	protected override void OnPick()
	{
		flower.SetState(FlowerState.LeafTouch);
	}

	protected override void OnRelease()
	{
		flower.SetState(FlowerState.None);
		anchor.localScale = scale;
		anchor.localRotation = rotation;
	}

	protected override void OnStartDrag()
	{
		flower.SetState(FlowerState.LeafDrag);
	}

	protected override void OnDrag(Vector3 newPos)
	{
		var dir = newPos - anchor.position;
		anchor.localScale = scale + Vector3.forward * (Vector3.Dot(dir, offset) / leafSize) * (offset.magnitude / leafSize);
		anchor.localRotation = rotation * Quaternion.Euler(0f, Vector3.SignedAngle(offset.normalized, ((newPos + offset) - anchor.position).normalized, Vector3.back) * (offset.magnitude / leafSize), 0f);
		if(!flower.MoveLeaf(anchor.position + dir * (Mathf.Abs(Vector3.Dot(dir, offset) / leafSize))))
		{
			state = State.None;
			flower.RemoveLeaf(leafIndex);
		}
	}
}