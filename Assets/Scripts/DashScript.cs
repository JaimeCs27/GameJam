using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashScript : MonoBehaviour
{
    bool dash_available = true;
    bool is_dashing = false;
    float dashing_power;
    float dashing_time;
    float dashing_cooldown;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TrailRenderer trail;

    private void Update() {
        
    }

    
}