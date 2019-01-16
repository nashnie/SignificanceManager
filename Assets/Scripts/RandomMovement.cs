using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nash
/// </summary>
public class RandomMovement : MonoBehaviour
{
    public float moveSpeed;
    private bool turnRound;
    private int frame;
    private int maxFrame = 300;
    // Start is called before the first frame update
    void Start()
    { 
    }

    // Update is called once per frame
    void Update()
    {
        frame++;
        if (frame >= maxFrame)
        {
            frame = 0;
            turnRound = !turnRound;
        }
        float direction = turnRound ? 1.0f : -1.0f; 
        transform.position += Vector3.forward * moveSpeed * direction * Time.deltaTime;
    }
}
