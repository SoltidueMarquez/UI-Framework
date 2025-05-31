using System;
using System.Collections.Generic;
using UI_Framework.Scripts.Test;

namespace UI_Framework.Scripts
{
    public static class UIFormRegistry
    {
        private static readonly Dictionary<Type, string> pathMap = new()
        {
            { typeof(Test1), "Prefabs/1" },
            { typeof(Test2), "Prefabs/2" },
        };

        public static string GetPath<T>() where T : UIFormBase
        {
            return pathMap[typeof(T)];
        }
    }
}