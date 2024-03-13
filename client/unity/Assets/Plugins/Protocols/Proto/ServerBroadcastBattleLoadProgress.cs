// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Proto
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct ServerBroadcastBattleLoadProgress : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static ServerBroadcastBattleLoadProgress GetRootAsServerBroadcastBattleLoadProgress(ByteBuffer _bb) { return GetRootAsServerBroadcastBattleLoadProgress(_bb, new ServerBroadcastBattleLoadProgress()); }
  public static ServerBroadcastBattleLoadProgress GetRootAsServerBroadcastBattleLoadProgress(ByteBuffer _bb, ServerBroadcastBattleLoadProgress obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public ServerBroadcastBattleLoadProgress __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public ulong PlayerId { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetUlong(o + __p.bb_pos) : (ulong)0; } }
  public int Progress { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }

  public static Offset<Proto.ServerBroadcastBattleLoadProgress> CreateServerBroadcastBattleLoadProgress(FlatBufferBuilder builder,
      ulong player_id = 0,
      int progress = 0) {
    builder.StartTable(2);
    ServerBroadcastBattleLoadProgress.AddPlayerId(builder, player_id);
    ServerBroadcastBattleLoadProgress.AddProgress(builder, progress);
    return ServerBroadcastBattleLoadProgress.EndServerBroadcastBattleLoadProgress(builder);
  }

  public static void StartServerBroadcastBattleLoadProgress(FlatBufferBuilder builder) { builder.StartTable(2); }
  public static void AddPlayerId(FlatBufferBuilder builder, ulong playerId) { builder.AddUlong(0, playerId, 0); }
  public static void AddProgress(FlatBufferBuilder builder, int progress) { builder.AddInt(1, progress, 0); }
  public static Offset<Proto.ServerBroadcastBattleLoadProgress> EndServerBroadcastBattleLoadProgress(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<Proto.ServerBroadcastBattleLoadProgress>(o);
  }
  public ServerBroadcastBattleLoadProgressT UnPack() {
    var _o = new ServerBroadcastBattleLoadProgressT();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(ServerBroadcastBattleLoadProgressT _o) {
    _o.PlayerId = this.PlayerId;
    _o.Progress = this.Progress;
  }
  public static Offset<Proto.ServerBroadcastBattleLoadProgress> Pack(FlatBufferBuilder builder, ServerBroadcastBattleLoadProgressT _o) {
    if (_o == null) return default(Offset<Proto.ServerBroadcastBattleLoadProgress>);
    return CreateServerBroadcastBattleLoadProgress(
      builder,
      _o.PlayerId,
      _o.Progress);
  }
}

public class ServerBroadcastBattleLoadProgressT
{
  public ulong PlayerId { get; set; }
  public int Progress { get; set; }

  public ServerBroadcastBattleLoadProgressT() {
    this.PlayerId = 0;
    this.Progress = 0;
  }
}


static public class ServerBroadcastBattleLoadProgressVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*PlayerId*/, 8 /*ulong*/, 8, false)
      && verifier.VerifyField(tablePos, 6 /*Progress*/, 4 /*int*/, 4, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}
