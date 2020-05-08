using System;
using DitzelGames.FastIK;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class FlowerPetal : FlowerDraggable
{
	[SerializeField]
	private PetalDrop petalDropPrefab = null;
	[SerializeField]
	private Transform endBone = null;
	[SerializeField]
	private FastIKFabric ik = null;
	[SerializeField]
	private Transform target = null;
	[SerializeField]
	private Transform[] bones = null;
	[SerializeField]
	private Vector3 pickOffset = Vector3.back;

	[NonSerialized, HideInInspector]
	public int index;
	[NonSerialized, HideInInspector]
	public int side;
	[NonSerialized, HideInInspector]
	public Quaternion targetRotation;

	public Color color { get; private set; }

	private float petalSize;
	private Vector3 targetOffset;

	private Vector3 nextPos;
	private Vector3 lastPos;
	private Renderer[] renderers = null;
	private MaterialPropertyBlock block = null;
	private bool isInited = false;

	public void Init(Quaternion rotation, Color color, int side)
	{
		this.color = color;
		this.side = side;
		anchor.localScale = Vector3.one;
		anchor.localRotation = rotation;
		targetRotation = rotation;

		lastPos = anchor.position;

		if(!isInited) InitInternal();
		target.localPosition = targetOffset;
	}

#if UNITY_EDITOR
	void OnValidate()
	{
		if(renderers == null) renderers = GetComponentsInChildren<Renderer>();
		if(block == null) block = new MaterialPropertyBlock();
	}
#endif

	void Update()
	{
		for(int i = 0; i < renderers.Length; i++)
		{
			renderers[i].GetPropertyBlock(block);
			block.SetColor("_Color", color);
			renderers[i].SetPropertyBlock(block);
		}
	}

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

					var drop = PetalDrop.GetInstance(petalDropPrefab);
					this.eventData.pointerDrag = drop.gameObject;
					this.eventData = null;
					drop.Init(color, bones);

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

	private void InitInternal()
	{
		isInited = true;

		petalSize = 0f;
		var b = endBone;
		while(b != anchor && b.parent != null)
		{
			petalSize += Vector3.Distance(b.position, b.parent.position);
			b = b.parent;
		}

		targetOffset = target.localPosition;
		ik.Init();

		renderers = GetComponentsInChildren<Renderer>();
		block = new MaterialPropertyBlock();
	}

	protected override void OnPick()
	{
		flower.PlaySound(FlowerSound.PetalTouch);
		nextPos = anchor.position + offset;
		target.position = nextPos + pickOffset;
		flower.SetState(FlowerState.PetalTouch);
	}

	protected override void OnRelease()
	{
		flower.PlaySound(FlowerSound.PetalDrop);
		flower.SetState(FlowerState.None);
		lastPos = anchor.position;
	}

	protected override void OnStartDrag()
	{
		flower.PlaySound(FlowerSound.PetalDrag);
		flower.SetState(FlowerState.PetalDrag);
	}

	protected override void OnDrag(Vector3 newPos)
	{
		target.position = nextPos + pickOffset;
		nextPos = newPos;
	}
}