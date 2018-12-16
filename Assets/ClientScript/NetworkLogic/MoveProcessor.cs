using Google.Protobuf;
using UnityEngine;

public class MoveProcessor : BaseProcessor {

    public override void Process(MessageSerializer serializer, IMessage msg)
    {
        //rspMove rsp = (rspMove)msg;
        //rsp.Error;

        //Debug.Log(rsp.Error);

        //do send message
        //Intent i = new Intent(BaseProcessor.MessageFilter);
        //i.putExtra(Intent.EXTRA_TEXT,rsp.getError());
        //MainActivity.Instance.sendBroadcast(i);
    }
}
