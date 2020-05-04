using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetalsController : MonoBehaviour
{
	[SerializeField]
	private float petalsMoveTime = 1f;
	[SerializeField]
	private float petalsShowTime = 0.5f;

	public bool isAnimate { get; private set; }
	public int count { get; private set; }

	private FlowerPetal[] _allPetals = null;
	private FlowerPetal[] allPetals
	{
		get
		{
			if(_allPetals == null)
			{
				_allPetals = GetComponentsInChildren<FlowerPetal>(true);
				for(int i = 0; i < _allPetals.Length; i++)
				{
					_allPetals[i].gameObject.SetActive(false);
				}
			}
			return _allPetals;
		}
	}
	private FlowerPetal[] activePetals;
	private Quaternion[] slots;

	private FlowerPetal merge = null;

	public void SetPetals(FlowerController flower, Levels.PetalInfo[] petals)
	{
		this.count = petals.Length;
		activePetals = new FlowerPetal[petals.Length];
		slots = new Quaternion[petals.Length];
		float angle = 360f / petals.Length;
		int half = activePetals.Length / 2;
		for(int i = 0; i < petals.Length; i++)
		{
			activePetals[i] = allPetals[i];
			activePetals[i].flower = flower;
			activePetals[i].index = i;

			slots[i] = Quaternion.Euler(0f, angle * i, 0f);
			activePetals[i].Init(slots[i], petals[i].color, i == 0 ? 0 : (i > half ? -1 : 1));

			activePetals[i].petalAnchor.localScale = Vector3.zero;
			activePetals[i].gameObject.SetActive(true);
		}
		isAnimate = true;
		StartCoroutine(ShowPetalsRoutine(petals));
	}

	public bool ShiftPetals(int toIndex, int side)
	{
		bool isMoved = false;
		if(side == 0) side = activePetals[1] ? 1 : -1;
		for(int i = toIndex;; i = (i + side + activePetals.Length) % activePetals.Length)
		{
			int k = (i + side + activePetals.Length) % activePetals.Length;
			if(activePetals[k] == null || activePetals[k].side != side) break;
			activePetals[i] = activePetals[k];
			activePetals[k] = null;

			activePetals[i].index = i;
			activePetals[i].side = i == 0 ? 0 : side;
			activePetals[i].targetRotation = slots[i];
			isMoved = true;
		}
		return isMoved;
	}

	public void RemovePetal(int index)
	{
		int side = activePetals[index].side;
		activePetals[index].gameObject.SetActive(false);
		activePetals[index] = null;
		count--;
		if(count <= 0) return;

		isAnimate |= ShiftPetals(index, side);
		int pos = (index + 1) % activePetals.Length;
		if(side <= 0 && activePetals[pos] && activePetals[index] && activePetals[pos].color == activePetals[index].color)
		{
			merge = activePetals[index == 0 ? 0 : pos];
			if(index == 0) isAnimate |= ShiftPetals(0, 1);
			else isAnimate |= ShiftPetals(pos, -1);
			count--;
		}
		else
		{
			int neg = (index - 1 + activePetals.Length) % activePetals.Length;
			if(side >= 0 && activePetals[neg] && activePetals[index] && activePetals[neg].color == activePetals[index].color)
			{
				merge = activePetals[index == 0 ? 0 : neg];
				if(index == 0) isAnimate |= ShiftPetals(0, -1);
				else isAnimate |= ShiftPetals(neg, 1);
				count--;
			}
		}
		if(isAnimate) StartCoroutine(MovePetalsRoutine());
	}

	private IEnumerator ShowPetalsRoutine(Levels.PetalInfo[] petals)
	{
		var list = new List<KeyValuePair<int, int>>(petals.Length);
		for(int i = 0; i < petals.Length; i++) list.Add(new KeyValuePair<int, int>(i, petals[i].showIndex));
		list.Sort(PetalsQueueComparer);

		float time;
		for(int i = 0; i < list.Count; i++)
		{
			int index = list[i].Key;
			time = 0f;
			while(time < petalsShowTime)
			{
				time += Time.deltaTime;
				activePetals[index].petalAnchor.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / petalsShowTime);
				yield return null;
			}
		}
		isAnimate = false;
	}

	private int PetalsQueueComparer(KeyValuePair<int, int> a, KeyValuePair<int, int> b)
	{
		return a.Value.CompareTo(b.Value);
	}

	private IEnumerator MovePetalsRoutine()
	{
		float time = 0f;
		while(time < petalsMoveTime)
		{
			time += Time.deltaTime;
			for(int i = 0; i < activePetals.Length; i++)
			{
				if(activePetals[i] == null) continue;
				activePetals[i].petalAnchor.localRotation = Quaternion.Lerp(activePetals[i].petalAnchor.localRotation, activePetals[i].targetRotation, time / petalsMoveTime);
			}
			if(merge != null) merge.petalAnchor.localRotation = Quaternion.Lerp(merge.petalAnchor.localRotation, merge.targetRotation, time / petalsMoveTime);
			yield return null;
		}
		isAnimate = false;
		if(merge != null) merge.gameObject.SetActive(false);
		merge = null;
	}
}