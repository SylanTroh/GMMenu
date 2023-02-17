
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu.Utils
{
    public class CanvasUtils : UdonSharpBehaviour
    {
        public static void CanvasSetActive(Canvas canvas, bool val)
        {
            canvas.enabled = val;
            canvas.GetComponent<Collider>().enabled = val;
        }
        public static void CanvasSetActive(GameObject canvas, bool val)
        {
            canvas.GetComponent<Canvas>().enabled = val;
            canvas.GetComponent<Collider>().enabled = val;
        }
        public static void CanvasSetActive(Transform canvas, bool val)
        {
            canvas.GetComponent<Canvas>().enabled = val;
            canvas.GetComponent<Collider>().enabled = val;
        }
    }
}
