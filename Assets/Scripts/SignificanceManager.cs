using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// Nash
/// </summary>
public class SignificanceManager : MonoBehaviour
{
    public delegate float FSignificanceFunction(UnityEngine.Object inObject, Transform transform);
    public delegate void FPostSignificanceFunction(UnityEngine.Object inObject, float oldSignificance, float significance, bool bUnregistered);

    public delegate float FManagedObjectSignificanceFunction(ManagedObjectInfo ManagedObjectInfo, Transform transform);
    public delegate void FManagedObjectPostSignificanceFunction(ManagedObjectInfo ManagedObjectInfo, float oldSignificance, float significance, bool bUnregistered);

    protected bool bSortSignificanceAscending;
    private int managedObjectsWithSequentialPostWork;
    List<Transform> viewpoints;
    Dictionary<string, List<ManagedObjectInfo>> managedObjectsByTag;
    Dictionary<UnityEngine.Object, ManagedObjectInfo> managedObjects;
    List<ManagedObjectInfo> objArray;
    List<SequentialPostWorkPair> objWithSequentialPostWork;

    static int SignificanceManagerObjectsToShow = 15;

    public enum PostSignificanceType
    {
        None,
        Concurrent,//同时 TODO thread
        Sequntial,//顺序
    }

    public struct SequentialPostWorkPair
    {
        public ManagedObjectInfo objectInfo;
        public float oldSignificance;
    }

    private void Awake()
    {
        bSortSignificanceAscending = false;
        viewpoints = new List<Transform>();
        objArray = new List<ManagedObjectInfo>();
        managedObjectsByTag = new Dictionary<string, List<ManagedObjectInfo>>();
        managedObjects = new Dictionary<UnityEngine.Object, ManagedObjectInfo>();
        objWithSequentialPostWork = new List<SequentialPostWorkPair>();
    }

    private void OnDestroy()
    {
        bSortSignificanceAscending = false;
        viewpoints = null;
        objArray = null;
        managedObjectsByTag = null;
        managedObjects = null;
        managedObjects = null;
        objWithSequentialPostWork = null;
    }

    public void UpdateSignificance(List<Transform> inViewpoints)
    {
        viewpoints.Clear();
        viewpoints.AddRange(inViewpoints);

        objArray.Capacity = managedObjects.Count;
        objWithSequentialPostWork.Capacity = managedObjectsWithSequentialPostWork;
        foreach (ManagedObjectInfo ObjectInfo in managedObjects.Values)
        {
            objArray.Add(ObjectInfo);
            if (ObjectInfo.GetPostSignificanceType() == PostSignificanceType.Sequntial)
            {
                SequentialPostWorkPair sequentialPostWorkPair = new SequentialPostWorkPair();
                sequentialPostWorkPair.objectInfo = ObjectInfo;
                sequentialPostWorkPair.oldSignificance = ObjectInfo.GetSignificance();
                objWithSequentialPostWork.Add(sequentialPostWorkPair);
            }
        }

        for (int i = 0; i < objArray.Count; i++)
        {
            ManagedObjectInfo ObjectInfo = objArray[i];
            ObjectInfo.UpdateSignificance(viewpoints, bSortSignificanceAscending);
        }

        foreach (SequentialPostWorkPair sequentialPostWorkPair in objWithSequentialPostWork)
        {
            ManagedObjectInfo objectInfo = sequentialPostWorkPair.objectInfo;
            objectInfo.GetPostSignificanceFunction()(objectInfo, sequentialPostWorkPair.oldSignificance, objectInfo.GetSignificance(), false);
        }

        objArray.Clear();
        objWithSequentialPostWork.Clear();

        foreach (List<ManagedObjectInfo> managedObjectInfos in managedObjectsByTag.Values)
        {
            managedObjectInfos.Sort(CompareBySignificance);
        }
    }

    public void RegisterObject(UnityEngine.Object inObject, string tag, FManagedObjectSignificanceFunction managedObjectSignificanceFunction, PostSignificanceType postSignificanceType = PostSignificanceType.None, FManagedObjectPostSignificanceFunction managedObjectPostSignificanceFunction = null)
    {
        ManagedObjectInfo managedObjectInfo = new ManagedObjectInfo(
                                                                    inObject, 
                                                                    tag, 
                                                                    1.0f, 
                                                                    postSignificanceType, 
                                                                    managedObjectSignificanceFunction, 
                                                                    managedObjectPostSignificanceFunction);
        RegisterManagedObject(managedObjectInfo);
    }

    public void UnregisterObject(UnityEngine.Object InObject)
    {
        ManagedObjectInfo objectInfo = RemoveAndReturnValue(InObject, managedObjects);
        if (objectInfo != null)
        {
            if (objectInfo.GetPostSignificanceType() == PostSignificanceType.Sequntial)
            {
                --managedObjectsWithSequentialPostWork;
            }
            if (managedObjectsByTag.ContainsKey(objectInfo.GetTag()))
            {
                List<ManagedObjectInfo> ObjectsWithTag = managedObjectsByTag[objectInfo.GetTag()];
                if (ObjectsWithTag.Count == 1)
                {
                    managedObjectsByTag.Remove(objectInfo.GetTag());
                }
                else
                {
                    ObjectsWithTag.Remove(objectInfo);
                }
                FManagedObjectPostSignificanceFunction ManagedObjectPostSignificanceFunction = objectInfo.GetPostSignificanceFunction();
                if (ManagedObjectPostSignificanceFunction != null)
                {
                    ManagedObjectPostSignificanceFunction(objectInfo, objectInfo.GetSignificance(), 1.0f, true);
                }
            }

            objectInfo = null;
        }
    }

    public void UnregisterAll(string tag)
    {
        if (managedObjectsByTag.ContainsKey(tag))
        {
            List<ManagedObjectInfo> ObjectsWithTag = managedObjectsByTag[tag];
            foreach (ManagedObjectInfo ManagedObj in ObjectsWithTag)
            {
                managedObjects.Remove(ManagedObj.GetObject());
                if (ManagedObj.GetPostSignificanceFunction() != null)
                {
                    ManagedObj.GetPostSignificanceFunction()(ManagedObj, ManagedObj.GetSignificance(), 1.0f, true);
                }
            }
            managedObjectsByTag.Remove(tag);
        }
    }

    public List<ManagedObjectInfo> GetManagedObjects(string tag)
    {
        if (managedObjectsByTag.ContainsKey(tag))
        {
            return managedObjectsByTag[tag];
        }
        return null;
    }

    public ManagedObjectInfo GetManagedObject(UnityEngine.Object inObject)
    {
       if (managedObjects.ContainsKey(inObject))
        {
            return managedObjects[inObject];
        }
        return null;
    }

    public void GetManagedObjects(out List<ManagedObjectInfo> outManagedObjects, bool bInSignificanceOrder = false)
    {
        outManagedObjects = new List<ManagedObjectInfo>(managedObjects.Count);
        foreach (List<ManagedObjectInfo> managedObjectInfos in managedObjectsByTag.Values)
        {
            outManagedObjects.AddRange(managedObjectInfos);
        }
        if (bInSignificanceOrder)
        {
            outManagedObjects.Sort(CompareBySignificance);
        }
    }

    //Ascending
    private int CompareBySignificance(ManagedObjectInfo x, ManagedObjectInfo y)
    {
        if (x.GetSignificance() > y.GetSignificance())
        {
            return -1;
        }
        else if (x.GetSignificance() < y.GetSignificance())
        {
            return 1;
        }
        return 0;
    }

    public float GetSignificance(UnityEngine.Object inObject)
    {
        float significance = 0f;
        if (managedObjects.ContainsKey(inObject))
        {
            significance = managedObjects[inObject].GetSignificance();
        }
        return significance;
    }

    public bool QuerySignificance(UnityEngine.Object inObject, out float outSignificance)
    {
        if (managedObjects.ContainsKey(inObject))
        {
            outSignificance = managedObjects[inObject].GetSignificance();
            return true;
        }
        outSignificance = 0f;
        return false;
    }

    public List<Transform> GetViewpoints()
    {
        return viewpoints;
    }

    protected void RegisterManagedObject(ManagedObjectInfo objectInfo)
    {
        UnityEngine.Object Object = objectInfo.GetObject();
        if (objectInfo.GetPostSignificanceType() == PostSignificanceType.Sequntial)
        {
            ++managedObjectsWithSequentialPostWork;
        }
        if (viewpoints.Count > 0)
        {
            objectInfo.UpdateSignificance(viewpoints, bSortSignificanceAscending);

            if (objectInfo.GetPostSignificanceType() == PostSignificanceType.Sequntial)
            {
                FManagedObjectPostSignificanceFunction PostSignificanceFunction = objectInfo.GetPostSignificanceFunction();
                PostSignificanceFunction(objectInfo, 1.0f, objectInfo.GetSignificance(), false);
            }
        }

        managedObjects.Add(Object, objectInfo);

        List<ManagedObjectInfo> managedObjectInfos = FindOrAdd(objectInfo.GetTag(), managedObjectsByTag);
        if (managedObjectInfos.Count > 0)
        {
            int LowIndex = 0;
            int HighIndex = managedObjectInfos.Count - 1;
            while (true)
            {
                int MidIndex = LowIndex + (HighIndex - LowIndex) / 2;
                if (CompareBySignificanceAscending(objectInfo, managedObjectInfos[MidIndex]))
                {
                    if (LowIndex == MidIndex)
                    {
                        managedObjectInfos.Insert(LowIndex, objectInfo);
                        break;
                    }
                    else
                    {
                        HighIndex = MidIndex - 1;
                    }
                }
                else if (LowIndex == HighIndex)
                {
                    managedObjectInfos.Insert(LowIndex + 1, objectInfo);
                    break;
                }
                else
                {
                    LowIndex = MidIndex + 1;
                }
            }
        }
        else
        {
            managedObjectInfos.Add(objectInfo);
        }
    }

    private List<ManagedObjectInfo> FindOrAdd(string Tag, Dictionary<string, List<ManagedObjectInfo>> ManagedObjectsByTag)
    {
        List<ManagedObjectInfo> ManagedObjectInfos;
        if (ManagedObjectsByTag.ContainsKey(Tag))
        {
            ManagedObjectInfos = ManagedObjectsByTag[Tag];
        }
        else
        {
            ManagedObjectInfos = new List<ManagedObjectInfo>();
            ManagedObjectsByTag.Add(Tag, ManagedObjectInfos);
        }
        return ManagedObjectInfos;
    }

    private ManagedObjectInfo RemoveAndReturnValue(UnityEngine.Object Object, Dictionary<UnityEngine.Object, ManagedObjectInfo> managedObjects)
    {
        ManagedObjectInfo managedObjectInfo = null;
        if (managedObjects.ContainsKey(Object))
        {
            managedObjectInfo = managedObjects[Object];
            managedObjects.Remove(Object);
        }
        return managedObjectInfo;
    }

    private bool CompareBySignificanceAscending(ManagedObjectInfo A, ManagedObjectInfo B)
    {
        return A.GetSignificance() < B.GetSignificance();
    }
}


