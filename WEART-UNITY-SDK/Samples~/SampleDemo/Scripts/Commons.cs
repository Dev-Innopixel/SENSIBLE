using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WEART
{

    public static class WDebug
    {
        public static bool _enableLog = true;

        public static void Log(string message)
        {
            if(_enableLog)
                Debug.Log(message);
        }
    }
}
