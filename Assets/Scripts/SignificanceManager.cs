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
    List<Transform> Viewpoints;
    Dictionary<string, List<ManagedObjectInfo>> managedObjectsByTag;
    Dictionary<UnityEngine.Object, ManagedObjectInfo> managedObjects;
    List<ManagedObjectInfo> ObjArray;
    List<SequentialPostWorkPair> ObjWithSequentialPostWork;

    static int SignificanceManagerObjectsToShow = 15;

    public enum PostSignificanceType
    {
        None,
        Concurrent,//同时 TODO thread
        Sequntial,//顺序
    }

    public struct SequentialPostWorkPair
    {
        public ManagedObjectInfo ObjectInfo;
        public float OldSignificance;
    }

    private void Awake()
    {
        bSortSignificanceAscending = false;
        Viewpoints = new List<Transform>();
        ObjArray = new List<ManagedObjectInfo>();
        managedObjectsByTag = new Dictionary<string, List<ManagedObjectInfo>>();
        managedObjects = new Dictionary<UnityEngine.Object, ManagedObjectInfo>();
        ObjWithSequentialPostWork = new List<SequentialPostWorkPair>();
    }

    private void OnDestroy()
    {
    }

    public void UpdateSignificance(List<Transform> InViewpoints)
    {
        Viewpoints.Clear();
        Viewpoints.AddRange(InViewpoints);

        ObjArray.Capacity = managedObjects.Count;
        ObjWithSequentialPostWork.Capacity = managedObjectsWithSequentialPostWork;
        foreach (ManagedObjectInfo ObjectInfo in managedObjects.Values)
        {
            ObjArray.Add(ObjectInfo);
            if (ObjectInfo.GetPostSignificanceType() == PostSignificanceType.Sequntial)
            {
                SequentialPostWorkPair sequentialPostWorkPair = new SequentialPostWorkPair();
                sequentialPostWorkPair.ObjectInfo = ObjectInfo;
                sequentialPostWorkPair.OldSignificance = ObjectInfo.GetSignificance();
                ObjWithSequentialPostWork.Add(sequentialPostWorkPair);
            }
        }

        for (int i = 0; i < ObjArray.Count; i++)
        {
            ManagedObjectInfo ObjectInfo = ObjArray[i];
            ObjectInfo.UpdateSignificance(Viewpoints, bSortSignificanceAscending);
        }

        foreach (SequentialPostWorkPair sequentialPostWorkPair in ObjWithSequentialPostWork)
        {
            ManagedObjectInfo objectInfo = sequentialPostWorkPair.ObjectInfo;
            objectInfo.GetPostSignificanceFunction()(objectInfo, sequentialPostWorkPair.OldSignificance, objectInfo.GetSignificance(), false);
        }

        ObjArray.Clear();
        ObjWithSequentialPostWork.Clear();

        foreach (List<ManagedObjectInfo> managedObjectInfos in managedObjectsByTag.Values)
        {
            managedObjectInfos.Sort(CompareBySignificance);
        }
    }

    public void RegisterObject(UnityEngine.Object InObject, string Tag, FSignificanceFunction SignificanceFunction, PostSignificanceType PostSignificanceType = PostSignificanceType.None, FPostSignificanceFunction PostSignificanceFunction = null)
    {

    }

    public void RegisterObject(UnityEngine.Object InObject, string Tag, FManagedObjectSignificanceFunction ManagedObjectSignificanceFunction, PostSignificanceType PostSignificanceType = PostSignificanceType.None, FManagedObjectPostSignificanceFunction ManagedObjectPostSignificanceFunction = null)
    {
        ManagedObjectInfo managedObjectInfo = new ManagedObjectInfo(InObject, Tag, 1.0f, PostSignificanceType, ManagedObjectSignificanceFunction, ManagedObjectPostSignificanceFunction);
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

    public void UnregisterAll(string Tag)
    {
        if (managedObjectsByTag.ContainsKey(Tag))
        {
            List<ManagedObjectInfo> ObjectsWithTag = managedObjectsByTag[Tag];
            foreach (ManagedObjectInfo ManagedObj in ObjectsWithTag)
            {
                managedObjects.Remove(ManagedObj.GetObject());
                if (ManagedObj.GetPostSignificanceFunction() != null)
                {
                    ManagedObj.GetPostSignificanceFunction()(ManagedObj, ManagedObj.GetSignificance(), 1.0f, true);
                }
            }
            managedObjectsByTag.Remove(Tag);
        }
    }

    public List<ManagedObjectInfo> GetManagedObjects(string Tag)
    {
        if (managedObjectsByTag.ContainsKey(Tag))
        {
            return managedObjectsByTag[Tag];
        }
        return null;
    }

    public ManagedObjectInfo GetManagedObject(UnityEngine.Object Object)
    {
       if (managedObjects.ContainsKey(Object))
        {
            return managedObjects[Object];
        }
        return null;
    }

    public void GetManagedObjects(out List<ManagedObjectInfo> OutManagedObjects, bool bInSignificanceOrder = false)
    {
        OutManagedObjects = new List<ManagedObjectInfo>(managedObjects.Count);
        foreach (List<ManagedObjectInfo> managedObjectInfos in managedObjectsByTag.Values)
        {
            OutManagedObjects.AddRange(managedObjectInfos);
        }
        if (bInSignificanceOrder)
        {
            OutManagedObjects.Sort(CompareBySignificance);
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

    public float GetSignificance(UnityEngine.Object InObject)
    {
        float Significance = 0f;
        if (managedObjects.ContainsKey(InObject))
        {
            Significance = managedObjects[InObject].GetSignificance();
        }
        return Significance;
    }

    public bool QuerySignificance(UnityEngine.Object InObject, out float OutSignificance)
    {
        if (managedObjects.ContainsKey(InObject))
        {
            OutSignificance = managedObjects[InObject].GetSignificance();
            return true;
        }
        OutSignificance = 0f;
        return false;
    }

    public List<Transform> GetViewpoints()
    {
        return null;
    }

    protected void RegisterManagedObject(ManagedObjectInfo objectInfo)
    {
        UnityEngine.Object Object = objectInfo.GetObject();
        if (objectInfo.GetPostSignificanceType() == PostSignificanceType.Sequntial)
        {
            ++managedObjectsWithSequentialPostWork;
        }
        if (Viewpoints.Count > 0)
        {
            objectInfo.UpdateSignificance(Viewpoints, bSortSignificanceAscending);

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


