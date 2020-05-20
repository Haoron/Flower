using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetalsController : MonoBehaviour
{
	[SerializeField]
	private float petalsMoveTime = 1f;
	[SerializeField]
	private AnimationCurve petalsShowAnimation = null;

	[SerializeField]
	private FlowerSounds sounds = null;

	[SerializeField, Range(-180f, 180f)]
	private float angle;

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
	private int removeIndex;

	public void SetPetals(FlowerController flower, Levels.FlowerConfiguration config)
	{
		var petals = config.petals;
		this.angle = config.angle;
		this.count = petals.Length;

		int len = Mathf.Max(config.slots, petals.Length);
		slots = new Quaternion[len];
		float angle = 360f / len;
		for(int i = 0; i < len; i++)
		{
			slots[i] = Quaternion.Euler(0f, angle * i + config.angle, 0f);
		}

		var list = new List<KeyValuePair<int, int>>(petals.Length);
		activePetals = new FlowerPetal[len];
		int half = petals.Length / 2;
		for(int j = 0, i; j < petals.Length; j++)
		{
			i = j > half ? (len - (petals.Length - j)) : j;
			activePetals[i] = allPetals[i];
			activePetals[i].flower = flower;
			activePetals[i].index = i;
			list.Add(new KeyValuePair<int, int>(i, petals[j].showIndex));

			activePetals[i].Init(slots[i], petals[j].color, i == 0 ? 0 : (i > half ? -1 : 1));

			activePetals[i].petalAnchor.localScale = Vector3.one * 0.01f;
			activePetals[i].gameObject.SetActive(true);
		}
		isAnimate = true;
		removeIndex = 0;
		list.Sort(PetalsQueueComparer);
		StartCoroutine(ShowPetalsRoutine(list.ConvertAll(x => x.Key)));
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

	public float RemovePetal(int index)
	{
		int side = activePetals[index].side;
		activePetals[index].gameObject.SetActive(false);
		activePetals[index] = null;
		count--;
		if(count <= 0) return 0f;

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

		float time = sounds.PlayPetalRemove(removeIndex, 0.1f);
		removeIndex++;
		return time;
	}

	private IEnumerator ShowPetalsRoutine(List<int> list)
	{
		float animTime = petalsShowAnimation.keys[petalsShowAnimation.length - 1].time;
		float time;
		for(int i = 0; i < list.Count; i++)
		{
			sounds.PlayPetalCreate(i, 0.1f);

			int index = list[i];
			time = 0f;
			while(time < animTime)
			{
				time += Time.deltaTime;
				activePetals[index].petalAnchor.localScale = Vector3.one * Mathf.Max(petalsShowAnimation.Evaluate(Mathf.Min(time, animTime)), 0.01f);
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

#if UNITY_EDITOR
	void OnValidate()
	{
		if(Application.isPlaying && slots != null && activePetals != null)
		{
			float angle = 360f / slots.Length;
			for(int i = 0; i < slots.Length; i++)
			{
				slots[i] = Quaternion.Euler(0f, angle * i + this.angle, 0f);
			}
			for(int i = 0; i < activePetals.Length; i++)
			{
				activePetals[i].targetRotation = slots[activePetals[i].index];
				activePetals[i].petalAnchor.localRotation = activePetals[i].targetRotation;
			}
		}
	}
#endif
}