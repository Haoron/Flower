using UnityEngine;

public class FlowerSounds : MonoBehaviour
{
	[System.Serializable]
	private struct SoundPair
	{
#pragma warning disable CS0649
		public FlowerSound sound;
		public AudioClip clip;
#pragma warning restore CS0649
	}

	[SerializeField]
	private AudioSource source = null;
	[SerializeField]
	private SoundPair[] sounds = null;
	[SerializeField]
	private AudioClip[] petalCreate = null;
	[SerializeField]
	private AudioClip[] petalRemove = null;

	public float Play(FlowerSound sound, float pitchRange = 0f)
	{
		int index = System.Array.FindIndex(sounds, s => s.sound == sound);
		if(index < 0) return 0f;

		return Play(sounds[index].clip);
	}

	public float PlayPetalCreate(int index, float pitchRange = 0f)
	{
		if(index < 0 || index >= petalCreate.Length)
		{
			Debug.LogWarning("Petal create sounds out of range");
			return 0f;
		}
		return Play(petalCreate[index]);
	}

	public float PlayPetalRemove(int index, float pitchRange = 0f)
	{
		if(index < 0 || index >= petalRemove.Length)
		{
			Debug.LogWarning("Petal remove sounds out of range");
			return 0f;
		}
		return Play(petalRemove[index]);
	}

	private float Play(AudioClip clip, float pitchRange = 0f)
	{
		source.pitch = 1f + Random.Range(-pitchRange * 0.5f, pitchRange * 0.5f);
		source.PlayOneShot(clip);
		return clip.length * source.pitch;
	}
}

public enum FlowerSound
{
	FaceTouch,
	FaceDrag,
	FaceDrop,
	PetalTouch,
	PetalDrag,
	PetalDrop
}