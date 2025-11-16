using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneTimeEffect : MonoBehaviour
{
    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
