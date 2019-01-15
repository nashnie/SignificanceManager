using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignificanceEntry : MonoBehaviour
{
    public SignificanceManager significanceManagerInstace;
    public GameObject significanceObjectContainer;
    public static string Tag = "group1";
    public Transform Player;
    public DebugDisplayInfo debugDisplayInfo;
    private List<Transform> transformArray;

    private static float significanceDistance = 30f;

    // Start is called before the first frame update
    void Start()
    {
        debugDisplayInfo = new DebugDisplayInfo();
        transformArray = new List<Transform>();
        significanceManagerInstace = significanceObjectContainer.AddComponent<SignificanceManager>();
        int childCount = significanceObjectContainer.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform significanceObject = significanceObjectContainer.transform.GetChild(i);
            significanceManagerInstace.RegisterObject(significanceObject, Tag, SignificanceFunction, SignificanceManager.PostSignificanceType.Sequntial, PostSignificanceFunction);
        }

        debugDisplayInfo.ShouldDisplayDebug = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player)
        {
            if (transformArray.Contains(Player) == false)
            {
                transformArray.Add(Player);
            }
            significanceManagerInstace.UpdateSignificance(transformArray);
        }
    }

    public float SignificanceFunction(ManagedObjectInfo objectInfo, Transform transform)
    {
        Debug.Log("SignificanceFunction");
        Transform significanceActor = (Transform)objectInfo.GetObject();
        float distance = Vector3.Distance(transform.position, significanceActor.position);

        if (distance < significanceDistance)
        {
            float Significance = 1f - distance / significanceDistance;
            return Significance;
        }
        return 0f;
    }

    public void PostSignificanceFunction(ManagedObjectInfo objectInfo, float oldSignificance, float significance, bool bUnregistered)
    {
        Debug.Log("PostSignificanceFunction");
        if (significance > 0f)
        {
            if (debugDisplayInfo.ShouldDisplayDebug)
            {
                Transform significanceActor = (Transform)objectInfo.GetObject();
                TextMesh textMesh = significanceActor.GetComponentInChildren<TextMesh>();
                if (textMesh)
                {
                    textMesh.color = Color.green * significance;
                }
            }
        }
        else
        {
            if (debugDisplayInfo.ShouldDisplayDebug)
            {
                Transform significanceActor = (Transform)objectInfo.GetObject();
                TextMesh textMesh = significanceActor.GetComponentInChildren<TextMesh>();
                if (textMesh)
                {
                    textMesh.color = Color.red;
                }
            }
        }
    }
}
