﻿using UnityEngine;

namespace SkyMavis.AxieMixer.Unity.Demo
{
    public class Obstacle : MonoBehaviour
    {
        void Update()
        {
            transform.position += ((Vector3.left * 2) * Time.deltaTime);
        }
    }
}
