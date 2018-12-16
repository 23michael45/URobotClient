using DDRCommLib;
using DDRCommProto;
using Google.Protobuf;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

public class MessageSerializer {

    public  MessageSerializer(BaseSocketConnection bc,BaseMessageDispatcher dispather)
    {
        m_MessageDispatcher = dispather;
        m_BaseSocketConnection = bc;
    }


    private  BaseSocketConnection m_BaseSocketConnection;
    private  BaseMessageDispatcher m_MessageDispatcher = null;

    private static string HeadString = "pbh\0";

    public  static string SharpClass2ProtoTypeName(string className)
    {
        string stype = className.Replace("class BaseCmd.Cmd\\$","BaseCmd.");
        return stype;
    }
    public  static  string ProtoTypeName2SharpClassName(string typeName)
    {
        string className = typeName.Replace("BaseCmd\\.","class BaseCmd.Cmd\\$");
        return className;
    }





    public void ProcessReceive(byte[] buf)
    {
        var msg = Parse(buf);



        //do dispatch
        m_MessageDispatcher.Dispatcher(this, msg.GetType().ToString(), (IMessage)msg);
    }
    public void ProcessSend(byte[] buf)
    {

    }

    public static IMessage Parse(byte[] buf)
    {

        MemoryStream ms = new MemoryStream(buf);
        

        byte[] head = new byte[4];
        ms.Read(head, 0, 4);

        string shead = System.Text.Encoding.ASCII.GetString(head);

        if (shead == HeadString)
        {
            byte[] btotallen = new byte[4];
            ms.Read(btotallen, 0, 4);
            int totallen = bytesToIntLittle(btotallen, 0);

            byte[] bheadlen = new byte[4];
            ms.Read(bheadlen, 0, 4);
            int headlen = bytesToIntLittle(bheadlen, 0);


            byte[] bheaddata = new byte[headlen];
            byte[] bbodydata = new byte[totallen - headlen - 8];


            ms.Read(bheaddata,0,bheaddata.Length);
            ms.Read(bbodydata, 0, bbodydata.Length);

            CommonHeader headdata = null;
            IMessage bodydatamsg = null;
            bool needEncrypt = true;
            if (needEncrypt)
            {



                byte[] bheaddataDE = new byte[bheaddata.Length - 5];
                if (Encrypt.Txt_Decrypt(bheaddata, bheaddata.Length, bheaddataDE, bheaddataDE.Length))
                {

                }
                else
                {
                    Debug.LogError("Txt_Decrypt Error");

                }

                headdata = CommonHeader.Parser.ParseFrom(bheaddataDE);

                if (bbodydata.Length > 5)
                {
                    byte[] bbodydataDE = new byte[bbodydata.Length - 5];
                    if (Encrypt.Txt_Decrypt(bbodydata, bbodydata.Length, bbodydataDE, bbodydataDE.Length))
                    {

                        bodydatamsg = parseDynamic(headdata.BodyType, bbodydataDE);
                    }
                    else
                    {
                        Debug.LogError("Txt_Decrypt Error");

                    }

                }
                else
                {

                    bodydatamsg = parseDynamic(headdata.BodyType, null);
                }
            }
            else
            {

                headdata = CommonHeader.Parser.ParseFrom(bheaddata);
                bodydatamsg = parseDynamic(headdata.BodyType, bbodydata);
            }


            return bodydatamsg;
        }
        else
        {

        }
        return null;
    }


    /**
     * 发送数据
     *
     * @param data 需要发送的内容
     */
    public static MemoryStream Serialize<T>(T msg) where T : IMessage
    {
        

        byte[] bbody = msg.ToByteArray();
        string stype = msg.GetType().ToString();
        stype = SharpClass2ProtoTypeName(stype);
        int bodylen = bbody.Length;



        CommonHeader headdata = new CommonHeader();
        headdata.BodyType = stype;

        byte[] bhead = headdata.ToByteArray();
        int headlen = bhead.Length;


        int totallen = 8 + headlen + bodylen;


        byte[] bshead = Encoding.ASCII.GetBytes(HeadString);

        MemoryStream ms = new MemoryStream();
        ms.Write(bshead, 0, 4);



        bool needEncrypt = true;
        if (needEncrypt)
        {

            ms.Write(intToBytesLittle(totallen + 10), 0, 4);
            ms.Write(intToBytesLittle(headlen + 5), 0, 4);

            byte[] bheadE = new byte[bhead.Length + 5];
            if (Encrypt.Txt_Encrypt(bhead, bhead.Length, bheadE, bheadE.Length))
            {

                ms.Write(bheadE, 0, bheadE.Length);
            }
            else
            {
                Debug.LogError("Txt_Encrypt Error");

            }

            if(bbody.Length > 0)
            {
                byte[] bbodyE = new byte[bbody.Length + 5];
                if (Encrypt.Txt_Encrypt(bbody, bbody.Length, bbodyE, bbodyE.Length))
                {

                    ms.Write(bbodyE, 0, bbodyE.Length);
                }
                else
                {
                    Debug.LogError("Txt_Encrypt Error");
                }

            }


        }
        else
        {
            ms.Write(intToBytesLittle(totallen), 0, 4);
            ms.Write(intToBytesLittle(headlen), 0, 4);
            ms.Write(bhead, 0, bhead.Length);
            ms.Write(bbody, 0, bbody.Length);
        }

        return ms;
    }




    public static IMessage parseDynamic(string stype, byte[] bytes) {
        try {

            //type = ProtoTypeName2JavaClassName(type);

            //type = type.replace("class ", "");

            Debug.Log("Tcp Connect　parseDynamic:");


            string assemblyName = typeof(CommonHeader).Assembly.ToString();
            string assemblyQualifiedName = Assembly.CreateQualifiedName(assemblyName, stype);
            Type type = Type.GetType(assemblyQualifiedName);


            //object obj = Activator.CreateInstance(type);



            MethodInfo[] methods = type.GetMethods();


            object[] arguments = new object[2];
            arguments[0] = bytes;
            arguments[1] = bytes.Length;

            var parseFromMethod = Array.Find(methods,m => m.Name == "get_Parser");

            MessageParser parser = parseFromMethod.Invoke(null,null) as MessageParser;
            System.Object obj = parser.ParseFrom(bytes,0,bytes.Length);


            //Assembly assembly = Assembly.GetExecutingAssembly();
            //IMessage obj = assembly.CreateInstance(stype) as IMessage; 
            return (IMessage)obj;
        }
        catch (Exception e)
        {
        }
        return null;
    }


    /**
     * 以大端模式将int转成byte[]
     */
    public static byte[] intToBytesBig(int value) {
        byte[] src = new byte[4];
        src[0] = (byte) ((value >> 24) & 0xFF);
        src[1] = (byte) ((value >> 16) & 0xFF);
        src[2] = (byte) ((value >> 8) & 0xFF);
        src[3] = (byte) (value & 0xFF);
        return src;
    }

    /**
     * 以小端模式将int转成byte[]
     *
     * @param value
     * @return
     */
    public static byte[] intToBytesLittle(int value) {
        byte[] src = new byte[4];
        src[3] = (byte) ((value >> 24) & 0xFF);
        src[2] = (byte)((value >> 16) & 0xFF);
        src[1] = (byte) ((value >> 8) & 0xFF);
        src[0] = (byte) (value & 0xFF);
        return src;
    }

    /**
     * 以大端模式将byte[]转成int
     */
    public static int bytesToIntBig(byte[] src, int offset) {
        int value;
        value = (int) (((src[offset] & 0xFF) << 24)
                | ((src[offset + 1] & 0xFF) << 16)
                | ((src[offset + 2] & 0xFF) << 8)
                | (src[offset + 3] & 0xFF));
        return value;
    }

    /**
     * 以小端模式将byte[]转成int
     */
    public static int bytesToIntLittle(byte[] src, int offset) {
        int value;
        value = (int) ((src[offset] & 0xFF)
                | ((src[offset + 1] & 0xFF) << 8)
                | ((src[offset + 2] & 0xFF) << 16)
                | ((src[offset + 3] & 0xFF) << 24));
        return value;
    }
    
}
