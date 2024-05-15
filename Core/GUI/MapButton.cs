using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapButton : MonoBehaviour
{
    public float alphaThreshold = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<UnityEngine.UI.Image>().alphaHitTestMinimumThreshold = alphaThreshold;
    }
}
