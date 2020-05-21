using Facebook.Unity;
using GameAnalyticsSDK;
using UnityEngine;

public class Analytics : MonoBehaviour
{
	void Awake()
	{
		GameAnalytics.Initialize();
		FB.Init(FBInitCallback);
	}

	private void FBInitCallback()
	{
		if(FB.IsInitialized)
		{
			FB.ActivateApp();
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
}