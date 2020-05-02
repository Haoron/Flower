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
	private Vector3 targetScale;
	private Quaternion targetRotation;
	private Vector3 lastPos;

	void Awake()
	{
		targetScale = scale = anchor.localScale;
		targetRotation = rotation = anchor.localRotation;
		lastPos = anchor.position;
	}

	void LateUpdate()
	{
		if(state == State.None)
		{
			anchor.localScale = Vector3.MoveTowards(anchor.localScale, targetScale, Time.deltaTime * leafSize * 0.5f);
			anchor.localRotation = Quaternion.RotateTowards(anchor.localRotation, targetRotation, Time.deltaTime * 90f * 0.5f);

			if(Mathf.Approximately(lastPos.sqrMagnitude, anchor.position.sqrMagnitude))
			{
				targetScale = Vector3.MoveTowards(targetScale, scale, Time.deltaTime * leafSize * 0.5f);
				targetRotation = Quaternion.RotateTowards(targetRotation, rotation, Time.deltaTime * 90f * 0.5f);
			}
			else
			{
				var dir = lastPos - anchor.position;
				targetScale = scale + Vector3.forward * (Vector3.Dot(dir, anchor.forward) / leafSize);
				targetRotation = rotation * Quaternion.Euler(0f, Vector3.SignedAngle(anchor.forward, dir.normalized, Vector3.back) / 30f, 0f);
				lastPos = anchor.position;
			}
		}
	}

	protected override void OnPick()
	{
		flower.SetState(FlowerState.LeafTouch);
	}

	protected override void OnRelease()
	{
		flower.SetState(FlowerState.None);
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