using System.Collections.Generic;
using UnityEngine;

namespace UI_Framework.Scripts.Test
{
    public class FormTest : MonoBehaviour
    {
        public int id1;
        public int id2;
        
        public List<int> id1List;
        
        private void Start()
        {
            id2 = UIMgr.Instance.CreateUI<Test2>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                id1 = UIMgr.Instance.CreateUI<Test1>();
                UIMgr.Instance.ShowUIForm(id1);
            }
            
            if (Input.GetKeyDown(KeyCode.D))
            {
                UIMgr.Instance.ShowUIForm(id2);
            }
            
            if (Input.GetKeyDown(KeyCode.W))
            {
                UIMgr.Instance.HideUIForm(id1);
            }
            
            if (Input.GetKeyDown(KeyCode.S))
            {
                UIMgr.Instance.HideUIForm(id2);
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                UIMgr.Instance.DestroyUI(id1);
            }

            if (UIMgr.Instance.HasActiveForm())
            {
                if (Input.GetKeyDown(KeyCode.Escape))
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