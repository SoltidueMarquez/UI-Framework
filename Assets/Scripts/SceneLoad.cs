using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class SceneLoad : MonoBehaviour
    {
        public string name;

        public void ChangeScene()
        {
            SceneManager.LoadScene(name);
        }
    }
}