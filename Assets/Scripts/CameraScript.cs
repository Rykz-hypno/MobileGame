using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private Transform player;
    public float SmoothSpeedX = 0.3f;
    public float SmoothSpeedY = 0.05f;
    private Vector3 offset;
    private Vector3 velocityX;
    private Vector3 velocityY;

    void Start()
    {
        offset = transform.position - player.position;
    }
    
    void LateUpdate()
    {
        Vector3 targetPosition = player.position + offset;
        
        Vector3 currentPos = transform.position;
        currentPos.x = Mathf.SmoothDamp(currentPos.x, targetPosition.x, ref velocityX.x, SmoothSpeedX);
        currentPos.y = Mathf.SmoothDamp(currentPos.y, targetPosition.y, ref velocityY.y, SmoothSpeedY);
        
        transform.position = currentPos;
    }
}
