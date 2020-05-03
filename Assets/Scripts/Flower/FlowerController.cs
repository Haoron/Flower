using UnityEngine;

public class FlowerController : MonoBehaviour
{
	[SerializeField]
	private FlowerState flowerState = null;
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
	private FlowerPetal[] petals = null;

	private FlowerPetal merge = null;
	private Quaternion[] petalSlots;
	private int petalsCount;

	void Awake()
	{
		petalSlots = new Quaternion[petals.Length];
		for(int i = 0; i < petals.Length; i++)
		{
			petals[i].flower = this;
			petals[i].index = i;
			petalSlots[i] = petals[i].petalAnchor.localRotation;
		}
		flowerFace.flower = this;
		flowerState.Init(true);

		int count = petals.Length / 2;
		for(int i = -count; i < (petals.Length - count); i++)
		{
			petals[(i + petals.Length) % petals.Length].side = Mathf.Clamp(i, -1, 1);
		}
		petalsCount = petals.Length;
	}

	public bool CanInteract() { return merge == null && flowerState.state == FlowerFaceState.None && flowerState.inPlace && flowerState.isIdle; }

	public void SetState(FlowerFaceState state)
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
		int side = petals[index].side;
		petals[index].gameObject.SetActive(false);

		flowerState.Toggle();
		SetAnimation(true);
		SetState(FlowerFaceState.None);

		petals[index] = null;
		petalsCount--;
		if(petalsCount <= 0)
		{
			//TODO: game end
		}
		else
		{
			ShiftPetals(index, side);
			int pos = (index + 1) % petals.Length;
			if(side <= 0 && petals[pos] && petals[index] && petals[pos].color == petals[index].color)
			{
				petals[index == 0 ? 0 : pos].gameObject.SetActive(false);
				if(index == 0) ShiftPetals(0, 1);
				else ShiftPetals(pos, -1);
			}
			else
			{
				int neg = (index - 1 + petals.Length) % petals.Length;
				if(side >= 0 && petals[neg] && petals[index] && petals[neg].color == petals[index].color)
				{
					petals[index == 0 ? 0 : neg].gameObject.SetActive(false);
					if(index == 0) ShiftPetals(0, -1);
					else ShiftPetals(neg, 1);
				}
			}
		}
	}

	private void ShiftPetals(int toIndex, int side)
	{
		if(side == 0) side = petals[1] ? 1 : -1;
		for(int i = toIndex;; i = (i + side + petals.Length) % petals.Length)
		{
			int k = (i + side + petals.Length) % petals.Length;
			if(petals[k] == null || petals[k].side != side) break;
			petals[i] = petals[k];
			petals[k] = null;

			petals[i].index = i;
			petals[i].side = i == 0 ? 0 : side;
			petals[i].MoveTo(petalSlots[i]);
		}
	}
}