// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Proto
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct EffectOverTimeData : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static EffectOverTimeData GetRootAsEffectOverTimeData(ByteBuffer _bb) { return GetRootAsEffectOverTimeData(_bb, new EffectOverTimeData()); }
  public static EffectOverTimeData GetRootAsEffectOverTimeData(ByteBuffer _bb, EffectOverTimeData obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public static bool VerifyEffectOverTimeData(ByteBuffer _bb) {Google.FlatBuffers.Verifier verifier = new Google.FlatBuffers.Verifier(_bb); return verifier.VerifyBuffer("", false, EffectOverTimeDataVerify.Verify); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public EffectOverTimeData __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public Proto.EffectOverTimeDataRow? Rows(int j) { int o = __p.__offset(4); return o != 0 ? (Proto.EffectOverTimeDataRow?)(new Proto.EffectOverTimeDataRow()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int RowsLength { get { int o = __p.__offset(4); return o != 0 ? __p.__vector_len(o) : 0; } }

  public static Offset<Proto.EffectOverTimeData> CreateEffectOverTimeData(FlatBufferBuilder builder,
      VectorOffset rowsOffset = default(VectorOffset)) {
    builder.StartTable(1);
    EffectOverTimeData.AddRows(builder, rowsOffset);
    return EffectOverTimeData.EndEffectOverTimeData(builder);
  }

  public static void StartEffectOverTimeData(FlatBufferBuilder builder) { builder.StartTable(1); }
  public static void AddRows(FlatBufferBuilder builder, VectorOffset rowsOffset) { builder.AddOffset(0, rowsOffset.Value, 0); }
  public static VectorOffset CreateRowsVector(FlatBufferBuilder builder, Offset<Proto.EffectOverTimeDataRow>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static VectorOffset CreateRowsVectorBlock(FlatBufferBuilder builder, Offset<Proto.EffectOverTimeDataRow>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateRowsVectorBlock(FlatBufferBuilder builder, ArraySegment<Offset<Proto.EffectOverTimeDataRow>> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateRowsVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<Offset<Proto.EffectOverTimeDataRow>>(dataPtr, sizeInBytes); return builder.EndVector(); }
  public static void StartRowsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<Proto.EffectOverTimeData> EndEffectOverTimeData(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<Proto.EffectOverTimeData>(o);
  }
  public static void FinishEffectOverTimeDataBuffer(FlatBufferBuilder builder, Offset<Proto.EffectOverTimeData> offset) { builder.Finish(offset.Value); }
  public static void FinishSizePrefixedEffectOverTimeDataBuffer(FlatBufferBuilder builder, Offset<Proto.EffectOverTimeData> offset) { builder.FinishSizePrefixed(offset.Value); }
  public EffectOverTimeDataT UnPack() {
    var _o = new EffectOverTimeDataT();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(EffectOverTimeDataT _o) {
    _o.Rows = new List<Proto.EffectOverTimeDataRowT>();
    for (var _j = 0; _j < this.RowsLength; ++_j) {_o.Rows.Add(this.Rows(_j).HasValue ? this.Rows(_j).Value.UnPack() : null);}
  }
  public static Offset<Proto.EffectOverTimeData> Pack(FlatBufferBuilder builder, EffectOverTimeDataT _o) {
    if (_o == null) return default(Offset<Proto.EffectOverTimeData>);
    var _rows = default(VectorOffset);
    if (_o.Rows != null) {
      var __rows = new Offset<Proto.EffectOverTimeDataRow>[_o.Rows.Count];
      for (var _j = 0; _j < __rows.Length; ++_j) { __rows[_j] = Proto.EffectOverTimeDataRow.Pack(builder, _o.Rows[_j]); }
      _rows = CreateRowsVector(builder, __rows);
    }
    return CreateEffectOverTimeData(
      builder,
      _rows);
  }
}

public class EffectOverTimeDataT
{
  public List<Proto.EffectOverTimeDataRowT> Rows { get; set; }

  public EffectOverTimeDataT() {
    this.Rows = null;
  }
  public static EffectOverTimeDataT DeserializeFromBinary(byte[] fbBuffer) {
    return EffectOverTimeData.GetRootAsEffectOverTimeData(new ByteBuffer(fbBuffer)).UnPack();
  }
  public byte[] SerializeToBinary() {
    var fbb = new FlatBufferBuilder(0x10000);
    EffectOverTimeData.FinishEffectOverTimeDataBuffer(fbb, EffectOverTimeData.Pack(fbb, this));
    return fbb.DataBuffer.ToSizedArray();
  }
}


static public class EffectOverTimeDataVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyVectorOfTables(tablePos, 4 /*Rows*/, Proto.EffectOverTimeDataRowVerify.Verify, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}