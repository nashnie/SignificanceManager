using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagedObjectInfo
{
    private UnityEngine.Object InObject;
    private string InTag;
    private float Significance = 1.0f;
    private SignificanceManager.PostSignificanceType PostSignificanceType;

    private SignificanceManager.FManagedObjectSignificanceFunction SignificanceFunction;
    private SignificanceManager.FManagedObjectPostSignificanceFunction PostSignificanceFunction;

    public UnityEngine.Object GetObject()
    {
        return InObject;
    }

    public string GetTag()
    {
        return InTag;
    }

    public float GetSignificance()
    {
        return Significance;
    }

    public SignificanceManager.FManagedObjectSignificanceFunction GetSignificanceFunction()
    {
        return SignificanceFunction;
    }

    public SignificanceManager.FManagedObjectPostSignificanceFunction GetPostSignificanceFunction()
    {
        return PostSignificanceFunction;
    }

    public SignificanceManager.PostSignificanceType GetPostSignificanceType()
    {
        return PostSignificanceType;
    }

    void UpdateSignificance(List<Transform> ViewPoints, bool bSortSignificanceAscending)
    {

    }
}