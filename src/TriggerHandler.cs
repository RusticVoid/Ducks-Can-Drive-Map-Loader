using UnityEngine;
using System;

public class TriggerHandler : MonoBehaviour
{
    public event Action<GameObject> OnTriggered;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggered?.Invoke(other.gameObject);
    }
}