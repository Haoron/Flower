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
	private float _leafDragStart = 0.9f;
	public float leafDragStart { get { return _leafDragStart; } }

	[SerializeField]
	private float _leafDragCenterOffset = 0.1f;
	public float leafDragCenterOffset { get { return _leafDragCenterOffset; } }

	private FlowerLeaf[] leafs;

	void Awake()
	{
		leafs = GetComponentsInChildren<FlowerLeaf>();
		for(int i = 0; i < leafs.Length; i++)
		{
			leafs[i].flower = this;
			leafs[i].leafIndex = i;
		}
		flowerFace.flower = this;
		flowerState.Init(true);
	}

	public bool CanInteract() { return flowerState.state == FlowerFaceState.None && flowerState.inPlace && flowerState.isIdle; }

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

	public bool MoveLeaf(Vector3 pos)
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

	public void RemoveLeaf(int index)
	{
		leafs[index].gameObject.SetActive(false);

		flowerState.Toggle();
		SetAnimation(true);
		SetState(FlowerFaceState.None);
	}
}