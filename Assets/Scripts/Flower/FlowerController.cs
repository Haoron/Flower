using UnityEngine;

public class FlowerController : MonoBehaviour
{
	[SerializeField]
	private Animator animator = null;
	[SerializeField]
	private FlowerFace flowerFace = null;
	[SerializeField]
	private Transform flowerAnchor = null;
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

	private FlowerState state = FlowerState.None;
	private FlowerLeaf[] leafs;

	private bool animEnabled = true;
	private bool flowerFun = true;

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
			flowerAnchor.localPosition = Vector3.Lerp(flowerAnchor.localPosition, Vector3.zero, Time.deltaTime * 4f);
		}
	}

	public bool CanInteract() { return state == FlowerState.None; }

	public void SetState(FlowerState state) { this.state = state; }

	public void SetAnimation(bool enabled)
	{
		animEnabled = enabled;
		animator.enabled = enabled;
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

		flowerFun = !flowerFun;
		SetAnimation(true);
		SetState(FlowerState.None);
	}
}