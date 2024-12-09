using Common.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Common;

internal static class ObjectAndComponentExtensions
{
    public static void CallAfterDelay(this GameObject go, float delay, UnityAction action)
    {
        go.AddComponent<CallAfterDelay>().Initialize(delay, action);
    }

    public static C EnsureComponent<C>(this GameObject go) where C : Component
    {
        return go.EnsureComponent(typeof(C)) as C;
    }

    public static Component EnsureComponent(this GameObject go, Type type)
    {
        return go.GetComponent(type) ?? go.AddComponent(type);
    }

    public static GameObject GetParent(this GameObject go)
    {
        return go.transform.parent?.gameObject;
    }

    public static GameObject GetChild(this GameObject go, string name)
    {
        return go.transform.Find(name)?.gameObject;
    }

    public static void SetTransform(this GameObject go, Vector3? pos = null, Vector3? localPos = null, Vector3? localAngles = null, Vector3? localScale = null)
    {
        Transform tr = go.transform;

        if (pos != null)
        {
            tr.position = (Vector3)pos;
        }

        if (localPos != null)
        {
            tr.localPosition = (Vector3)localPos;
        }

        if (localAngles != null)
        {
            tr.localEulerAngles = (Vector3)localAngles;
        }

        if (localScale != null)
        {
            tr.localScale = (Vector3)localScale;
        }
    }

    public static void SetParent(this GameObject go, GameObject parent, Vector3? localPos = null, Vector3? localAngles = null)
    {
        go.transform.SetParent(parent.transform, false);
        go.SetTransform(localPos: localPos, localAngles: localAngles);
    }

    public static GameObject CreateChild(this GameObject go, string name, Vector3? localPos = null)
    {
        GameObject child = new(name);
        child.SetParent(go);

        child.SetTransform(localPos: localPos);

        return child;
    }

    public static GameObject CreateChild(this GameObject go, GameObject prefab, string name = null,
                                         Vector3? localPos = null, Vector3? localAngles = null, Vector3? localScale = null)
    {
        GameObject child = Object.Instantiate(prefab, go.transform);

        if (name != null)
        {
            child.name = name;
        }

        child.SetTransform(localPos: localPos, localAngles: localAngles, localScale: localScale);

        return child;
    }

    // for use with inactive game objects
    public static C GetComponentInParent<C>(this GameObject go) where C : Component
    {
        return _get<C>(go);

        static _C _get<_C>(GameObject _go) where _C : Component
        {
            return !_go ? null : (_go.GetComponent<_C>() ?? _get<_C>(_go.GetParent()));
        }
    }

    private static void Destroy(this Object obj, bool immediate)
    {
        if (immediate)
        {
            Object.DestroyImmediate(obj);
        }
        else
        {
            Object.Destroy(obj);
        }
    }

    public static void DestroyChild(this GameObject go, string name, bool immediate = true)
    {
        go.GetChild(name)?.Destroy(immediate);
    }

    public static void DestroyChildren(this GameObject go, params string[] children)
    {
        children.ForEach(name => go.DestroyChild(name, true));
    }

    public static void DestroyComponent(this GameObject go, Type componentType, bool immediate = true)
    {
        go.GetComponent(componentType)?.Destroy(immediate);
    }

    public static void DestroyComponent<C>(this GameObject go, bool immediate = true) where C : Component
    {
        DestroyComponent(go, typeof(C), immediate);
    }

    public static void DestroyComponentInChildren<C>(this GameObject go, bool immediate = true) where C : Component
    {
        go.GetComponentInChildren<C>()?.Destroy(immediate);
    }


    // if fields is empty we try to copy all fields
    public static void CopyFieldsFrom<CT, CF>(this CT cmpTo, CF cmpFrom, params string[] fieldNames) where CT : Component where CF : Component
    {
        try
        {
            Type typeTo = cmpTo.GetType(), typeFrom = cmpFrom.GetType();

            foreach (var fieldTo in fieldNames.Length == 0 ? typeTo.Fields() : fieldNames.Select(name => typeTo.Field(name)))
            {
                if (typeFrom.Field(fieldTo.Name) is FieldInfo fieldFrom)
                {
                    $"copyFieldsFrom: copying field {fieldTo.Name} from {cmpFrom} to {cmpTo}".LogDebug();
                    fieldTo.SetValue(cmpTo, fieldFrom.GetValue(cmpFrom));
                }
            }
        }
        catch (Exception e) { Log.Message(e); }
    }
}

internal static class StructsExtension
{
    public static Vector2 SetX(this Vector2 vec, float val) { vec.x = val; return vec; }
    public static Vector2 SetY(this Vector2 vec, float val) { vec.y = val; return vec; }

    public static Vector3 SetX(this Vector3 vec, float val) { vec.x = val; return vec; }
    public static Vector3 SetY(this Vector3 vec, float val) { vec.y = val; return vec; }
    public static Vector3 SetZ(this Vector3 vec, float val) { vec.z = val; return vec; }

    public static Color SetA(this Color color, float val) { color.a = val; return color; }
}

internal static class UnityHelper
{
    public static GameObject CreatePersistentGameObject(string name)
    {
        $"UnityHelper.createPersistentGameObject: creating '{name}'".LogDebug();
        GameObject obj = new(name, typeof(SceneCleanerPreserve));
        Object.DontDestroyOnLoad(obj);
        return obj;
    }

    public static GameObject CreatePersistentGameObject<C>(string name) where C : Component
    {
        GameObject obj = CreatePersistentGameObject(name);
        _ = obj.AddComponent<C>();
        return obj;
    }

    private class CoroutineHost : MonoBehaviour
    {
        public static CoroutineHost main;
    }

    public static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        CoroutineHost.main ??= CreatePersistentGameObject("CoroutineHost").AddComponent<CoroutineHost>();
        return CoroutineHost.main.StartCoroutine(coroutine);
    }

    // includes inactive objects
    // for use in non-performance critical code
    public static List<T> FindObjectsOfTypeAll<T>() where T : Behaviour
    {
        List<T> list = Enumerable.Range(0, SceneManager.sceneCount).
            Select(SceneManager.GetSceneAt).
            Where(s => s.isLoaded).
            SelectMany(s => s.GetRootGameObjects()).
            SelectMany(go => go.GetComponentsInChildren<T>(true)).
            ToList();

        $"FindObjectsOfTypeAll({typeof(T)}) result => all: {list.Count}, active: {list.Where(c => c.isActiveAndEnabled).Count()}".LogDebug();
        return list;
    }

    // using reflection to avoid including UnityEngine.UI in all projects
    private static readonly Type eventSystem = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
    private static readonly PropertyWrapper currentEventSystem = eventSystem.Property("current").Wrap();
    private static readonly MethodWrapper setSelectedGameObject = eventSystem.Method("SetSelectedGameObject", typeof(GameObject)).Wrap();

    // unselects currently selected object (needed for buttons)
    public static void ClearSelectedUIObject()
    {
        setSelectedGameObject.Invoke(currentEventSystem.Get(), null);
    }

    // for use in non-performance critical code
    public static C FindNearest<C>(Vector3? pos, out float distance, Predicate<C> condition = null) where C : Component
    {
        using Debug.Profiler _ = Debug.GetProfiler($"UnityHelper.findNearest({typeof(C).Name})");

        distance = float.MaxValue;

        if (pos == null)
        {
            return null;
        }

        C result = null;
        Vector3 validPos = (Vector3)pos;

        foreach (C c in Object.FindObjectsOfType<C>())
        {
            if (condition != null && !condition(c))
            {
                continue;
            }

            float distSq = (c.transform.position - validPos).sqrMagnitude;

            if (distSq < distance)
            {
                distance = distSq;
                result = c;
            }
        }

        if (distance < float.MaxValue)
        {
            distance = Mathf.Sqrt(distance);
        }

        return result;
    }
}

internal static class InputHelper
{
    public static int GetMouseWheelDir()
    {
        return Math.Sign(GetMouseWheelValue());
    }

    public static float GetMouseWheelValue()
    {
        return getAxis 
            ? getAxis.Invoke("Mouse ScrollWheel") 
            : 0f;
    }

    private static readonly MethodWrapper<Func<string, float>> getAxis =
        Type.GetType("UnityEngine.Input, UnityEngine.InputLegacyModule")?.Method("GetAxis")?.Wrap<Func<string, float>>();
}

internal class CallAfterDelay : MonoBehaviour
{
    private float delay;
    private UnityAction action;

    public void Initialize(float delay, UnityAction action)
    {
        this.delay = delay;
        this.action = action;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);

        action();
        Destroy(this);
    }
}