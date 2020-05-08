﻿using UnityEngine;

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
	private SoundPair[] sounds = null;
	[SerializeField]
	private AudioSource source = null;

	public float Play(FlowerSound sound, float pitchRange = 0f)
	{
		int index = System.Array.FindIndex(sounds, s => s.sound == sound);
		if(index < 0) return 0f;

		var clip = sounds[index].clip;
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
	PetalDrop,
	PetalRemove,
	PetalCreate
}