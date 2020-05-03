using System;
using UnityEngine;

public class FlowerPetal : FlowerDraggable
{
	[SerializeField]
	private Transform endBone = null;
	[SerializeField]
	private Transform target = null;

	[NonSerialized, HideInInspector]
	public int index;
	[NonSerialized, HideInInspector]
	public int side;
	//[NonSerialized, HideInInspector]
	public Color color;

	private float petalSize;
	private Vector3 boneOffset;
	private Vector3 targetOffset;

	private Vector3 nextPos;
	private Vector3 lastPos;

	void Awake()
	{
		petalSize = 0f;
		var b = endBone;
		while(b != anchor && b.parent != null)
		{
			petalSize += Vector3.Distance(b.position, b.parent.position);
			b = b.parent;
		}
		boneOffset = endBone.position - anchor.position;
		targetOffset = target.localPosition;
		lastPos = anchor.position;
		
		var renderers = GetComponentsInChildren<Renderer>();
		for(int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material.color = color;
		}
	}

	void FixedUpdate()
	{
		if(state == State.Drag)
		{
			var offset = anchor.forward * flower.petalDragCenterOffset * petalSize;
			var dir = nextPos - (anchor.position + offset);

			float dist = (new Vector2(dir.x, dir.y).magnitude / petalSize) - (flower.petalDragStart - flower.petalDragCenterOffset);
			if(dist > 0f)
			{
				dist *= Mathf.Clamp01(dist / (1f - flower.petalDragStart));
				if(!flower.MovePetal(anchor.position + dir.normalized * petalSize * dist))
				{
					state = State.None;
					flower.RemovePetal(index);
				}
			}
		}
		else if(state == State.None)
		{
			target.localPosition = Vector3.Lerp(target.localPosition, targetOffset + Vector3.ClampMagnitude(anchor.InverseTransformVector(lastPos - anchor.position), 0.25f), Time.deltaTime);
			lastPos = anchor.position + Vector3.Lerp(Vector3.ClampMagnitude(lastPos - anchor.position, 0.25f), Vector3.zero, Time.deltaTime * 8f);
		}
	}

	public void MoveTo(Quaternion rotation)
	{
		anchor.localRotation = rotation;
	}

	protected override void OnPick()
	{
		nextPos = anchor.position + offset;
		target.position = new Vector3(nextPos.x, nextPos.y, nextPos.z - 1f);
		flower.SetState(FlowerFaceState.PetalTouch);
	}

	protected override void OnRelease()
	{
		flower.SetState(FlowerFaceState.None);
		lastPos = anchor.position;
	}

	protected override void OnStartDrag()
	{
		flower.SetState(FlowerFaceState.PetalDrag);
	}

	protected override void OnDrag(Vector3 newPos)
	{
		target.position = new Vector3(newPos.x, newPos.y, newPos.z - 1f);
		nextPos = newPos;
	}
}