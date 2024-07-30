using UnityEngine;
using UnityEngine.UI;

namespace Utils{
    class DebugUtils{
        static public void LogButtonSubscribers(Button myButton){
            int scriptCount = myButton.onClick.GetPersistentEventCount();
            for (int i = 0; i < scriptCount; i++)
            {
                UnityEngine.Object target = myButton.onClick.GetPersistentTarget(i);
                string methodName = myButton.onClick.GetPersistentMethodName(i);

                if (target != null)
                {
                    System.Type type = target.GetType();
                    Debug.Log("Script #" + i + ": " + type.FullName + " - " + methodName);
                }
                else
                {
                    Debug.Log("Script #" + i + ": No target object found - " + methodName);
                }
            }
        }
    }
}