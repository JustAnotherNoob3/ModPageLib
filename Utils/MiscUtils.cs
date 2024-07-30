using UnityEngine;
using System.Collections;

namespace Utils{
    class MiscUtils{
        public static bool IsOffBounds(GameObject uiObject, float xMargin, float yMargin){
        Vector3 uiObjectPosition = uiObject.transform.position;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(uiObjectPosition);
        return screenPosition.x < 0 - Screen.width/100 * xMargin|| screenPosition.x > Screen.width + Screen.width/100 * xMargin|| screenPosition.y < 0 - Screen.height/100 * yMargin || screenPosition.y > Screen.height + Screen.height/100 * yMargin;
    }
    public static string GetGameObjectPath(Transform obj)
    {
     string path = obj.name;
     while (obj.parent != null)
     {
         obj = obj.parent;
         path = obj.name + "/" + path;
     }
     return path;
    }

    }
}