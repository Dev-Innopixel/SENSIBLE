#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using WeArt.Components;


namespace WeArt.UnityEditor
{
    public class WeArtPrefabMenuItem : Editor
    {
        [MenuItem("WEART/Add Weart Startup Components", false, -1)]
        public static void AddWeartVR()
        {
            GameObject wRoot = new GameObject("WEART");
            //GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/WEART Components/Prefabs/WEART_VR.prefab", typeof(GameObject)) as GameObject;
            //PrefabUtility.InstantiatePrefab(prefab);

            GameObject wCtrl = AssetDatabase.LoadAssetAtPath("Packages/com.weart.sdk/Runtime/Prefabs/WeArtController.prefab", typeof(GameObject)) as GameObject;
            PrefabUtility.InstantiatePrefab(wCtrl, wRoot.transform);

            GameObject handL = AssetDatabase.LoadAssetAtPath("Packages/com.weart.sdk/Runtime/Prefabs/WEARTLeftHand.prefab", typeof (GameObject)) as GameObject;
            PrefabUtility.InstantiatePrefab(handL, wRoot.transform);

            GameObject handR = AssetDatabase.LoadAssetAtPath("Packages/com.weart.sdk/Runtime/Prefabs/WEARTRightHand.prefab", typeof(GameObject)) as GameObject;
            PrefabUtility.InstantiatePrefab(handR, wRoot.transform);

            EditorUtility.SetDirty(wCtrl);
            EditorUtility.SetDirty(handL);
            EditorUtility.SetDirty(handR);

            Debug.Log("Weart Startup components created succesfully");             
        }        
    }
}
#endif