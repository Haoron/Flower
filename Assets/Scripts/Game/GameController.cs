﻿using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField]
	private FlowerController flower = null;
	[SerializeField]
	private Levels levels = null;

	[SerializeField]
	private AudioSource source = null;
	[SerializeField]
	private AudioClip winClip = null;
	[SerializeField]
	private AudioClip loseClip = null;

	public System.Action<bool> onGameEnd;
	public System.Action<int> onLevelUpdate;

	private int levelIndex;
	private bool hasPlayedAnim = false;

	void Awake()
	{
		flower.onEnd += OnEndGame;

		levelIndex = 0;
		StartLevel();
	}

	void OnDestroy()
	{
		flower.onEnd -= OnEndGame;
	}

	public System.Action UIAnimCallback()
	{
		return OnEndUIAnim;
	}

	public void ReplaceLevel(int index)
	{
		if(flower.CanInteract())
		{
			levelIndex = Mathf.Clamp(index, 0, levels.levels.Length);
			OnEndGame(false);
		}
	}

	private void OnEndGame(bool isHappy)
	{
		if(isHappy) levelIndex++;
		hasPlayedAnim = false;
		source.PlayOneShot(isHappy ? winClip : loseClip);
		flower.PlayAnimation(isHappy ? "Win" : "Lose", 0.05f, OnEndFlowerAnim);
		if(onGameEnd != null) onGameEnd.Invoke(isHappy);
	}

	private void OnEndUIAnim()
	{
		if(hasPlayedAnim) StartLevel();
		hasPlayedAnim = true;
	}

	private void OnEndFlowerAnim()
	{
		if(hasPlayedAnim) StartLevel();
		hasPlayedAnim = true;
	}

	private void StartLevel()
	{
		if(levelIndex >= levels.levels.Length)
		{
			levelIndex = Random.Range(0, levels.levels.Length);
		}
		flower.Init(levels.levels[levelIndex]);
		if(onLevelUpdate != null) onLevelUpdate.Invoke(levelIndex);
	}
}