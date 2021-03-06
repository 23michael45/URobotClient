﻿using DDRCommProto;
using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerInformationProcessor : BaseProcessor
{

    public override void Process(MessageSerializer serializer, IMessage msg)
    {
        bcLSAddr bcmsg = (bcLSAddr)msg;

        string s = "";
        foreach(var ip in bcmsg.LSInfo.Ips)
        {
            s += " : "  + ip;
            if(ip.Contains("192.168.1"))
            {
                MainUILogic.mInstance.m_TcpIP = ip;
                MainUILogic.mInstance.m_TcpPort = bcmsg.LSInfo.Port;
            }
        }
        Debug.Log(bcmsg.LSInfo.Name + s);
        //bcmsg.ServerName ="ClientName 127";
        //AsyncUdpClient.getInstance().SendTo("127.0.0.1", bcmsg);
        //bcmsg.ServerName = "ClientName 183";
        //AsyncUdpClient.getInstance().SendTo("192.168.1.183", bcmsg);

    }

}