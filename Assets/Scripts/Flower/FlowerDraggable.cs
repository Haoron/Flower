﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class FlowerDraggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[SerializeField]
	protected Transform anchor = null;

	[NonSerialized, HideInInspector]
	public FlowerController flower;

	public Transform petalAnchor { get { return anchor; } }

	protected State state;
	protected Vector3 offset;
	protected PointerEventData eventData;

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		if(flower.CanInteract())
		{
			state = State.Hold;
			offset = WorldPos(eventData) - anchor.position;
			flower.SetAnimation(false);
			OnPick();
			eventData.Use();
		}
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		if(state == State.Hold)
		{
			state = State.None;
			flower.SetAnimation(true);
			OnRelease();
			eventData.Use();
		}
	}

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		if(state == State.Hold)
		{
			state = State.Drag;
			OnStartDrag();
			this.eventData = eventData;
		}
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		if(state == State.Drag)
		{
			OnDrag(WorldPos(eventData));
		}
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		if(state == State.Drag)
		{
			state = State.None;
			flower.SetAnimation(true);
			OnRelease();
			this.eventData = null;
		}
	}

	private Vector3 WorldPos(PointerEventData eventData)
	{
		var ray = eventData.pressEventCamera.ScreenPointToRay(eventData.position);
		return ray.origin + ray.direction * ((-Vector3.Dot(ray.origin, Vector3.back) - anchor.position.z) / Vector3.Dot(ray.direction, Vector3.back));
	}

	protected abstract void OnPick();
	protected abstract void OnRelease();
	protected abstract void OnStartDrag();
	protected abstract void OnDrag(Vector3 newPos);

	protected enum State
	{
		None,
		Hold,
		Drag
	}
}