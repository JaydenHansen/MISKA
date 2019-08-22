using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    public int m_requiredTrash;
    public VoidEvent m_onAllTrash;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DepositTrash(Player player)
    {
        if (m_requiredTrash > 0)
        {
            if (m_requiredTrash >= player.TrashCount)
            {
                Debug.Log("Deposited " + player.TrashCount.ToString() + " trash");
                m_requiredTrash -= player.TrashCount;
                player.TrashCount = 0;
            }
            else
            {
                Debug.Log("Deposited " + m_requiredTrash.ToString() + " trash");
                player.TrashCount -= m_requiredTrash;
                m_requiredTrash = 0;
            }

            if (m_requiredTrash <= 0)
            {
                // do stuff
                Debug.Log("Collected all trash");
                m_onAllTrash.Invoke();
            }
        }
    }    
}
