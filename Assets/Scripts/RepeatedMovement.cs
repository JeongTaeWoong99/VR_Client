using UnityEngine;

public class RepeatedMovement : MonoBehaviour
{
    // Movement range and speed exposed in the Inspector
    [SerializeField] private float movementRange = 5f; // Range of movement
    [SerializeField] private float speed = 2f;         // Speed of movement

    private Vector3 startPosition;

    void Start()
    {
        // Save the starting position of the object
        startPosition = transform.position;
    }

    void Update()
    {
        // Calculate the new position using Mathf.Sin for smooth back-and-forth movement
        float movement = Mathf.Sin(Time.time * speed) * movementRange;
        transform.position = new Vector3(startPosition.x + movement, startPosition.y, startPosition.z);
    }
}