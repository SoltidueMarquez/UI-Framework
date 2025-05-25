using UnityEngine;

namespace UI_Framework.Scripts.Tools
{
    public class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }
 
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
 
            DontDestroyOnLoad(gameObject);
        }
    }
}