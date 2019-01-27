using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    [Range(0, 50f)] [SerializeField] private float m_AttractionForce = 10f;
    [Range(0, 2f)] [SerializeField] private float m_ShrinkSpeedPlayer = 1f;
    [Range(0, 2f)] [SerializeField] private float m_ShrinkSpeedSelf = 0.75f;
    [Range(0, 1f)] [SerializeField] private float m_ShrinkMin = 0.5f;
    [Range(0, 1f)] [SerializeField] private float m_ShrinkMinPlayer = 0f;
    [Range(0, 10f)] [SerializeField] private float m_Damping = 5f;
    [Range(-20f, 20f)] [SerializeField] private float m_Torque = 10f;
    [Range(-20f, 20)] [SerializeField] private float m_TorquePlayer = 10f;

    private LevelManager m_LevelManager;
    private bool m_IsLevelEndet = false;
    private GameObject m_Player;
    private float m_PlayerSize;
    private float m_Size;

    private void FixedUpdate()
    {
        // do level ending animation
        if (m_IsLevelEndet)
        {
            // pull player towards this
            Vector2 deltaPos = transform.position - m_Player.transform.position;
            Rigidbody2D t_RB = m_Player.GetComponent<Rigidbody2D>();
            t_RB.AddForce(deltaPos * m_AttractionForce * deltaPos.magnitude);
            t_RB.AddForce(-t_RB.velocity * m_Damping);

            // torque to player
            m_Player.transform.Find("SpriteFollower").GetComponent<Rigidbody2D>().AddTorque(m_TorquePlayer);

            // shrink player
            m_PlayerSize -= m_ShrinkSpeedPlayer * Time.fixedDeltaTime;
            if (m_PlayerSize < m_ShrinkMinPlayer)
                m_PlayerSize = m_ShrinkMinPlayer;
            m_Player.GetComponent<Transform>().localScale = Vector2.one * m_PlayerSize;

            // spin
            GetComponent<Rigidbody2D>().AddTorque(m_Torque);

            // shrink self
            m_Size -= m_ShrinkSpeedSelf * Time.fixedDeltaTime;
            if (m_Size < m_ShrinkMin)
                m_Size = m_ShrinkMin;
            transform.localScale = Vector2.one * m_Size;
        }
    }

    private void Start()
    {
        m_LevelManager = GameObject.Find("Level").GetComponent<LevelManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if player end level
        if (collision.gameObject.GetComponent<CharacterController2D>() != null && m_LevelManager.m_LevelState == LevelManager.LevelState.Playing)
        {
            m_Player = m_LevelManager.GetPlayer().gameObject;
            m_IsLevelEndet = true;
            m_LevelManager.LevelDone();
            m_Player.GetComponent<Rigidbody2D>().gravityScale = 0f;
            m_PlayerSize = m_Player.GetComponent<Transform>().localScale.x;
            m_Size = transform.localScale.x;
        }
    }

}
