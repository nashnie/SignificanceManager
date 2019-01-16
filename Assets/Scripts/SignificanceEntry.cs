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

    public float significanceDistance = 50f;
    public float significancePixelSize = 100f;

    private List<Transform> transformArray;

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

        //distance、visibility、screen size
        //TODO occlusionCulling 

        if (distance < significanceDistance)
        {
            Collider collider = significanceActor.GetComponent<Collider>();
            if (collider)
            {
                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
                if (GeometryUtility.TestPlanesAABB(planes, collider.bounds))
                {
                    float diameter = collider.bounds.extents.magnitude;
                    float distanceToCamera = Vector3.Distance(mainCamera.transform.position, significanceActor.position);
                    float angularSize = (diameter / distanceToCamera) * Mathf.Rad2Deg;
                    float pixelSize = ((angularSize * Screen.height) / mainCamera.fieldOfView);
                    float distanceSignificance = 1f - distance / significanceDistance;
                    pixelSize = pixelSize > significancePixelSize ? significancePixelSize : pixelSize;
                    float pixelSignificance = (1 - distanceSignificance) * pixelSize / significancePixelSize;//能量守恒

                    return distanceSignificance + pixelSignificance;
                }
            }
        }
        return 0.0f;
    }

    public void PostSignificanceFunction(ManagedObjectInfo objectInfo, float oldSignificance, float significance, bool bUnregistered)
    {
        if (significance > 0f)
        {
            //提高 AI tick 频率，设置粒子发射器等等
            //设置 lod level
        }
        else
        {
            //关闭 AI tick，关闭粒子等
            //关闭 lod
        }

#if UNITY_EDITOR
        Transform significanceActor = (Transform)objectInfo.GetObject();
        DebugHUD textMesh = significanceActor.GetComponentInChildren<DebugHUD>();
        textMesh.ShowDebugView(significance, debugDisplayInfo.ShouldDisplayDebug);
#endif
    }
}
