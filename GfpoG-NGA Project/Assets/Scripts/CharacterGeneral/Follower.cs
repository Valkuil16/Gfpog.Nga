using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    [Range(0, 1)] [SerializeField] private float m_TorqueTime = 0.5f;
    [Range(-30, 30)] [SerializeField] private float m_JumpTorque = 10f;
    [Range(-1000, 1000)] [SerializeField] private float m_RagdollTorque = 500f;

    public GameObject parent;
    private Transform m_ParentTransform;

    private bool m_AddTorque = false;                     // Do we want to spin
    private float m_TorqueThresh = 2.5f;
    private Rigidbody2D m_Rigidbody2D;
    private Rigidbody2D m_ParentBody;
    private float m_TorqueEndTime;

    // Start is called before the first frame update
    void Start()
    {
        m_ParentTransform = parent.GetComponent<Transform>();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_ParentBody = parent.GetComponent<Rigidbody2D>();

        CharacterController2D.OnJumpEvent.AddListener(delegate { OnJump(); });
        CharacterController2D.OnDeathEvent.AddListener(delegate { OnRagdoll(); });
    }

    private void Update()
    {
        transform.position = m_ParentTransform.position;
    }

    private void FixedUpdate()
    {
        if (m_AddTorque && Time.timeSinceLevelLoad < m_TorqueEndTime)
        {
            //Adds Torque To The SpriteFollower
            if (Mathf.Abs(m_ParentBody.velocity.x) > m_TorqueThresh)
            {
                int Multi = 1;
                if (m_ParentBody.velocity.x > 0)
                {
                    Multi = -1;
                }
                m_Rigidbody2D.AddTorque(m_JumpTorque * Multi);
            }
        }
    }

    private void OnJump()
    {
        m_AddTorque = true;
        m_TorqueEndTime = Time.timeSinceLevelLoad + m_TorqueTime;
    }

    private void OnRagdoll()
    {
        m_Rigidbody2D.AddTorque(m_RagdollTorque);
    }
}
