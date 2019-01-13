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

    protected bool bSortSignificanceAscending;
    private int managedObjectsWithSequentialPostWork;
    List<Transform> Viewpoints;
    Dictionary<string, List<ManagedObjectInfo>> managedObjectsByTag;
    Dictionary<UnityEngine.Object, ManagedObjectInfo> managedObjects;
    List<ManagedObjectInfo> ObjArray;
    List<SequentialPostWorkPair> ObjWithSequentialPostWork;

    public enum PostSignificanceType
    {
        None,
        Concurrent,
        Sequntial,
    }

    struct SequentialPostWorkPair
    {
        ManagedObjectInfo ObjectInfo;
        float OldSignificance;
    }

    // Start is called before the first frame update
    void Start()
    {
        bSortSignificanceAscending = false;
    }

    private void OnDestroy()
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

    private void OnShowDebugInfo(HUD hud, DebugDisplayInfo displayInfo, float yl, float ypos)
    {
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


