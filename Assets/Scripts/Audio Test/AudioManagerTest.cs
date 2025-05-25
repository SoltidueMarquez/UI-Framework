using Audio_Manager;
using UnityEngine;

public class AudioManagerTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.Instance.PlayMusic("bgm1");
        }
    }
}
