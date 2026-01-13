using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private Transform player;
    public float SmoothSpeed = 0.1f;
    private Vector3 offset;
    private Vector3 velocity;

    void Start()
    {
        offset = transform.position - player.position;
    }
    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, player.position + offset, ref velocity, SmoothSpeed);
    }
}
