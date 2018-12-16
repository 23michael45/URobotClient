using DDRCommProto;

public class ClientMessageDispatcher :  BaseMessageDispatcher {


    public  ClientMessageDispatcher()
    {
        respLogin rsp = new respLogin();
        m_ProcessorMap.Add(rsp.GetType().ToString(), new SigninProcessor());

        
    }

}
