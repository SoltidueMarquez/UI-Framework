using Battle.UI;
using UnityEngine;

namespace UI_Framework.Scripts.Test
{
    public class SplineTest : MonoBehaviour
    {
        public UISpline spline;
        public Vector3 start;
        public Vector3 end;
        
        private void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                end = Input.mousePosition;
                spline.UpdateUISpline(start, end);
            }
        }
    }
}