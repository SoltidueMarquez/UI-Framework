using UnityEngine;
using UnityEngine.UI;

namespace UI_Framework.Scripts.Test
{
    public class Test1Item : MonoBehaviour
    {
        public Text text;
        public void Init(int id)
        {
            text.text = $"测试{id}";
        }
    }
}