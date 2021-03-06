﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PetalDrop : MonoBehaviour, IDragHandler, IEndDragHandler
{
	private static Dictionary<PetalDrop, Stack<PetalDrop>> petals;

	[SerializeField]
	private Transform target = null;
	[SerializeField]
	private Rigidbody targetRB = null;
	[SerializeField]
	private Transform[] bones = null;
	[SerializeField]
	private HingeJoint[] joints = null;
	[SerializeField]
	private AnimationCurve scaleOverLifeTime = null;
	[SerializeField]
	private AnimationCurve opacityOverLifeTime = null;

	private bool isDropped;
	private Color color;
	private PetalDrop prefab;
	private Renderer[] renderers = null;
	private MaterialPropertyBlock block = null;

	public void Init(Color color, Transform[] bones)
	{
		gameObject.SetActive(false);
		for(int i = 0; i < joints.Length; i++)
		{
			joints[i].autoConfigureConnectedAnchor = false;
		}
		this.color = color;
		for(int i = 0; i < bones.Length; i++)
		{
			this.bones[i].position = bones[i].position;
			this.bones[i].rotation = bones[i].rotation;
			this.bones[i].localScale = bones[i].lossyScale;
		}
		target.position = bones[bones.Length - 1].position;

		for(int i = 0; i < joints.Length; i++)
		{
			joints[i].connectedAnchor = joints[i].connectedAnchor;
		}
		targetRB.isKinematic = true;
		isDropped = false;
		for(int i = 0; i < renderers.Length; i++)
		{
			renderers[i].GetPropertyBlock(block);
			block.SetColor("_Color", color);
			renderers[i].SetPropertyBlock(block);
		}
		gameObject.SetActive(true);
	}

	void Awake()
	{
		renderers = GetComponentsInChildren<Renderer>();
		block = new MaterialPropertyBlock();
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

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		if(isDropped) return;
		target.position = WorldPos(eventData);
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		if(isDropped) return;
		targetRB.isKinematic = false;
		isDropped = true;
		StartCoroutine(DropRoutine());
	}

	private IEnumerator DropRoutine()
	{
		float time = 0f;
		float lifeTime = scaleOverLifeTime.keys[scaleOverLifeTime.length - 1].time;
		Vector3 fromScale = this.bones[0].localScale;
		while(time < lifeTime)
		{
			time += Time.deltaTime;
			Vector3 scale = fromScale * scaleOverLifeTime.Evaluate(Mathf.Min(time, lifeTime));
			for(int i = 0; i < bones.Length; i++)
			{
				this.bones[i].localScale = scale;
			}
			for(int i = 0; i < joints.Length; i++)
			{
				joints[i].connectedAnchor = joints[i].connectedAnchor;
			}
			color.a = opacityOverLifeTime.Evaluate(Mathf.Min(time, lifeTime));
			yield return null;
		}
		gameObject.SetActive(false);
		petals[prefab].Push(this);
	}

	private Vector3 WorldPos(PointerEventData eventData)
	{
		var ray = eventData.pressEventCamera.ScreenPointToRay(eventData.position);
		return ray.origin + ray.direction * ((-Vector3.Dot(ray.origin, Vector3.back) - target.position.z) / Vector3.Dot(ray.direction, Vector3.back));
	}

	public static PetalDrop GetInstance(PetalDrop prefab) //TODO: если будет реализован пул, переделать под него
	{
		if(petals == null) petals = new Dictionary<PetalDrop, Stack<PetalDrop>>();
		Stack<PetalDrop> stack;
		if(!petals.TryGetValue(prefab, out stack)) petals[prefab] = (stack = new Stack<PetalDrop>());

		PetalDrop instance;
		if(stack.Count < 1)
		{
			instance = Instantiate(prefab);
			instance.prefab = prefab;
		}
		else
		{
			instance = stack.Pop();
		}
		instance.gameObject.SetActive(true);
		return instance;
	}
}