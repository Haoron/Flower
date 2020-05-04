using UnityEngine;

[CreateAssetMenu(menuName = "Levels", fileName = "Levels.asset")]
public class Levels : ScriptableObject
{
	public FlowerConfiguration[] levels;

	[System.Serializable]
	public struct FlowerConfiguration
	{
		public PetalInfo[] petals;
	}

	[System.Serializable]
	public struct PetalInfo
	{
		public Color color;
		public int showIndex;
	}
}