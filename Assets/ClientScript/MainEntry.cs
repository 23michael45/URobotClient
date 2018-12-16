using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Google.Protobuf;
using System.Threading;
using System.Threading.Tasks;
using System;
using DDRCommProto;
using UnityEngine.UI;

public class MainEntry : MonoBehaviour
{
    public Button m_ConnectBtn;
    public Button m_CloseBtn;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    public void OnClose()
    {

    }

    public void OnSendReqSignin()
    {

    }
    public void OnStartHeartBeat()
    {

    }
    public void OnStopHeartBeat()
    {
    }


    public void OnSendReqMove()
    {
        //reqMove req = new reqMove();
        //AsyncTcpClient.getInstance().Send(req);
    }
    
}
