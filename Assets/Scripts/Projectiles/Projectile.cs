using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 400f;
    public float maxTraveledDistance = 50f;

    [SerializeField] private GameObject impactPrefab;
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.velocity = transform.forward * speed;
    }

    private void Update()
    {
        maxTraveledDistance -= Time.deltaTime * speed;
        if (maxTraveledDistance <= 0f)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Projectile"))
            return;
        
        if (other.collider.CompareTag("Player"))
            return;
        
        Destroy(gameObject);

        ContactPoint contactPoint = other.contacts[0];
        Instantiate(impactPrefab, contactPoint.point, Quaternion.LookRotation(contactPoint.normal));
    }
}
