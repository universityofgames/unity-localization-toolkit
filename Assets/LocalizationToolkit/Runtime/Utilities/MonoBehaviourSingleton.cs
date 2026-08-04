using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>
	/// Base class for scene-based MonoBehaviour singletons. The instance is cached
	/// in play mode and resolved on demand in edit mode, so derived components can
	/// be used both at runtime and from editor tooling.
	/// </summary>
	/// <typeparam name="T">The concrete singleton component type.</typeparam>
	public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
	{
		private static T _instance;

		/// <summary>Active instance of <typeparamref name="T"/>, or null when none exists in the scene.</summary>
		public static T Instance
		{
			get
			{
				if (_instance != null)
					return _instance;

				T found = FindAnyObjectByType<T>();
				if (Application.isPlaying)
					_instance = found;

				return found;
			}
		}

		protected virtual void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Debug.LogWarning($"[LocalizationToolkit] More than one {typeof(T).Name} instance detected. Only one should exist per scene.", this);
				return;
			}

			_instance = (T)this;
		}

		protected virtual void OnDestroy()
		{
			if (_instance == this)
				_instance = null;
		}
	}
}
