using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 m_Velocity;
    private Collider2D m_Collider;
    [SerializeField] private LayerMask m_CollidesWith;

    private void Awake()
    {
        m_Collider = gameObject.GetComponent<Collider2D>();
    }

    private void Start()
    {
        LevelManager.m_OnSpawnEvent.AddListener(DestroyThis);
    }

    private void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
        if (player != null)
        {
            player.Kill();
        }
        // destroy this after half a second
        Destroy(gameObject, 0.1f);
    }

    public void Whitelist(GameObject otherObject)
    {
        Physics2D.IgnoreCollision(otherObject.GetComponent<Collider2D>(), m_Collider);
    }

    public void SetVelocity(Vector2 vel)
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = vel;
        m_Velocity = vel;
    }
    
    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
