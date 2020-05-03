using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerState : MonoBehaviour
{
	[SerializeField]
	private Animator animator = null;
	[SerializeField]
	private Transform _anchor = null;
	[SerializeField]
	private float restoreSpeed = 4f;

	public Transform anchor { get { return _anchor; } }
	public FlowerFaceState state { get; private set; }
	public bool isHappy { get; private set; }

	public bool inPlace { get { return Mathf.Approximately(_anchor.localPosition.sqrMagnitude, 0f); } }
	public bool isIdle { get { return animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"); } }

	private bool animEnabled = true;

	void LateUpdate()
	{
		if(animEnabled)
		{
			float mag = Vector3.SqrMagnitude(_anchor.localPosition);
			float delta = Time.deltaTime * restoreSpeed;
			if(mag < delta * delta) _anchor.localPosition = Vector3.zero;
			else _anchor.localPosition = Vector3.Lerp(_anchor.localPosition, Vector3.zero, delta);
		}
	}

	public void Init(bool isHappy)
	{
		state = FlowerFaceState.None;
		animEnabled = true;
		this.isHappy = isHappy;
		_anchor.localPosition = Vector3.zero;
	}

	public void Toggle() { isHappy = !isHappy; }

	public void SetState(FlowerFaceState state)
	{
		animator.SetBool("IsHappy", isHappy);
		if(state == FlowerFaceState.None)
		{
			animator.SetBool(this.state.ToString(), false);
		}
		else
		{
			animator.SetBool(state.ToString(), true);
		}
		this.state = state;
	}

	public void SetAnimation(bool enabled)
	{
		animEnabled = enabled;
		animator.enabled = enabled;
	}

	public bool SetPosition(Vector3 pos)
	{
		if(animEnabled) return false;
		_anchor.position = pos;
		return true;
	}
}