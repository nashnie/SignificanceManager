using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Nash 
/// MainEntry
/// </summary>
public class SignificanceEntry : MonoBehaviour
{
    public SignificanceManager significanceManagerInstace;
    public GameObject significanceObjectContainer;
    public static string Tag = "";
    public Transform player;
    public Camera mainCamera;
    public DebugDisplayInfo debugDisplayInfo;
    private List<Transform> transformArray;

    private static float significanceDistance = 30f;

    // Start is called before the first frame update
    void Start()
    {
        Tag = significanceObjectContainer.tag;
        debugDisplayInfo = new DebugDisplayInfo();
        transformArray = new List<Transform>();
        significanceManagerInstace = significanceObjectContainer.AddComponent<SignificanceManager>();
        int childCount = significanceObjectContainer.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform significanceObject = significanceObjectContainer.transform.GetChild(i);
            significanceManagerInstace.RegisterObject(significanceObject, Tag, SignificanceFunction, SignificanceManager.PostSignificanceType.Sequntial, PostSignificanceFunction);
        }
        transformArray.Add(player);

        debugDisplayInfo.ShouldDisplayDebug = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (player)
        {
            significanceManagerInstace.UpdateSignificance(transformArray);
        }
    }

    public float SignificanceFunction(ManagedObjectInfo objectInfo, Transform transform)
    {
        Transform significanceActor = (Transform)objectInfo.GetObject();
        float distance = Vector3.Distance(transform.position, significanceActor.position);

        //distance、visibility
        //TODO screen size...
        if (distance < significanceDistance)
        {     
            Collider collider = significanceActor.GetComponent<Collider>();
            if (collider)
            {
                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
                if (GeometryUtility.TestPlanesAABB(planes, collider.bounds))
                {
                    float significance = 1f - distance / significanceDistance;
                    return significance;
                }
                else
                {
                    return 0f;
                }
            }
        }
        return 0f;
    }

    public void PostSignificanceFunction(ManagedObjectInfo objectInfo, float oldSignificance, float significance, bool bUnregistered)
    {
        if (significance > 0f)
        {
        }
        else
        {
        }

#if UNITY_EDITOR
        Transform significanceActor = (Transform)objectInfo.GetObject();
        DebugHUD textMesh = significanceActor.GetComponentInChildren<DebugHUD>();
        textMesh.ShowDebugView(significance, debugDisplayInfo.ShouldDisplayDebug);
#endif
    }
}
