using System.Collections.Generic;
using UI_Framework.Scripts.Specific_UI;
using UI_Framework.Scripts.Tools;
using UnityEngine;

namespace UI_Framework.Scripts.Test
{
    public class FormTest : MonoBehaviour
    {
        public List<int> id1List;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                UIMgr.Instance.CreateUI<Test1>().Open();
            }
            
            if (Input.GetKeyDown(KeyCode.D))
            {
                UIMgr.Instance.CreateUI<UIQuestionBox>().SetTips("确认要跳转场景吗").YesCallback(() =>
                { SceneChangeHelper.Instance.LoadScene("SceneChangeDemo1"); });
            }
            
            if (Input.GetKeyDown(KeyCode.W))
            {
                UIMgr.Instance.GetFirstUI<Test1>()?.Close();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                UIMgr.Instance.GetFirstUI<Test1>().DestroySelf();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (UIMgr.Instance.HasActiveForm())
                {
                    UIMgr.Instance.HideUIFormTurn();
                }
            }

            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     id1List.Add(UIMgr.Instance.CreateUI<Test1>());
            // }
            //
            // if (Input.GetKeyDown(KeyCode.A))
            // {
            //     UIMgr.Instance.ShowUIForm(id1List[^1]);
            // }
            //
            // if (Input.GetKeyDown(KeyCode.D))
            // {
            //     // 摧毁最近不常用的UI
            //     UIMgr.Instance.DestroyUI(UIMgr.Instance.ShowFormsLRU.First.Value.id);
            // }
        }
    }
}