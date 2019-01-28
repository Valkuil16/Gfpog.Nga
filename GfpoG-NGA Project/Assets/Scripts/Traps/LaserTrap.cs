using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTrap : MonoBehaviour
{
    [Range(0, 5000)] [SerializeField] private int m_ShootDelayMS = 1000;
    [Range(0, 5000)] [SerializeField] private int m_ShootTimeMS = 2000;  // the speed of the projectile
    [Range(0, 10000)] [SerializeField] private int m_ShootStarTOffsetMS = 1000;  // the speed of the projectile
    [SerializeField] private LineRenderer m_LineRenderer;
    [SerializeField] private ParticleSystem[] m_ImpactSystems;
    [SerializeField] private ParticleSystem m_LaserParticles;


    private float m_NextTime;   // the time for the next change
    private bool m_Shooting = false;
    private LayerMask m_Ignore;
    private float m_OrigEmmisionRate;    // the set emmision rate;
    private Vector2 m_LastHitPos = Vector2.positiveInfinity;   // stored to save the last hit position so that there is no need to update if nothing changes (+infinity if nothing is hit)

    private void Start()
    {
        m_NextTime = Time.timeSinceLevelLoad + m_ShootStarTOffsetMS / 1000;
        m_OrigEmmisionRate = m_LaserParticles.emission.rateOverTime.constant;
    }   

    private void FixedUpdate()
    {
        // if over time switch
        if (Time.timeSinceLevelLoad > m_NextTime)
        {
            m_Shooting = !m_Shooting;
            if (m_Shooting)
            {
                m_NextTime = Time.timeSinceLevelLoad + m_ShootTimeMS / 1000;
                OnShootStart();
            } else
            {
                m_NextTime = Time.timeSinceLevelLoad + m_ShootDelayMS / 1000;
                OnShootEnd();
            }
        }

        if (m_Shooting)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Vector2 firePos = transform.position; // - transform.up;

        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position - transform.up, -transform.up, 1000f);//, ~m_Ignore);

        if (hitInfo)
        {
            CharacterController2D player = hitInfo.transform.GetComponent<CharacterController2D>();

            if (player != null)
                player.Kill();

            
            OnHIt(firePos, hitInfo.point, hitInfo.normal);
        } else
        {
            OnMiss(firePos);
        }
    }

    private void OnHIt(Vector2 firePos, Vector2 hitPos, Vector2 hitNormal)
    {
        if (m_LastHitPos != hitPos)
        {
            m_LineRenderer.SetPosition(0, firePos);
            m_LineRenderer.SetPosition(1, hitPos);

            float distance = (firePos - hitPos).magnitude;

            // scale particle emitter
            var sh = m_LaserParticles.shape;    // get reference to shape struct
            sh.scale = new Vector3(distance / 2, 1f, 1f);
            sh.position = new Vector2(0f, -distance / 2);

            // start emitting particles and set impact position
            foreach (ParticleSystem impactSystem in m_ImpactSystems)
            {
                impactSystem.Play();
                // move to position and rotate to surface normal
                impactSystem.transform.SetPositionAndRotation(hitPos, Quaternion.LookRotation(Vector3.back, hitNormal));
            }

            var emission = m_LaserParticles.emission;
            emission.rateOverTime = distance * m_OrigEmmisionRate;
        }
        m_LastHitPos = hitPos;
    }

    private void OnMiss(Vector2 firePos)
    {
        if (m_LastHitPos != Vector2.positiveInfinity)
        {
            m_LineRenderer.SetPosition(0, firePos);
            m_LineRenderer.SetPosition(1, firePos + (Vector2)transform.up * -100);

            var emission = m_LaserParticles.emission;
            emission.rateOverTime = 100f * m_OrigEmmisionRate;

            // stop impact systems from emitting particles
            foreach (ParticleSystem impactSystem in m_ImpactSystems)
            {
                impactSystem.Stop();
            }
        }
        m_LastHitPos = Vector2.positiveInfinity;
    }

    private void OnShootStart()
    {
        m_LineRenderer.enabled = true;
        m_LaserParticles.Play();
    }

    private void OnShootEnd() {
        m_LineRenderer.enabled = false;
        m_LaserParticles.Stop();
        OnMiss(Vector2.zero);
    }
}
