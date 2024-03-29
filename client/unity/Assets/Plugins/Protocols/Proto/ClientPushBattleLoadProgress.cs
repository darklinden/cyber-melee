// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Proto
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct ClientPushBattleLoadProgress : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static ClientPushBattleLoadProgress GetRootAsClientPushBattleLoadProgress(ByteBuffer _bb) { return GetRootAsClientPushBattleLoadProgress(_bb, new ClientPushBattleLoadProgress()); }
  public static ClientPushBattleLoadProgress GetRootAsClientPushBattleLoadProgress(ByteBuffer _bb, ClientPushBattleLoadProgress obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public ClientPushBattleLoadProgress __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int Progress { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }

  public static Offset<Proto.ClientPushBattleLoadProgress> CreateClientPushBattleLoadProgress(FlatBufferBuilder builder,
      int progress = 0) {
    builder.StartTable(1);
    ClientPushBattleLoadProgress.AddProgress(builder, progress);
    return ClientPushBattleLoadProgress.EndClientPushBattleLoadProgress(builder);
  }

  public static void StartClientPushBattleLoadProgress(FlatBufferBuilder builder) { builder.StartTable(1); }
  public static void AddProgress(FlatBufferBuilder builder, int progress) { builder.AddInt(0, progress, 0); }
  public static Offset<Proto.ClientPushBattleLoadProgress> EndClientPushBattleLoadProgress(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<Proto.ClientPushBattleLoadProgress>(o);
  }
  public ClientPushBattleLoadProgressT UnPack() {
    var _o = new ClientPushBattleLoadProgressT();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(ClientPushBattleLoadProgressT _o) {
    _o.Progress = this.Progress;
  }
  public static Offset<Proto.ClientPushBattleLoadProgress> Pack(FlatBufferBuilder builder, ClientPushBattleLoadProgressT _o) {
    if (_o == null) return default(Offset<Proto.ClientPushBattleLoadProgress>);
    return CreateClientPushBattleLoadProgress(
      builder,
      _o.Progress);
  }
}

public class ClientPushBattleLoadProgressT
{
  public int Progress { get; set; }

  public ClientPushBattleLoadProgressT() {
    this.Progress = 0;
  }
}


static public class ClientPushBattleLoadProgressVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*Progress*/, 4 /*int*/, 4, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}
