using System.Collections;
using UnityEngine;
using System.Collections;
 
public class TestObjScreenSize : MonoBehaviour
{
    public Transform target;
    public Texture2D texture;

    private float distance;
    private float diameter;
    private float angularSize;
    private float pixelSize;
    private Vector3 scrPos;

    void Start()
    {
        diameter = target.GetComponent<Collider>().bounds.extents.magnitude;
    }

    void Update()
    {
        distance = Vector3.Distance(target.position, Camera.main.transform.position);
        angularSize = (diameter / distance) * Mathf.Rad2Deg;
        pixelSize = ((angularSize * Screen.height) / Camera.main.fieldOfView);
        scrPos = Camera.main.WorldToScreenPoint(target.position);
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(scrPos.x - pixelSize / 2, Screen.height - scrPos.y - pixelSize / 2, pixelSize, pixelSize), texture);
    }
}