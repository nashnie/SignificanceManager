using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SignificanceManager : MonoBehaviour
{
    public delegate void FSignificanceFunction(UnityEngine.Object inObject, Transform transform);
    public delegate void FPostSignificanceFunction(UnityEngine.Object inObject, float param1, float param2, bool param3);

    public delegate void FManagedObjectSignificanceFunction(ManagedObjectInfo ManagedObjectInfo, Transform transform);
    public delegate void FManagedObjectPostSignificanceFunction(ManagedObjectInfo ManagedObjectInfo, float param1, float param2, bool param3);

    public enum PostSignificanceType
    {
        None,
        Concurrent,
        Sequntial,
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RegisterObject(UnityEngine.Object InObject, string Tag, FSignificanceFunction SignificanceFunction, PostSignificanceType PostSignificanceType = PostSignificanceType.None, FPostSignificanceFunction PostSignificanceFunction = null)
    {

    }

    public void RegisterObject(UnityEngine.Object InObject, string Tag, FManagedObjectSignificanceFunction ManagedObjectSignificanceFunction, PostSignificanceType PostSignificanceType = PostSignificanceType.None, FManagedObjectPostSignificanceFunction ManagedObjectPostSignificanceFunction = null)
    {

    }

    public void UnregisterObject(UnityEngine.Object InObject)
    {

    }

    public void UnregisterAll(string Tag)
    {

    }

    public List<ManagedObjectInfo> GetManagedObjects(string Tag)
    {
        return null;
    }

    public void GetManagedObjects(List<ManagedObjectInfo> OutManagedObjects, bool bInSignificanceOrder = false)
    {

    }

    public float GetSignificance(UnityEngine.Object InObject)
    {
        return 1.0f;
    }

    public bool QuerySignificance(UnityEngine.Object InObject, float OutSignificance)
    {
        return false;
    }

    public List<Transform> GetViewpoints()
    {
        return null;
    }

    //TODO proteced functuions and variables
}


