using System;
using UnityEngine;

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

	public System.Action<bool, int> onGameEnd;
	public System.Action<bool> onStateChange;
	public System.Action<int> onLevelUpdate;
	public System.Action onPetalsShown;

	private int levelIndex;
	private bool hasPlayedAnim = false;

	void Awake()
	{
		flower.onEnd += OnEndGame;
		flower.onStateChange += OnStateChange;
		flower.onPetalsShown += OnPetalsShown;

		levelIndex = 0;
		StartLevel();
	}

	void OnDestroy()
	{
		flower.onEnd -= OnEndGame;
		flower.onStateChange -= OnStateChange;
		flower.onPetalsShown -= OnPetalsShown;
	}

	private void OnPetalsShown()
	{
		if(onPetalsShown != null) onPetalsShown.Invoke();
		//TODO: show ad
	}

	private void OnStateChange(bool isHappy)
	{
		if(onStateChange != null) onStateChange.Invoke(isHappy);
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
		Analytics.LevelInfo(isHappy ? Analytics.LevelState.Complete : Analytics.LevelState.Fail, levelIndex);

		if(isHappy)
		{
			levelIndex++;
			if(levelIndex >= levels.levels.Length)
			{
				levelIndex = UnityEngine.Random.Range(0, levels.levels.Length);
			}
		}
		
		hasPlayedAnim = false;
		source.PlayOneShot(isHappy ? winClip : loseClip);
		flower.PlayAnimation(isHappy ? "Win" : "Lose", 0.05f, OnEndFlowerAnim);
		if(onGameEnd != null) onGameEnd.Invoke(isHappy, levelIndex);
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
		flower.Init(levels.levels[levelIndex]);
		Analytics.LevelInfo(Analytics.LevelState.Start, levelIndex);
		if(onLevelUpdate != null) onLevelUpdate.Invoke(levelIndex);
	}
}