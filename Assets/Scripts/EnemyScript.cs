using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public GameObject player;
    public float speed = 3f;

    private float distanceToPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);

    }
}
