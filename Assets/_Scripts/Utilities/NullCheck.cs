using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullCheck : StaticInstance<NullCheck>
{
    public static void CheckNull(Object name)
    {
        if (name is null)
        {
            Debug.LogError(name+" is null!");
        }
    }   
}
    
