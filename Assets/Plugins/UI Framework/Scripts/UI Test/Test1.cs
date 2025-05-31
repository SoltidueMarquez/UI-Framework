using UI_Framework.Scripts.Tools;

namespace UI_Framework.Scripts.Test
{
    public class Test1 : UIFormBase
    {
        public UIList uiList;
        
        protected override void OnInit()
        {
            for (int i = 0; i < 18; i++)
            {
                uiList.CloneItem<Test1Item>().Init(i);
            }
        }
    }
}

