using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Nash
/// </summary>
public class ManagedObjectInfo
{
    private UnityEngine.Object InObject;
    private string InTag;
    private float Significance = 1.0f;
    private SignificanceManager.PostSignificanceType PostSignificanceType;

    private SignificanceManager.FManagedObjectSignificanceFunction SignificanceFunction;
    private SignificanceManager.FManagedObjectPostSignificanceFunction PostSignificanceFunction;

    public ManagedObjectInfo(UnityEngine.Object InObject, string InTag, float Significance, 
        SignificanceManager.PostSignificanceType PostSignificanceType, 
        SignificanceManager.FManagedObjectSignificanceFunction SignificanceFunction, 
        SignificanceManager.FManagedObjectPostSignificanceFunction PostSignificanceFunction)
    {
        this.InObject = InObject;
        this.InTag = InTag;
        this.Significance = Significance;
        this.SignificanceFunction = SignificanceFunction;
        this.PostSignificanceType = PostSignificanceType;
        this.PostSignificanceFunction = PostSignificanceFunction;
    }

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

    public void UpdateSignificance(List<Transform> ViewPoints, bool bSortSignificanceAscending)
    {
        float OldSignificance = Significance;
        if (ViewPoints.Count > 0)
        {
            if (bSortSignificanceAscending)
            {
                Significance = float.MaxValue;
                foreach (Transform ViewPoint in ViewPoints)
                {
                    float ViewPointSignificance = SignificanceFunction(this, ViewPoint);
                    if (ViewPointSignificance < Significance)
                    {
                        Significance = ViewPointSignificance;
                    }
                }
            }
            else
            {
                Significance = float.MinValue;
                foreach (Transform ViewPoint in ViewPoints)
                {
                    float ViewPointSignificance = SignificanceFunction(this, ViewPoint);
                    if (ViewPointSignificance > Significance)
                    {
                        Significance = ViewPointSignificance;
                    }
                }
            }
        }
        else
        {
            Significance = 0f;
        }

        if (PostSignificanceType == SignificanceManager.PostSignificanceType.Concurrent)
        {
            PostSignificanceFunction(this, OldSignificance, Significance, false);
        }
    }
}