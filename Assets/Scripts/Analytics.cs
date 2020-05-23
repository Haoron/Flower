using System.Collections.Generic;
using Facebook.Unity;
using GameAnalyticsSDK;
using UnityEngine;

public class Analytics : MonoBehaviour
{
	private const int MAX_EVENTS_QUEUE = 128;

	private const string LEVEL_START = "Level start";
	private const string LEVEL_COMPLETE = "Level finish";
	private const string LEVEL_FAIL = "Level fail";

	private static Queue<KeyValuePair<string, Dictionary<string, object>>> eventsQueue = new Queue<KeyValuePair<string, Dictionary<string, object>>>();

	void Awake()
	{
		eventsQueue.Clear();
		GameAnalytics.Initialize();
		FB.Init(FBInitCallback);
	}

	private void FBInitCallback()
	{
		if(FB.IsInitialized)
		{
			FB.ActivateApp();

			while(eventsQueue.Count > 0)
			{
				var pair = eventsQueue.Dequeue();
				FB.LogAppEvent(pair.Key, parameters : pair.Value);
			}
		}
	}

	public void OnApplicationPause(bool paused)
	{
		if(!paused)
		{
			if(FB.IsInitialized)
			{
				FB.ActivateApp();
			}
		}
	}

	public static void LevelInfo(LevelState state, int levelIndex)
	{
		GameAnalytics.NewProgressionEvent(state == LevelState.Start ? GAProgressionStatus.Start :
			(state == LevelState.Complete ? GAProgressionStatus.Complete : GAProgressionStatus.Fail), levelIndex.ToString());

		string stateName = state == LevelState.Start ? LEVEL_START : (state == LevelState.Complete ? LEVEL_COMPLETE : LEVEL_FAIL);
		var parameters = new Dictionary<string, object>() { { "index", levelIndex.ToString() } };
		if(FB.IsInitialized)
		{
			FB.LogAppEvent(stateName, parameters : parameters);
		}
		else if(eventsQueue.Count < MAX_EVENTS_QUEUE)
		{
			eventsQueue.Enqueue(new KeyValuePair<string, Dictionary<string, object>>(stateName, parameters));
		}
	}

	public enum LevelState
	{
		Start,
		Complete,
		Fail
	}
}