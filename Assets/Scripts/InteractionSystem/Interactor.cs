using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour 
{
    public Transform interactPoint;
    public float interactPointRadius = 0.5f;
    public LayerMask interactableMask;
    private readonly Collider[] colliders = new Collider[3];
    public int numFound;

    private void Update() 
    {
        numFound = Physics.OverlapSphereNonAlloc(interactPoint.position, interactPointRadius, colliders, interactableMask);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(interactPoint.position, interactPointRadius);
    }
}