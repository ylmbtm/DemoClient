using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Protocol;
using ProtoBuf;

public class GLTeamCopy : GTSingleton<GLTeamCopy>
{
    public ulong m_uRoomID;
    public Msg_BroadRoomNotify m_RoomData;
    public Dictionary<int, int>       mGemIndexDict;

    public GLTeamCopy()
    {
       
    }

    public ulong GetRoomID()
    {
        return m_uRoomID;
    }

    public void AddListener()
    {
        NetworkManager.AddListener(MessageID.MSG_CREATE_ROOM_ACK, OnAck_CreateRoom);
        NetworkManager.AddListener(MessageID.MSG_LEAVE_ROOM_ACK, OnAck_LeaveRoom);
        NetworkManager.AddListener(MessageID.MSG_JOIN_ROOM_ACK, OnAck_JoinRoom);
        NetworkManager.AddListener(MessageID.MSG_START_ROOM_ACK, OnAck_StartRoom);
        NetworkManager.AddListener(MessageID.MSG_BROAD_ROOM_NOTIFY, OnNtf_BroadRoom);
    }

    public void TryCreateTeamRoom(uint copyID)
    {
        Msg_CreateRoomReq req = new Msg_CreateRoomReq();
        req.CopyID = copyID;
        req.RoleID = GTWorld.Main.GUID;
        NetworkManager.Instance.Send<Msg_CreateRoomReq>(MessageID.MSG_CREATE_ROOM_REQ, req);
    }
    public void TryStartTeamRoom(ulong roomID)
    {
        Msg_StartRoomReq req = new Msg_StartRoomReq();
        req.RoomID = roomID;
        req.RoleID = GTWorld.Main.GUID;
        NetworkManager.Instance.Send<Msg_StartRoomReq>(MessageID.MSG_START_ROOM_REQ, req);
    }

    public void TryLeaveTeamRoom(ulong roomID)
    {
        Msg_LeaveRoomReq req = new Msg_LeaveRoomReq();
        req.RoomID = roomID;
        req.RoleID = GTWorld.Main.GUID;
        NetworkManager.Instance.Send<Msg_LeaveRoomReq>(MessageID.MSG_LEAVE_ROOM_REQ, req);
    }

    public void TryJoinTeamRoom(ulong roomID)
    {
        Msg_JoinRoomReq req = new Msg_JoinRoomReq();
        req.RoomID = roomID;
        req.RoleID = GTWorld.Main.GUID;
        NetworkManager.Instance.Send<Msg_JoinRoomReq>(MessageID.MSG_JOIN_ROOM_REQ, req);
    }

    public void TryKickTeamRoom(ulong roomID, ulong targetID)
    {
        Msg_KickRoomReq req = new Msg_KickRoomReq();
        req.RoomID = roomID;
        req.RoleID = GTWorld.Main.GUID;
        NetworkManager.Instance.Send<Msg_KickRoomReq>(MessageID.MSG_KICK_ROOM_REQ, req);
    }

    private void OnAck_CreateRoom(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        Msg_CreateRoomAck ack = Serializer.Deserialize<Msg_CreateRoomAck>(ms);
        m_uRoomID = ack.RoomID;
    }

    private void OnAck_StartRoom(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        Msg_StartRoomAck ack = Serializer.Deserialize<Msg_StartRoomAck>(ms);
    }

    private void OnAck_JoinRoom(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        Msg_JoinRoomAck ack = Serializer.Deserialize<Msg_JoinRoomAck>(ms);
        m_uRoomID = ack.RoomID;
    }

    private void OnAck_LeaveRoom(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        Msg_LeaveRoomAck ack = Serializer.Deserialize<Msg_LeaveRoomAck>(ms);
    }

    private void OnAck_KickRoom(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        Msg_KickRoomAck ack = Serializer.Deserialize<Msg_KickRoomAck>(ms);
    }

    private void OnNtf_BroadRoom(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);

        m_RoomData = Serializer.Deserialize<Msg_BroadRoomNotify>(ms);

        GTEventCenter.FireEvent(GTEventID.TYPE_TEAM_UPDATE_VIEW);
    }

}
