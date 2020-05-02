using UnityEngine;

public class FlowerController : MonoBehaviour
{
	[SerializeField]
	private FlowerFace flowerFace = null;
	[SerializeField]
	private Transform flowerAnchor = null;
	[SerializeField]
	private Transform areaCenter = null;
	[SerializeField]
	private float areaRadius = 2f;

	private FlowerState state = FlowerState.None;
	private FlowerLeaf[] leafs;

	private bool animEnabled = true;

	void Awake()
	{
		leafs = GetComponentsInChildren<FlowerLeaf>();
		for(int i = 0; i < leafs.Length; i++)
		{
			leafs[i].flower = this;
			leafs[i].leafIndex = i;
		}
		flowerFace.flower = this;
	}

	void LateUpdate()
	{
		if(animEnabled)
		{
			flowerAnchor.position = Vector3.MoveTowards(flowerAnchor.position, areaCenter.position, Time.deltaTime * 32f);
		}
	}

	public bool CanInteract() { return state == FlowerState.None; }

	public void SetState(FlowerState state) { this.state = state; }

	public void SetAnimation(bool enabled)
	{
		animEnabled = enabled;
	}

	public void MoveFace(Vector3 pos)
	{
		pos.z = flowerAnchor.position.z;
		flowerAnchor.position = pos;
	}

	public bool MoveLeaf(Vector3 pos)
	{
		pos.z = flowerAnchor.position.z;
		float dist = Vector3.Distance(areaCenter.position, pos);
		if(dist > areaRadius)
		{
			flowerAnchor.position = areaCenter.position + (pos - areaCenter.position) * (areaRadius / dist);
			return false;
		}
		flowerAnchor.position = pos;
		return true;
	}

	public void RemoveLeaf(int index)
	{
		leafs[index].gameObject.SetActive(false);
		flowerAnchor.position = areaCenter.position;
		SetAnimation(true);
		SetState(FlowerState.None);
	}
}