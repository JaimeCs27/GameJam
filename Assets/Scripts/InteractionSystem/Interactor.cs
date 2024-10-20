using System;
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
    public InteractionPromptUI ui;

    public IInteractable interactable;

    private void Update()
    {
        numFound = Physics.OverlapSphereNonAlloc(interactPoint.position, interactPointRadius, colliders, interactableMask);
        if (numFound > 0)
        {
            interactable = colliders[0].GetComponent<IInteractable>();
            if (interactable != null)
            {
                if (!ui.isDisplayed) ui.SetUp("Press L to interact");
                if (Input.GetKeyDown(KeyCode.L)) interactable.Interact(this);
            }
        }
        else
        {
            if (interactable != null) interactable = null;
            if (ui.isDisplayed) ui.Close();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(interactPoint.position, interactPointRadius);
    }
}