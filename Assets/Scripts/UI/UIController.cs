using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
	private const string LEVEL_FORMAT = "{0}";

	[SerializeField]
	private GameController game = null;
	[SerializeField]
	private Animator animator = null;

	[SerializeField]
	private TMP_Text oldLevelText = null;
	[SerializeField]
	private TMP_Text levelText = null;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
	[SerializeField]
	private TMP_InputField setLevelText = null;
	[SerializeField]
	private Button setLevelBtn = null;
#endif

	void Awake()
	{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
		setLevelBtn.onClick.AddListener(SetLevel);
#endif
		game.onGameEnd += ShowEndGame;
		game.onStateChange += ShowState;
		game.onLevelUpdate += UpdateLevel;
	}

	void OnDestroy()
	{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
		setLevelBtn.onClick.RemoveListener(SetLevel);
#endif
		game.onGameEnd -= ShowEndGame;
		game.onStateChange -= ShowState;
		game.onLevelUpdate -= UpdateLevel;
	}

#if DEVELOPMENT_BUILD || UNITY_EDITOR
	private void SetLevel()
	{
		game.ReplaceLevel(int.Parse(setLevelText.text));
	}
#endif

	private void ShowState(bool isHappy)
	{
		animator.SetBool("IsHappy", isHappy);
	}

	private void UpdateLevel(int index)
	{
		levelText.text = string.Format(LEVEL_FORMAT, index + 1);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
		setLevelText.text = index.ToString();
#endif
	}

	private void ShowEndGame(bool win, int nextLevelIndex)
	{
		PlayAnimation(win ? "Win" : "Lose", game.UIAnimCallback());
		if(win)
		{
			oldLevelText.text = levelText.text;
			levelText.text = string.Format(LEVEL_FORMAT, nextLevelIndex + 1);

			oldLevelText.gameObject.SetActive(true);
			levelText.gameObject.SetActive(false);
		}
	}

	public void PlayAnimation(string name, System.Action callback)
	{
		StartCoroutine(AnimRoutine(name, callback));
	}

	private IEnumerator AnimRoutine(string name, System.Action callback)
	{
		animator.Play(name);
		AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
		while(!info.IsName(name))
		{
			yield return null;
			info = animator.GetCurrentAnimatorStateInfo(0);
		}
		do
		{
			yield return null;
			info = animator.GetCurrentAnimatorStateInfo(0);
		}
		while(info.IsName(name));
		if(callback != null) callback.Invoke();
	}
}