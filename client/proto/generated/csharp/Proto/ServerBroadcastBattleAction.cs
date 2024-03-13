// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Proto
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct ServerBroadcastBattleAction : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static ServerBroadcastBattleAction GetRootAsServerBroadcastBattleAction(ByteBuffer _bb) { return GetRootAsServerBroadcastBattleAction(_bb, new ServerBroadcastBattleAction()); }
  public static ServerBroadcastBattleAction GetRootAsServerBroadcastBattleAction(ByteBuffer _bb, ServerBroadcastBattleAction obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public ServerBroadcastBattleAction __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public ulong ServerTime { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetUlong(o + __p.bb_pos) : (ulong)0; } }
  public Proto.BattleAction? Actions(int j) { int o = __p.__offset(6); return o != 0 ? (Proto.BattleAction?)(new Proto.BattleAction()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int ActionsLength { get { int o = __p.__offset(6); return o != 0 ? __p.__vector_len(o) : 0; } }

  public static Offset<Proto.ServerBroadcastBattleAction> CreateServerBroadcastBattleAction(FlatBufferBuilder builder,
      ulong server_time = 0,
      VectorOffset actionsOffset = default(VectorOffset)) {
    builder.StartTable(2);
    ServerBroadcastBattleAction.AddServerTime(builder, server_time);
    ServerBroadcastBattleAction.AddActions(builder, actionsOffset);
    return ServerBroadcastBattleAction.EndServerBroadcastBattleAction(builder);
  }

  public static void StartServerBroadcastBattleAction(FlatBufferBuilder builder) { builder.StartTable(2); }
  public static void AddServerTime(FlatBufferBuilder builder, ulong serverTime) { builder.AddUlong(0, serverTime, 0); }
  public static void AddActions(FlatBufferBuilder builder, VectorOffset actionsOffset) { builder.AddOffset(1, actionsOffset.Value, 0); }
  public static VectorOffset CreateActionsVector(FlatBufferBuilder builder, Offset<Proto.BattleAction>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static VectorOffset CreateActionsVectorBlock(FlatBufferBuilder builder, Offset<Proto.BattleAction>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateActionsVectorBlock(FlatBufferBuilder builder, ArraySegment<Offset<Proto.BattleAction>> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateActionsVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<Offset<Proto.BattleAction>>(dataPtr, sizeInBytes); return builder.EndVector(); }
  public static void StartActionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<Proto.ServerBroadcastBattleAction> EndServerBroadcastBattleAction(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<Proto.ServerBroadcastBattleAction>(o);
  }
  public ServerBroadcastBattleActionT UnPack() {
    var _o = new ServerBroadcastBattleActionT();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(ServerBroadcastBattleActionT _o) {
    _o.ServerTime = this.ServerTime;
    _o.Actions = new List<Proto.BattleActionT>();
    for (var _j = 0; _j < this.ActionsLength; ++_j) {_o.Actions.Add(this.Actions(_j).HasValue ? this.Actions(_j).Value.UnPack() : null);}
  }
  public static Offset<Proto.ServerBroadcastBattleAction> Pack(FlatBufferBuilder builder, ServerBroadcastBattleActionT _o) {
    if (_o == null) return default(Offset<Proto.ServerBroadcastBattleAction>);
    var _actions = default(VectorOffset);
    if (_o.Actions != null) {
      var __actions = new Offset<Proto.BattleAction>[_o.Actions.Count];
      for (var _j = 0; _j < __actions.Length; ++_j) { __actions[_j] = Proto.BattleAction.Pack(builder, _o.Actions[_j]); }
      _actions = CreateActionsVector(builder, __actions);
    }
    return CreateServerBroadcastBattleAction(
      builder,
      _o.ServerTime,
      _actions);
  }
}

public class ServerBroadcastBattleActionT
{
  public ulong ServerTime { get; set; }
  public List<Proto.BattleActionT> Actions { get; set; }

  public ServerBroadcastBattleActionT() {
    this.ServerTime = 0;
    this.Actions = null;
  }
}


static public class ServerBroadcastBattleActionVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*ServerTime*/, 8 /*ulong*/, 8, false)
      && verifier.VerifyVectorOfTables(tablePos, 6 /*Actions*/, Proto.BattleActionVerify.Verify, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}
