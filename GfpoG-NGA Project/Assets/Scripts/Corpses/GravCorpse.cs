using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravCorpse : MonoBehaviour
{
    [Range(-500, 500)] [SerializeField] private float m_CorpseForce = 20f;       // the jump force the fat corpse applies
    [Range(0, 20)] [SerializeField] private float m_Range = 5f;
    [SerializeField] private LayerMask m_EffektedLayers;

    private void FixedUpdate()
    {
        Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, m_Range, m_EffektedLayers);

        foreach (Collider2D collider in results)
        {
            Rigidbody2D t_RB = collider.attachedRigidbody;
            if (t_RB != null)
            {
                Vector2 deltaVec = t_RB.transform.position - transform.position;
                t_RB.AddForce(deltaVec.normalized * m_CorpseForce / deltaVec.magnitude);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

    }
}
