using UnityEditor;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>
	/// One-time welcome window that gets new users from import to a working setup in
	/// a few clicks. Reopen it anytime via <c>Tools → Localization Toolkit → Welcome</c>.
	/// </summary>
	public class WelcomeWindow : EditorWindow
	{
		private const string ShownPrefsPrefix = "UniversityOfGames.LocalizationToolkit.WelcomeShown.";
		private const string DemoScenePath = "Assets/LocalizationToolkit/Samples/Demo.unity";
		private const string SampleLanguageFilePath = "Assets/LocalizationToolkit/Samples/LanguageFiles/lang.json";
		private const string UserGuidePath = "Assets/LocalizationToolkit/Documentation/UserGuide.md";

		[InitializeOnLoadMethod]
		private static void ShowOnFirstImport()
		{
			if (Application.isBatchMode)
				return;

			EditorApplication.delayCall += () =>
			{
				string key = ShownPrefsPrefix + PlayerSettings.productGUID;
				if (EditorPrefs.GetBool(key))
					return;

				EditorPrefs.SetBool(key, true);
				Open();
			};
		}

		[MenuItem("Tools/Localization Toolkit/Welcome")]
		private static void Open()
		{
			var window = GetWindow<WelcomeWindow>(true, "Localization Toolkit");
			window.minSize = window.maxSize = new Vector2(420f, 400f);
			window.Show();
		}

		private void OnGUI()
		{
			GUILayout.Space(14);
			GUILayout.Label("Localization Toolkit", new GUIStyle(EditorStyles.boldLabel)
			{
				fontSize = 20,
				alignment = TextAnchor.MiddleCenter
			});
			GUILayout.Label("Version " + LocalizationToolkitInfo.Version, new GUIStyle(EditorStyles.miniLabel)
			{
				alignment = TextAnchor.MiddleCenter
			});

			GUILayout.Space(8);
			EditorGUILayout.HelpBox(
				"Localize your game in minutes: drop a language file on the Localization Manager, " +
				"add Localized Text components and let the AI translate the rest.",
				MessageType.None);
			GUILayout.Space(8);

			if (DrawBigButton("Create Localization Manager", "Adds a configured manager to the open scene"))
				CreateManager();

			if (DrawBigButton("Open Demo Scene", "A ready-to-play example with six languages"))
				OpenDemoScene();

			if (DrawBigButton("Open Localization Editor", "Edit, audit and AI-translate your data"))
				EditorApplication.ExecuteMenuItem("Tools/Localization Toolkit/Localization Editor");

			if (DrawBigButton("Open User Guide", "The complete manual shipped with the package"))
				EditorUtility.OpenWithDefaultApp(UserGuidePath);

			if (DrawBigButton("Online Documentation", LocalizationToolkitInfo.DocumentationUrl))
				Application.OpenURL(LocalizationToolkitInfo.DocumentationUrl);

			GUILayout.FlexibleSpace();
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (EditorGUILayout.LinkButton(LocalizationToolkitInfo.SupportUrl))
					Application.OpenURL(LocalizationToolkitInfo.SupportUrl);
				GUILayout.FlexibleSpace();
			}
			GUILayout.Space(8);
		}

		private static bool DrawBigButton(string label, string tooltip)
		{
			bool clicked = GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Height(32f));
			GUILayout.Space(4);
			return clicked;
		}

		private static void CreateManager()
		{
			LocalizationManager existing = Object.FindAnyObjectByType<LocalizationManager>();
			if (existing != null)
			{
				EditorGUIUtility.PingObject(existing);
				Selection.activeGameObject = existing.gameObject;
				EditorUtility.DisplayDialog("Localization Toolkit",
					"The open scene already contains a Localization Manager — it has been selected.", "OK");
				return;
			}

			var gameObject = new GameObject("Localization Manager");
			var manager = gameObject.AddComponent<LocalizationManager>();

			var sampleFile = AssetDatabase.LoadAssetAtPath<TextAsset>(SampleLanguageFilePath);
			if (sampleFile != null)
			{
				var serialized = new SerializedObject(manager);
				serialized.FindProperty("_localizationFile").objectReferenceValue = sampleFile;
				serialized.ApplyModifiedPropertiesWithoutUndo();
			}

			Undo.RegisterCreatedObjectUndo(gameObject, "Create Localization Manager");
			Selection.activeGameObject = gameObject;
		}

		private static void OpenDemoScene()
		{
			if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				UnityEditor.SceneManagement.EditorSceneManager.OpenScene(DemoScenePath);
		}
	}
}
