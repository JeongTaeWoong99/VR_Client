using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] UnityEvent onTriggerEnter;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            onTriggerEnter.Invoke();
            
    }
}
