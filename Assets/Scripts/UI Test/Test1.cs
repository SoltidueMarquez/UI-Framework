using UI_Framework.Scripts.Tools;

namespace UI_Framework.Scripts.Test
{
    public class Test1 : UIFormBase
    {
        public override string PrefabPath => "Prefabs/1";


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

