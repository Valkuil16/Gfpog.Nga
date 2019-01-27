using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CharacterDudeGrav : CharacterController2D
{
    [Range(0, 5f)] [SerializeField] private float m_HoverTime = 2f;         // the time in seconds the character can hover
    [Range(0, 3f)] [SerializeField] private float m_HoverStrength = 0.5f;   // the strength of the anti gravity hover

    private float m_HoverTimeLeft;
    private Rigidbody2D m_Ridgidbody;

    private void Start()
    {
        CharacterController2D.OnLandEvent.AddListener(delegate { OnLand(); });
        CharacterController2D.OnDeathEvent.AddListener(delegate { OnDeath(); });
        m_HoverTimeLeft = m_HoverTime;
        m_Ridgidbody = GetComponent<Rigidbody2D>();
    }

    public override void Move(float move, bool crouch, bool jump)
    {
        base.Move(move, crouch, jump);

        // can't jump but can hover
        if (jump && m_CanJump && !m_Grounded && m_HoverTimeLeft > 0)
        {
            // hover
            m_Ridgidbody.gravityScale = -m_HoverStrength;

            m_HoverTimeLeft -= Time.fixedDeltaTime;
        }
    }

    private void OnLand()
    {
        // reset hover time
        m_HoverTimeLeft = m_HoverTime;
    }

    private void OnDeath()
    {
        // restore normal gravity on death
        m_Ridgidbody.gravityScale = m_PlayerGravity;
    }
}
