using System;
using UnityEngine;

public class FlowerLeaf : FlowerDraggable
{
	[SerializeField]
	private Transform endBone = null;
	[SerializeField]
	private Transform target = null;

	[NonSerialized, HideInInspector]
	public int leafIndex;

	private float leafSize;
	private Vector3 boneOffset;
	private Vector3 targetOffset;

	private Vector3 nextPos;
	private Vector3 lastPos;

	void Awake()
	{
		leafSize = 0f;
		var b = endBone;
		while(b != anchor && b.parent != null)
		{
			leafSize += Vector3.Distance(b.position, b.parent.position);
			b = b.parent;
		}
		boneOffset = endBone.position - anchor.position;
		targetOffset = target.localPosition;
		lastPos = anchor.position;
	}

	void FixedUpdate()
	{
		if(state == State.Drag)
		{
			var offset = anchor.forward * flower.leafDragCenterOffset * leafSize;
			var dir = nextPos - (anchor.position + offset);

			float dist = (new Vector2(dir.x, dir.y).magnitude / leafSize) - (flower.leafDragStart - flower.leafDragCenterOffset);
			if(dist > 0f)
			{
				dist *= Mathf.Clamp01(dist / (1f - flower.leafDragStart));
				if(!flower.MoveLeaf(anchor.position + dir.normalized * leafSize * dist))
				{
					state = State.None;
					flower.RemoveLeaf(leafIndex);
				}
			}
		}
		else if(state == State.None)
		{
			target.localPosition = Vector3.Lerp(target.localPosition, targetOffset + Vector3.ClampMagnitude(anchor.InverseTransformVector(lastPos - anchor.position), 0.25f), Time.deltaTime);
			lastPos = anchor.position + Vector3.Lerp(Vector3.ClampMagnitude(lastPos - anchor.position, 0.25f), Vector3.zero, Time.deltaTime * 8f);
		}
	}

	protected override void OnPick()
	{
		nextPos = anchor.position + offset;
		flower.SetState(FlowerFaceState.LeafTouch);
	}

	protected override void OnRelease()
	{
		flower.SetState(FlowerFaceState.None);
		lastPos = anchor.position;
	}

	protected override void OnStartDrag()
	{
		flower.SetState(FlowerFaceState.LeafDrag);
	}

	protected override void OnDrag(Vector3 newPos)
	{
		target.position = new Vector3(newPos.x, newPos.y, newPos.z - 1f);
		nextPos = newPos;
	}
}