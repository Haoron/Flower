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

	public System.Action<bool> onEnd;

	public void Init(Levels.FlowerConfiguration config)
	{
		flowerFace.flower = this;
		flowerState.Init(true);
		flowerPetals.SetPetals(this, config.petals);
	}

	public bool CanInteract() { return !flowerPetals.isAnimate && flowerState.state == FlowerState.None && flowerState.isIdle; }

	public void SetState(FlowerState state)
	{
		flowerState.SetState(state);
	}

	public void SetAnimation(bool enabled)
	{
		flowerState.SetAnimation(enabled);
	}

	public void MoveFace(Vector3 pos)
	{
		pos.z = flowerState.anchor.position.z;
		flowerState.SetPosition(pos);
	}

	public bool MovePetal(Vector3 pos)
	{
		pos.z = flowerState.anchor.position.z;
		float dist = Vector3.Distance(areaCenter.position, pos);
		if(dist > areaRadius)
		{
			if(flowerState.SetPosition(areaCenter.position + (pos - areaCenter.position) * (areaRadius / dist)))
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

		flowerPetals.RemovePetal(index);
		if(flowerPetals.count <= 0)
		{
			if(onEnd != null) onEnd.Invoke(flowerState.isHappy);
		}
	}
}