using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedMono : MonoBehaviour
{
    private readonly Dictionary<object, Coroutine> _runningRoutines = new();

    public Coroutine StartSafeCoroutine(object routineKey, IEnumerator routine)
    {
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("You can't start safe coroutine active false object.");
            return null;
        }
            
        StopSafeCoroutine(routineKey);

        Coroutine newCoroutine = StartCoroutine(routine);
        _runningRoutines[routineKey] = newCoroutine;
        return _runningRoutines[routineKey];
    }

    public void StopSafeCoroutine(object routineKey)
    {
        if (_runningRoutines.TryGetValue(routineKey, out Coroutine coroutine))
        {
            if (coroutine != null)
                StopCoroutine(coroutine);

            _runningRoutines.Remove(routineKey);
        }
    }

    public void StopAllSafeCoroutines()
    {
        foreach (var coroutine in _runningRoutines.Values)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }

        _runningRoutines.Clear();
    }
}