using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public string prompt;
    public string InteractionPrompt => prompt;
    public bool Interact(Interactor interactor)
    {
        Debug.Log("Interacting with door");
        return true;
    }
}
