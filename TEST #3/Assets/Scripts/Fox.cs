using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Fox : MonoBehaviour
{
    public Transform[] m_waypoints;

    NavMeshAgent m_agent;
    Animator m_animator;
    int m_currentWaypoint;

    // Start is called before the first frame update
    void Start()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_animator = GetComponent<Animator>();
        m_currentWaypoint = 0;
        m_agent.SetDestination(m_waypoints[m_currentWaypoint].position);
    }

    // Update is called once per frame
    void Update()
    {
        m_animator.SetFloat("Speed", m_agent.velocity.magnitude);
        if (m_agent.remainingDistance < 0.1f)
        {
            if (m_currentWaypoint + 1 < m_waypoints.Length)
            {
                m_currentWaypoint++;
                m_agent.SetDestination(m_waypoints[m_currentWaypoint].position);
            }
        }
    }
}
