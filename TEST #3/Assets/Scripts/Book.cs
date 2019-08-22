using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Book : MonoBehaviour
{
    public Player m_player;
    public CameraController m_playerCamera;
    public Camera m_bookCamera;
    public GameObject[] m_pages;
    public Transform m_pageTurnLeft;
    public Transform m_pageTurnRight;
    public Transform m_leftPages;
    public Transform m_rightPages;

    public Animation m_pageTurn;
    int m_currentPage;
    bool m_open;

    // Start is called before the first frame update
    void Start()
    {
        m_pageTurn.clip.legacy = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (m_open)
            {
                m_player.enabled = true;
                transform.GetChild(0).gameObject.SetActive(false);
                m_open = false;
                Cursor.lockState = CursorLockMode.Locked;

                m_bookCamera.enabled = false;
                m_playerCamera.m_camera.enabled = true;
                m_playerCamera.enabled = true;
            }
            else
            {
                m_player.enabled = false;
                transform.GetChild(0).gameObject.SetActive(true);
                OpenBook();
                m_open = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                m_bookCamera.enabled = true;
                m_playerCamera.m_camera.enabled = false;
                m_playerCamera.enabled = false;
            }
        }
        if (m_open)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (m_currentPage + 1 <= Mathf.CeilToInt(m_pages.Length / 2f) - 1)
                    StartCoroutine(SetPage(m_currentPage + 1, true));
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (m_currentPage - 1 >= 0)
                    StartCoroutine(SetPage(m_currentPage - 1, false));
            }
        }
    }

    void OpenBook()
    {
        m_currentPage = 0;
        m_pages[m_currentPage * 2].SetActive(true);
        m_pages[m_currentPage * 2 + 1].SetActive(true);
    }

    IEnumerator SetPage(int index, bool direction)
    {
        int oldPage = m_currentPage;
        m_currentPage = index;

        if (direction)
        {
            m_pages[oldPage * 2 + 1].transform.parent = m_pageTurnLeft;
            m_pages[oldPage * 2 + 1].transform.localPosition = Vector3.zero;
            m_pages[oldPage * 2 + 1].transform.localRotation = Quaternion.identity;

            m_pages[m_currentPage * 2].SetActive(true);

            if ((m_currentPage * 2 + 1) < m_pages.Length)
                m_pages[m_currentPage * 2 + 1].SetActive(true);

            m_pages[m_currentPage * 2].transform.parent = m_pageTurnRight;
            m_pages[m_currentPage * 2].transform.localPosition = Vector3.zero;
            m_pages[m_currentPage * 2].transform.localRotation = Quaternion.identity;

            m_pageTurn.Play(direction ? "BookFlip" : "BookFlipReverse");

            yield return WaitForAnimation(m_pageTurn);

            m_pages[oldPage * 2 + 1].transform.parent = m_rightPages;
            m_pages[oldPage * 2 + 1].transform.localPosition = Vector3.zero;
            m_pages[oldPage * 2 + 1].transform.localRotation = Quaternion.identity;

            m_pages[oldPage * 2 + 1].SetActive(false);
            m_pages[oldPage * 2].SetActive(false);

            m_pages[m_currentPage * 2].transform.parent = m_leftPages;
            m_pages[m_currentPage * 2].transform.localPosition = Vector3.zero;
            m_pages[m_currentPage * 2].transform.localRotation = Quaternion.identity;
        }
        else
        {
            m_pages[oldPage * 2].transform.parent = m_pageTurnRight;
            m_pages[oldPage * 2].transform.localPosition = Vector3.zero;
            m_pages[oldPage * 2].transform.localRotation = Quaternion.identity;

            m_pages[m_currentPage * 2].SetActive(true);
            m_pages[m_currentPage * 2 + 1].SetActive(true);

            m_pages[m_currentPage * 2 + 1].transform.parent = m_pageTurnLeft;
            m_pages[m_currentPage * 2 + 1].transform.localPosition = Vector3.zero;
            m_pages[m_currentPage * 2 + 1].transform.localRotation = Quaternion.identity;

            m_pageTurn.Play(direction ? "BookFlip" : "BookFlipReverse");

            yield return WaitForAnimation(m_pageTurn);
          
            m_pages[oldPage * 2].transform.parent = m_leftPages;
            m_pages[oldPage * 2].transform.localPosition = Vector3.zero;
            m_pages[oldPage * 2].transform.localRotation = Quaternion.identity;

            if ((oldPage * 2 + 1) < m_pages.Length)            
                m_pages[oldPage * 2 + 1].SetActive(false);         
            
            m_pages[oldPage * 2].SetActive(false);

            m_pages[m_currentPage * 2 + 1].transform.parent = m_rightPages;
            m_pages[m_currentPage * 2 + 1].transform.localPosition = Vector3.zero;
            m_pages[m_currentPage * 2 + 1].transform.localRotation = Quaternion.identity;
        }
    }

    private IEnumerator WaitForAnimation(Animation animation)
    {
        do
        {
            yield return null;
        } while (animation.isPlaying);
    }
}
