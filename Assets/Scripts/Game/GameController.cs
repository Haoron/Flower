using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField]
	private FlowerController flower = null;
	[SerializeField]
	private Levels levels = null;

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
		//TODO: show result
		if(isHappy) levelIndex++;
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