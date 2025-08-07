using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T mInstance;

    public static T Instance
    {
        get
        {
            if (mInstance == null || mInstance.Equals(null))
            {
                mInstance = FindObjectOfType<T>();
                if (mInstance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    mInstance = obj.AddComponent<T>();
                }
            }
            
            return mInstance;
        }
    }
    
    protected virtual void Awake()
    {
        if (mInstance == null)
        {
            mInstance = this as T;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (mInstance != this)     
        {
            Destroy(this);            
        }
    }

    protected virtual void OnDestroy()
    {
        if (mInstance == this)
        {
            mInstance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    protected abstract void OnSceneLoaded(Scene scene, LoadSceneMode mode);
}