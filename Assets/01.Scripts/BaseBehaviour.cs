using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class BaseBehaviour : MonoBehaviour
{
    protected virtual void Awake()
    {
        Initialize();
    }
    protected virtual void Initialize() { }

    protected virtual void OnBindField() { }
    protected virtual void OnButtonField() { }

    protected GameObject FindGameObjectInChildren(string name)
    {
        var objects = GetComponentsInChildren<Transform>(true);
        foreach (var obj in objects)
        {
            if (obj.gameObject.name.Equals(name))
                return obj.gameObject;
        }

        return null;
    }

    protected T FindGameObjectInChildren<T>(string name) where T : Component
    {
        T[] objects = GetComponentsInChildren<T>(true);
        foreach (var obj in objects)
        {
            if (obj.gameObject.name.Equals(name))
                return obj;
        }
        return null;
    }

    protected T[] GetComponentsInGameobject<T>(string name) where T : Component
    {
        GameObject gob = GameObject.Find(name);
        return gob.GetComponentsInChildren<T>(true);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BaseBehaviour), true)]
[CanEditMultipleObjects]
public class BehaviourBaseEditor : Editor
{
    private MethodInfo _bindMethod = (typeof(BaseBehaviour)).GetMethod("OnBindField", BindingFlags.NonPublic | BindingFlags.Instance);
    private MethodInfo _buttonMethod = (typeof(BaseBehaviour)).GetMethod("OnButtonField", BindingFlags.NonPublic | BindingFlags.Instance);

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Active Button"))
        {
            _buttonMethod.Invoke(target, new object[] { });
            EditorUtility.SetDirty(target);
        }

        GUILayout.Space(50);
        if (GUILayout.Button("Bind Objects"))
        {
            _bindMethod.Invoke(target, new object[] { });
            EditorUtility.SetDirty(target);
        }

        GUILayout.Space(20);

        base.OnInspectorGUI();
    }
}


#endif
