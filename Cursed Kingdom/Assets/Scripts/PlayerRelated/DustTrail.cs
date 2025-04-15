using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustTrail : MonoBehaviour
{
    public ParticleSystem dust;
    // Start is called before the first frame update
    public void Dust()
    {
        dust.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
