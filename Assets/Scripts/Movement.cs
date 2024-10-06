using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public GameObject ally;
    public float speed = 5f; 
    private Vector2 targetPosition;
    Rigidbody2D rb2d;
    void Start()
    {
        rb2d = ally.GetComponent<Rigidbody2D>();
        
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space)))
        {
            Vector2 destination = rb2d.position + Vector2.up * 2;
            MoveToPosition(destination);
        }
    }
    void MoveToPosition(Vector2 destination)
    {
        targetPosition = destination;
        StartCoroutine(MoveToDestination());
    }

    IEnumerator MoveToDestination()
    {
        while (rb2d.position != targetPosition)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            yield return null;
        }
    }


}
