using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Book : MonoBehaviour
{
    public Player m_player;
    public CameraController m_playerCamera;
    public Camera m_bookCamera;
    public GameObject m_pageMesh;
    public Transform m_pageTurnLeft;
    public Transform m_pageTurnRight;
    public Transform m_leftPages;
    public Transform m_rightPages;
    public GameObject m_nextPageButton;
    public GameObject m_prevPageButton;

    public Animation m_pageTurn;

    List<GameObject> m_pages;
    int m_currentPage;
    bool m_open;
    Vector3 m_baseCameraPos;
    bool m_zoomed = false;

    // Start is called before the first frame update
    void Start()
    {
        //m_pageTurn.clip.legacy = true;
        m_baseCameraPos = m_bookCamera.transform.localPosition;

        m_pages = new List<GameObject>();
        for(int i = 0; i < m_leftPages.childCount; i++)
        {
            m_pages.Add(m_leftPages.GetChild(i).gameObject);
            if (i < m_rightPages.childCount)
                m_pages.Add(m_rightPages.GetChild(i).gameObject);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (m_open && !m_pageTurn.isPlaying)
            {
                m_player.enabled = true;
                transform.GetChild(0).gameObject.SetActive(false);
                foreach (GameObject page in m_pages)
                    page.SetActive(false);
                m_nextPageButton.SetActive(false);
                m_prevPageButton.SetActive(false);
                m_open = false;
                Cursor.lockState = CursorLockMode.Locked;

                m_bookCamera.enabled = false;
                m_playerCamera.m_camera.enabled = true;
                m_playerCamera.enabled = true;
            }
            else if (!m_open)
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
            if (Input.GetKeyDown(KeyCode.RightArrow) && !m_pageTurn.isPlaying)
            {
                if (m_currentPage + 1 <= Mathf.CeilToInt(m_pages.Count / 2f) - 1)
                    StartCoroutine(SetPage(m_currentPage + 1, true));
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) && !m_pageTurn.isPlaying)
            {
                if (m_currentPage - 1 >= 0)
                    StartCoroutine(SetPage(m_currentPage - 1, false));
            }
            if (Input.GetKey(KeyCode.Mouse1))
            {                
                int width = Screen.width;
                int height = Screen.height;
                int x = (int)Input.mousePosition.x;
                int y = (int)Input.mousePosition.y;

                float imageAspectRatio = width / (float)height; // assuming width > height 
                float Px = (2 * (x / (float)width) - 1) * Mathf.Tan(m_bookCamera.fieldOfView / 2f * Mathf.PI / 180f) * imageAspectRatio;
                float Py = (2 * (y / (float)height) - 1) * Mathf.Tan(m_bookCamera.fieldOfView / 2f * Mathf.PI / 180f);

                Vector3 direction = new Vector3(Px, Py, 1);
                direction = m_bookCamera.transform.rotation * direction;

                Ray ray = new Ray(m_baseCameraPos * 0.1f + transform.position, direction);// * 0.1 cause of scale

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1, 1 - LayerMask.NameToLayer("Book"), QueryTriggerInteraction.Collide)) 
                {
                    m_bookCamera.transform.position = hit.point + (hit.normal * 0.1f);
                }
                else
                {
                    m_bookCamera.transform.localPosition = m_baseCameraPos;
                }
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                m_bookCamera.transform.localPosition = m_baseCameraPos;
            }
        }
    }

    void OpenBook()
    {
        m_currentPage = 0;
        m_leftPages.gameObject.SetActive(true);
        m_rightPages.gameObject.SetActive(true);
        m_nextPageButton.SetActive(true);
        m_prevPageButton.SetActive(true);
        m_pages[m_currentPage * 2].SetActive(true);
        m_pages[m_currentPage * 2 + 1].SetActive(true);
    }

    public void LeftZoom()
    {
        if (!m_pageTurn.isPlaying)
        {
            if (!m_zoomed)
            {
                m_pageTurn.Play("Book_Zoom_001");
                m_zoomed = true;
                m_nextPageButton.SetActive(false);
                m_prevPageButton.SetActive(false);
            }
            else
            {
                m_pageTurn.Play("Book_Zoom_Reverse_001");
                m_zoomed = false;
                m_nextPageButton.SetActive(true);
                m_prevPageButton.SetActive(true);
            }
        }
    }

    public void NextPage()
    {
        if (m_currentPage + 1 <= Mathf.CeilToInt(m_pages.Count / 2f) - 1)
            StartCoroutine(SetPage(m_currentPage + 1, true));
    }

    public void PrevPage()
    {
        if (m_currentPage - 1 >= 0)
            StartCoroutine(SetPage(m_currentPage - 1, false));
    }

    IEnumerator SetPage(int index, bool direction)
    {
        int oldPage = m_currentPage;
        m_currentPage = index;

        m_nextPageButton.SetActive(false);
        m_prevPageButton.SetActive(false);

        m_pageMesh.SetActive(true);

        if (direction)
        {
            m_pages[oldPage * 2 + 1].transform.parent = m_pageTurnLeft;
            m_pages[oldPage * 2 + 1].transform.localPosition = Vector3.zero;
            m_pages[oldPage * 2 + 1].transform.localRotation = Quaternion.identity;

            m_pages[m_currentPage * 2].SetActive(true);

            if ((m_currentPage * 2 + 1) < m_pages.Count)
                m_pages[m_currentPage * 2 + 1].SetActive(true);

            m_pages[m_currentPage * 2].transform.parent = m_pageTurnRight;
            m_pages[m_currentPage * 2].transform.localPosition = Vector3.zero;
            m_pages[m_currentPage * 2].transform.localRotation = Quaternion.identity;

            m_pageTurn.Play("Book_Flip_001");

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

            m_pageTurn.Play("Book_Flip_Reverse_001");

            yield return WaitForAnimation(m_pageTurn);
          
            m_pages[oldPage * 2].transform.parent = m_leftPages;
            m_pages[oldPage * 2].transform.localPosition = Vector3.zero;
            m_pages[oldPage * 2].transform.localRotation = Quaternion.identity;

            if ((oldPage * 2 + 1) < m_pages.Count)            
                m_pages[oldPage * 2 + 1].SetActive(false);         
            
            m_pages[oldPage * 2].SetActive(false);

            m_pages[m_currentPage * 2 + 1].transform.parent = m_rightPages;
            m_pages[m_currentPage * 2 + 1].transform.localPosition = Vector3.zero;
            m_pages[m_currentPage * 2 + 1].transform.localRotation = Quaternion.identity;
        }

        m_pageMesh.SetActive(false);

        m_nextPageButton.SetActive(true);
        m_prevPageButton.SetActive(true);
    }

    private IEnumerator WaitForAnimation(Animation animation)
    {
        do
        {
            yield return null;
        } while (animation.isPlaying);
    }
}
