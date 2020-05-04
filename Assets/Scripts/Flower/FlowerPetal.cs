using System;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class FlowerPetal : FlowerDraggable
{
	[SerializeField]
	private Transform endBone = null;
	[SerializeField]
	private Transform target = null;
	public Color color;

	[NonSerialized, HideInInspector]
	public int index;
	[NonSerialized, HideInInspector]
	public int side;
	[NonSerialized, HideInInspector]
	public Quaternion targetRotation;

	private float petalSize;
	private Vector3 boneOffset;
	private Vector3 targetOffset;

	private Vector3 nextPos;
	private Vector3 lastPos;
	private Renderer[] renderers = null;
	private MaterialPropertyBlock block = null;

	void Awake()
	{
#if UNITY_EDITOR
		if(!Application.isPlaying) return;
#endif
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

		renderers = GetComponentsInChildren<Renderer>();
		block = new MaterialPropertyBlock();
	}

#if UNITY_EDITOR
	void OnValidate()
	{
		if(renderers == null) renderers = GetComponentsInChildren<Renderer>();
		if(block == null) block = new MaterialPropertyBlock();
	}

	void Update()
	{
		for(int i = 0; i < renderers.Length; i++)
		{
			renderers[i].GetPropertyBlock(block);
			block.SetColor("_Color", color);
			renderers[i].SetPropertyBlock(block);
		}
	}
#endif

	void FixedUpdate()
	{
#if UNITY_EDITOR
		if(!Application.isPlaying) return;
#endif
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