using UnityEngine;
public class PhysicRig : MonoBehaviour
{
    public Transform playerHead;
    public CapsuleCollider bodyCollider;

    public float bodyHeightmin = 0.5f;
    public float bodyHeightmax = 2.0f;
    
    void FixedUpdate()
    {
        bodyCollider.height = Mathf.Clamp(playerHead.localPosition.y, bodyHeightmin, bodyHeightmax);
        bodyCollider.center = new Vector3(playerHead.localPosition.x, bodyCollider.height/2, playerHead.localPosition.z);
    }
}
