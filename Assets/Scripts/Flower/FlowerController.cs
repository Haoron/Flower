using System.Collections;
using UnityEngine;

public class FlowerController : MonoBehaviour
{
	[SerializeField]
	private FlowerStateController flowerState = null;
	[SerializeField]
	private PetalsController flowerPetals = null;
	[SerializeField]
	private FlowerFace flowerFace = null;

	[SerializeField]
	private Transform areaCenter = null;
	[SerializeField]
	private float areaRadius = 2f;

	[SerializeField]
	private float _petalDragStart = 0.9f;
	public float petalDragStart { get { return _petalDragStart; } }

	[SerializeField]
	private float _petalDragCenterOffset = 0.1f;
	public float petalDragCenterOffset { get { return _petalDragCenterOffset; } }

	[SerializeField]
	private Transform flowerRoot = null;
	[SerializeField]
	private float flowerMaxDistance = 1f;
	[SerializeField]
	private float flowerFloor = 1f;

	[SerializeField]
	private FlowerSounds sounds = null;

	public System.Action<bool> onEnd;

	public void Init(Levels.FlowerConfiguration config)
	{
		flowerFace.flower = this;
		flowerState.Init(config.isHappy);
		flowerPetals.SetPetals(this, config.petals);
	}

	public bool CanInteract() { return !flowerPetals.isAnimate && flowerState.state == FlowerState.None && flowerState.isIdle; }

	public void SetState(FlowerState state)
	{
		flowerState.SetState(state);
	}

	public void PlaySound(FlowerSound sound)
	{
		sounds.Play(sound, 0.1f);
	}

	public void SetAnimation(bool enabled)
	{
		flowerState.SetAnimation(enabled);
	}

	public void PlayAnimation(string name, float lerpTime, System.Action callback)
	{
		flowerState.PlayAnimation(name, lerpTime, callback);
	}

	public void MoveFace(Vector3 pos)
	{
		pos = ClampPos(pos);
		flowerState.SetPosition(pos);
	}

	public bool MovePetal(Vector3 pos)
	{
		float aDist = Vector3.Distance(areaCenter.position, pos);
		pos = ClampPos(pos);

		if(aDist > areaRadius)
		{
			if(flowerState.SetPosition(areaCenter.position + (pos - areaCenter.position) * (areaRadius / aDist)))
			{
				return false;
			}
		}
		flowerState.SetPosition(pos);
		return true;
	}

	public void RemovePetal(int index)
	{
		flowerState.Toggle();
		SetAnimation(true);
		SetState(FlowerState.None);

		float time = sounds.Play(FlowerSound.PetalRemove, 0.1f);

		flowerPetals.RemovePetal(index);
		if(flowerPetals.count <= 0)
		{
			StartCoroutine(EndGameRoutine(time));
		}
	}

	private Vector3 ClampPos(Vector3 pos)
	{
		float dist = Vector3.Distance(flowerRoot.position, pos);
		if(dist > flowerMaxDistance) pos = flowerRoot.position + (pos - flowerRoot.position).normalized * flowerMaxDistance;
		if(pos.y < flowerRoot.position.y + flowerFloor) pos.y = flowerRoot.position.y + flowerFloor;
		pos.z = flowerState.anchor.position.z;
		return pos;
	}

	private IEnumerator EndGameRoutine(float time)
	{
		yield return new WaitForSeconds(time);
		if(onEnd != null) onEnd.Invoke(flowerState.isHappy);
	}

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if(flowerRoot != null)
		{
			Color color = UnityEditor.Handles.color;
			UnityEditor.Handles.color = Color.yellow;

			float angle = Mathf.Acos(flowerFloor / flowerMaxDistance);
			Vector3 floor = transform.up * flowerFloor;
			Vector3 side = transform.right * (Mathf.Sin(angle) * flowerMaxDistance);
			UnityEditor.Handles.DrawLine(transform.position + floor - side, transform.position + floor + side);

			angle *= Mathf.Rad2Deg;
			UnityEditor.Handles.DrawWireArc(transform.position, -transform.forward, Quaternion.Euler(0f, 0f, angle) * Vector3.up, -angle * 2f, flowerMaxDistance);

			UnityEditor.Handles.color = color;
		}
	}
#endif
}