using System.Collections;
using UnityEngine;

public class FlowerStateController : MonoBehaviour
{
	[SerializeField]
	private Animator animator = null;
	[SerializeField]
	private Transform _anchor = null;
	[SerializeField]
	private float restoreSpeed = 4f;

	public Transform anchor { get { return _anchor; } }
	public FlowerState state { get; private set; }
	public bool isHappy { get; private set; }

	public bool isIdle { get { return animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"); } }

	private bool animEnabled = true;

	public void Init(bool isHappy)
	{
		state = FlowerState.None;
		animEnabled = true;
		this.isHappy = isHappy;
		_anchor.localPosition = Vector3.zero;
	}

	void LateUpdate()
	{
		if(animEnabled)
		{
			float delta = Time.deltaTime * restoreSpeed;
			_anchor.localPosition = Vector3.Lerp(_anchor.localPosition, Vector3.zero, delta);
		}
	}

	public void Toggle() { isHappy = !isHappy; }

	public void SetState(FlowerState state)
	{
		animator.SetBool("IsHappy", isHappy);
		if(state == FlowerState.None)
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

	public void PlayAnimation(string name, float lerpTime, System.Action callback)
	{
		StartCoroutine(AnimationRoutine(name, lerpTime, callback));
	}

	private IEnumerator AnimationRoutine(string name, float lerpTime, System.Action callback)
	{
		animator.CrossFade(name, lerpTime);
		AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
		while(!info.IsName(name))
		{
			yield return null;
			info = animator.GetCurrentAnimatorStateInfo(0);
		}
		do
		{
			yield return null;
			info = animator.GetCurrentAnimatorStateInfo(0);
		}
		while(info.IsName(name));
		if(callback != null) callback.Invoke();
	}

	public bool SetPosition(Vector3 pos)
	{
		if(animEnabled) return false;
		_anchor.position = pos;
		return true;
	}
}