using UnityEngine;

// A MonoBehaviour-based singleton for use at runtime. Add to a single 'global' scene.
// Note: OnEnable() / OnDisable() should be used to register with any global events
// to properly support domain reloads.
public class MonoBehaviourSingleton<T> : MonoBehaviour
    where T : MonoBehaviourSingleton<T>
{
    // The singleton instance.
    public static T instance => _instance != null ? _instance :
        Application.isPlaying ? _instance = FindObjectOfType<T>() : null;
    static T _instance;

    // Called when the instance is created.
    protected virtual void Awake()
    {
        // Verify there is not more than one instance and assign _instance.
        Debug.Assert( _instance == null || _instance == this,
            "More than one singleton instance instantiated!", this );
        _instance = ( T )this;
    }

    // Clear the instance field when destroyed.
    protected virtual void OnDestroy() => _instance = null;
}