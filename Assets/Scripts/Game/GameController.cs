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

	private int levelIndex;

	void Awake()
	{
		flower.onEnd += OnEndGame;

		levelIndex = 0;
		flower.Init(levels.levels[levelIndex]);
	}

	void OnDestroy()
	{
		flower.onEnd -= OnEndGame;
	}

	private void OnEndGame(bool isHappy)
	{
		if(isHappy) levelIndex++;
		source.PlayOneShot(isHappy ? winClip : loseClip);
		flower.PlayAnimation(isHappy ? "Win" : "Lose", 0.05f, OnEndAnim);
	}

	private void OnEndAnim()
	{
		if(levelIndex < levels.levels.Length)
		{
			flower.Init(levels.levels[levelIndex]);
		}
		else
		{
			//TODO: end game
		}
	}
}