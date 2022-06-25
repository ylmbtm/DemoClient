using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GTPreviewPlayer : MonoBehaviour
{
    private Animator            m_Animator;
    private CharacterController m_CC;
    private GTCameraFollow      m_Follow;
    private GameObject          m_FreeLook;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_CC = GetComponent<CharacterController>();
        GTInput.Instance.SetRoot(transform);
        CreateMainCamera(transform);
        Time.timeScale = 1;
    }

    private void CreateMainCamera(Transform trans)
    {
        if (m_FreeLook == null)
        {
            m_FreeLook = GTResourceManager.Instance.Load<GameObject>("Model/Other/FreeLook", true);
            Camera cam = m_FreeLook.GetComponentInChildren<Camera>();
            GTTools.SetTag(cam.gameObject, GTTools.Tags.MainCamera);
            m_FreeLook.transform.parent = transform;
            cam.fieldOfView = 60;
            cam.renderingPath = RenderingPath.Forward;
            cam.depth = 3;
        }
        this.m_Follow = m_FreeLook.GET<GTCameraFollow>();
        this.m_Follow.SetTarget(trans, 1.5f);
    }

    private void Update()
    {
        float move = 0;
        if (Input.GetKey(KeyCode.W))
        {
            move = 1;
            m_Animator.speed = 1;
            m_CC.SimpleMove(transform.forward * Time.deltaTime * 100);

        }
        if (Input.GetKey(KeyCode.S))
        {
            move = 1;
            m_Animator.speed = -1;
            m_CC.SimpleMove(-transform.forward * Time.deltaTime * 100);
        }
        //if (Input.GetKey(KeyCode.A))
        //{
        //    move = 1;
        //    transform.LookAt(transform.forward);
        //    m_CC.SimpleMove(transform.forward * Time.deltaTime * 200);
        //}
        //if (Input.GetKey(KeyCode.D))
        //{
        //    move = 1;
        //    transform.LookAt(transform.forward);
        //    m_CC.SimpleMove(transform.forward * Time.deltaTime * 200);

        //}


        m_Animator.SetFloat("Move", move);

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            m_Animator.SetTrigger("Combo01");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            m_Animator.SetTrigger("Combo02");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            m_Animator.SetTrigger("Combo03");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            m_Animator.SetTrigger("Combo04");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            m_Animator.SetBool("DoStun", true);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            m_Animator.SetBool("DoStun", false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            m_Animator.SetTrigger("DoKnockDown");
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
   
        }
    }

}
