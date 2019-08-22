﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    protected Transform m_target;
    protected Rigidbody m_rigidbody;
    protected Collider m_collider;
    protected float m_baseDrag;
    protected float m_baseAngularDrag;

    public Transform Target
    {
        get { return m_target; }
    }
    public Rigidbody Rigidbody
    {
        get { return m_rigidbody; }
    }
    public Collider Collider
    {
        get { return m_collider; }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_collider = GetComponent<Collider>();
        m_baseDrag = m_rigidbody.drag;
        m_baseAngularDrag = m_rigidbody.angularDrag;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_target != null)
        {
            m_rigidbody.drag = 10 - Mathf.Clamp((transform.position - m_target.position).magnitude, 0, 10);
            m_rigidbody.AddForce((m_target.position - transform.position) * 100);
        }
    }

    public void StartPickup(Transform target)
    {
        m_target = target;
        m_rigidbody.useGravity = false;
        m_rigidbody.angularDrag = 5f;
    }
    public void EndPickup()
    {
        m_target = null;
        m_rigidbody.drag = m_baseDrag;
        m_rigidbody.useGravity = true;
        m_rigidbody.angularDrag = m_baseAngularDrag;
    }
}
