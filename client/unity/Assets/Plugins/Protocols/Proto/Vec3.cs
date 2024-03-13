// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Proto
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct Vec3 : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static Vec3 GetRootAsVec3(ByteBuffer _bb) { return GetRootAsVec3(_bb, new Vec3()); }
  public static Vec3 GetRootAsVec3(ByteBuffer _bb, Vec3 obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public Vec3 __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int X { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public int Y { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public int Z { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }

  public static Offset<Proto.Vec3> CreateVec3(FlatBufferBuilder builder,
      int x = 0,
      int y = 0,
      int z = 0) {
    builder.StartTable(3);
    Vec3.AddZ(builder, z);
    Vec3.AddY(builder, y);
    Vec3.AddX(builder, x);
    return Vec3.EndVec3(builder);
  }

  public static void StartVec3(FlatBufferBuilder builder) { builder.StartTable(3); }
  public static void AddX(FlatBufferBuilder builder, int x) { builder.AddInt(0, x, 0); }
  public static void AddY(FlatBufferBuilder builder, int y) { builder.AddInt(1, y, 0); }
  public static void AddZ(FlatBufferBuilder builder, int z) { builder.AddInt(2, z, 0); }
  public static Offset<Proto.Vec3> EndVec3(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<Proto.Vec3>(o);
  }
  public Vec3T UnPack() {
    var _o = new Vec3T();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(Vec3T _o) {
    _o.X = this.X;
    _o.Y = this.Y;
    _o.Z = this.Z;
  }
  public static Offset<Proto.Vec3> Pack(FlatBufferBuilder builder, Vec3T _o) {
    if (_o == null) return default(Offset<Proto.Vec3>);
    return CreateVec3(
      builder,
      _o.X,
      _o.Y,
      _o.Z);
  }
}

public class Vec3T
{
  public int X { get; set; }
  public int Y { get; set; }
  public int Z { get; set; }

  public Vec3T() {
    this.X = 0;
    this.Y = 0;
    this.Z = 0;
  }
}


static public class Vec3Verify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*X*/, 4 /*int*/, 4, false)
      && verifier.VerifyField(tablePos, 6 /*Y*/, 4 /*int*/, 4, false)
      && verifier.VerifyField(tablePos, 8 /*Z*/, 4 /*int*/, 4, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}
