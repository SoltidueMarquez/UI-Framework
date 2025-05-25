using UnityEngine;

namespace UI_Framework.Scripts
{
    /// <summary>
    /// 懒汉模式单例脚本
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> : MonoBehaviour where T : Component, new()
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
        }
    }
}
