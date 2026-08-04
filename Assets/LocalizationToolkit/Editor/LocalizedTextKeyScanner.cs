using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>Collects the translation keys referenced by <see cref="LocalizedText"/> components.</summary>
	public static class LocalizedTextKeyScanner
	{
		/// <summary>Collects keys from every LocalizedText in the currently loaded scenes.</summary>
		/// <returns>Distinct, trimmed keys.</returns>
		public static HashSet<string> CollectFromLoadedScenes()
		{
			var keys = new HashSet<string>();
			foreach (LocalizedText text in Object.FindObjectsByType<LocalizedText>(FindObjectsInactive.Include))
				AddKey(keys, text);

			return keys;
		}

		/// <summary>Collects keys from every prefab asset in the project.</summary>
		/// <returns>Distinct, trimmed keys.</returns>
		public static HashSet<string> CollectFromPrefabs()
		{
			var keys = new HashSet<string>();
			foreach (string guid in AssetDatabase.FindAssets("t:Prefab"))
			{
				var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
				if (prefab == null)
					continue;

				foreach (LocalizedText text in prefab.GetComponentsInChildren<LocalizedText>(true))
					AddKey(keys, text);
			}

			return keys;
		}

		/// <summary>
		/// Collects keys from every enabled scene in Build Settings by opening them one
		/// by one, then restores the previous scene setup.
		/// </summary>
		/// <returns>Distinct keys, or null when the user cancelled saving modified scenes.</returns>
		public static HashSet<string> CollectFromBuildScenes()
		{
			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				return null;

			SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
			var keys = new HashSet<string>();

			try
			{
				foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
				{
					if (!buildScene.enabled || string.IsNullOrEmpty(buildScene.path))
						continue;

					EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
					keys.UnionWith(CollectFromLoadedScenes());
				}
			}
			finally
			{
				if (previousSetup != null && previousSetup.Length > 0)
					EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
			}

			return keys;
		}

		private static void AddKey(HashSet<string> keys, LocalizedText text)
		{
			string key = text.Key?.Trim();
			if (!string.IsNullOrEmpty(key))
				keys.Add(key);
		}
	}
}
