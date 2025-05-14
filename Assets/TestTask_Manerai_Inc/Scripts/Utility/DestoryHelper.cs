using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryHelper : MonoBehaviour
{
    public GameObject particle;
    
    public void CreateDestoryEffect()
    {
        GameObject temp = Instantiate(particle , transform.position , transform.rotation);
        temp.GetComponent<ParticleSystem>().Play();
        Destroy(gameObject);
    }
}
