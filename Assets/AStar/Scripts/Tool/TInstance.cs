using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TInstance<T> : MonoBehaviour where T : TInstance<T>
{
    private static T instance;

    public static T Instance
    {
        get
        {
            return instance;
        }

    }


    private void Awake()
    {
        if(Instance == null || Instance  == this)
        {
            instance = (T)this;
        }
        else
        {
            Destroy(this);
        }
    }
}
