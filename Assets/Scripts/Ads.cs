using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class Ads : MonoBehaviour
{
	[SerializeField]
	private string gameID_IOS = "";
	[SerializeField]
	private string gameID_Android = "";
	[SerializeField]
	private bool testMode = true;

	[SerializeField]
	private GameController game = null;

	private int winCount = 0;
	private int loseCount = 0;

	void Awake()
	{
#if UNITY_IOS
		Advertisement.Initialize(gameID_IOS, testMode);
#elif UNITY_ANDROID
		Advertisement.Initialize(gameID_Android, testMode);
#endif
		game.onGameEnd += OnGameEnd;
		game.onPetalsShown += OnPetalsShown;
	}

	void OnDestroy()
	{
		game.onGameEnd -= OnGameEnd;
		game.onPetalsShown -= OnPetalsShown;
	}

	private void OnGameEnd(bool isWin, int index)
	{
		if(isWin) winCount++;
		else loseCount++;
	}

	private void OnPetalsShown()
	{
		if(winCount >= 2 || loseCount >= 2) ShowAds();
	}

	private void ShowAds()
	{
		winCount = 0;
		loseCount = 0;
		Advertisement.Show();
	}
}