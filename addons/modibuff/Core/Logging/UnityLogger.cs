#if UNITY_5_3_OR_NEWER //UNITY_2_6?
using UnityEngine;

namespace ModiBuff.Core
{
	public class UnityLogger : ILogger
	{
		public void Log(string message) => Debug.Log(message);

		public void LogWarning(string message) => Debug.LogWarning(message);

		public void LogError(string message) => Debug.LogError(message);
	}
}
#endif