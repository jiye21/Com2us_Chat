using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

public class LobbyManager_TCP : MonoBehaviour
{
    enum PACKET_TYPE
    {
        LOGIN,
        CHAT,
        ROOM_ID,
        GET_ID,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class BasePacket
    {
        public ushort packet_len;
        public ushort packet_id;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class Room_IDPacket : BasePacket
    {
        public int roomID;

        public Room_IDPacket()
        {
            roomID = 0;
            packet_id = (ushort)PACKET_TYPE.ROOM_ID;
            packet_len = (ushort)Marshal.SizeOf(typeof(Room_IDPacket));
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class Get_IDPacket : BasePacket
    {
        public int id;

        public Get_IDPacket()
        {
            id = 0;
            packet_id = (ushort)PACKET_TYPE.GET_ID;
            packet_len = (ushort)Marshal.SizeOf(typeof(Get_IDPacket));
        }
    };


    [SerializeField]
    string ipAddr = "127.0.0.1";
    [SerializeField]
    int port = 9999;
    //[SerializeField]
    //GameObject playerObj;

    //Dictionary<int, GameObject> playerList = new Dictionary<int, GameObject>();

    TcpClient tcpClient;

    Queue<string> msgQueue;
    Queue<BasePacket> Queue;

    private void Start()
    {
        Debug.Log(Marshal.SizeOf(typeof(Room_IDPacket)));


        //  메시지 큐
        msgQueue = new Queue<string>();
        Queue = new Queue<BasePacket>();

        ConnectTCP();
    }

    //float tick = 0;
    //int myId = 0;

    private void Update()
    {
        if (msgQueue.Count > 0)
        {
            var msg = msgQueue.Dequeue();
        }

        if (Queue.Count > 0)
        {
            var basePack = Queue.Dequeue();

            switch ((PACKET_TYPE)basePack.packet_id)
            {
                case PACKET_TYPE.ROOM_ID:
                    {
                        //var pack = (Room_IDPacket)basePack;
                        //playerObj.name = "player" + pack.roomID;
                        //myId = pack.id;

                        //playerList.Add(pack.id, playerObj);
                    }
                    break;
                                
            }
        }

        {
            Room_IDPacket packet = new Room_IDPacket();
            packet.roomID = 0;

            SendCall(packet);
        }

        //tick -= Time.deltaTime;
        /*
        if (tick <= 0)
        {
            PosPacket packet = new PosPacket();
            packet.id = myId;
            packet.pos = playerObj.transform.position;
            packet.rot = playerObj.transform.rotation;

            SendCall(packet);

            tick = 0.1f;
        }*/
    }
    /// <summary>
    /// ////////////////////////////////////////////////////////////
    /// </summary>
    private void requestCall(System.IAsyncResult ar)
    {
        tcpClient.EndConnect(ar);

        SendCall(new Room_IDPacket());

        byte[] buf = new byte[512];
        tcpClient.GetStream().BeginRead(buf, 0, buf.Length, requestCallTCP, buf);
    }

    Queue<byte> streamBuffer = new Queue<byte>();

    private void requestCallTCP(System.IAsyncResult ar)
    {
        try
        {
            var byteRead = tcpClient.GetStream().EndRead(ar);

            if (byteRead > 0)
            {
                byte[] data = (byte[])ar.AsyncState;

                //  버퍼에 추가
                for (int i = 0; i < byteRead; i++)
                {
                    streamBuffer.Enqueue(data[i]);
                }

                //  버퍼가 베이스보다 커지면
                if (streamBuffer.Count > Marshal.SizeOf(typeof(BasePacket)))
                {
                    //  베이스로 변환해봄( 픽 )
                    var basePack = ByteToObject<BasePacket>(streamBuffer.ToArray());
                    //var basePack2 = ByteToObject<BasePacket>(data);

                    //  버퍼크기가 패킷만큼 왔으면~
                    if (streamBuffer.Count >= basePack.packet_len)
                    {
                        switch ((PACKET_TYPE)basePack.packet_id)
                        {
                            case PACKET_TYPE.ROOM_ID:
                                var pack = ByteToObject<Room_IDPacket>(data);
                                Queue.Enqueue(pack);
                                break;

                        }

                        //  사용한 패킷 제거
                        for (int i = 0; i < basePack.packet_len; i++)
                        {
                            streamBuffer.Dequeue();
                        }
                    }
                }

                tcpClient.GetStream().BeginRead(data, 0, data.Length, requestCallTCP, data);
            }
            else
            {
                tcpClient.GetStream().Close();
                tcpClient.Close();
                tcpClient = null;
            }
        }
        catch (SocketException e)
        {
            Debug.LogException(e);
        }
    }


    public void ConnectTCP()
    {
        if (tcpClient != null)
            return;

        tcpClient = new TcpClient();
        tcpClient.BeginConnect(ipAddr, port, requestCall, null);
    }
    public void SendCall(byte[] _data)
    {
        tcpClient.GetStream().Write(_data);
    }
    public void SendCall<T>(T _obj)
    {
        if (tcpClient.Connected)
            tcpClient.GetStream().Write(ObjectToByte(_obj));
    }

    void OnDestroy()
    {
        if (tcpClient != null)
        {
            tcpClient.GetStream().Close();
            tcpClient.Close();
        }

    }


    public static T ByteToObject<T>(byte[] buffer)
    {
        T structure;

        int size = Marshal.SizeOf(typeof(T));

        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(buffer, 0, ptr, size);
            structure = Marshal.PtrToStructure<T>(ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return structure;
    }

    public static byte[] ObjectToByte<T>(T structure)
    {
        int size = Marshal.SizeOf(structure);
        byte[] byteArray = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(structure, ptr, false);
            Marshal.Copy(ptr, byteArray, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return byteArray;
    }
}
