// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Proto
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct OrbitMapData : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static OrbitMapData GetRootAsOrbitMapData(ByteBuffer _bb) { return GetRootAsOrbitMapData(_bb, new OrbitMapData()); }
  public static OrbitMapData GetRootAsOrbitMapData(ByteBuffer _bb, OrbitMapData obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public static bool VerifyOrbitMapData(ByteBuffer _bb) {Google.FlatBuffers.Verifier verifier = new Google.FlatBuffers.Verifier(_bb); return verifier.VerifyBuffer("", false, OrbitMapDataVerify.Verify); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public OrbitMapData __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public Proto.OrbitMapDataRow? Rows(int j) { int o = __p.__offset(4); return o != 0 ? (Proto.OrbitMapDataRow?)(new Proto.OrbitMapDataRow()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int RowsLength { get { int o = __p.__offset(4); return o != 0 ? __p.__vector_len(o) : 0; } }

  public static Offset<Proto.OrbitMapData> CreateOrbitMapData(FlatBufferBuilder builder,
      VectorOffset rowsOffset = default(VectorOffset)) {
    builder.StartTable(1);
    OrbitMapData.AddRows(builder, rowsOffset);
    return OrbitMapData.EndOrbitMapData(builder);
  }

  public static void StartOrbitMapData(FlatBufferBuilder builder) { builder.StartTable(1); }
  public static void AddRows(FlatBufferBuilder builder, VectorOffset rowsOffset) { builder.AddOffset(0, rowsOffset.Value, 0); }
  public static VectorOffset CreateRowsVector(FlatBufferBuilder builder, Offset<Proto.OrbitMapDataRow>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static VectorOffset CreateRowsVectorBlock(FlatBufferBuilder builder, Offset<Proto.OrbitMapDataRow>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateRowsVectorBlock(FlatBufferBuilder builder, ArraySegment<Offset<Proto.OrbitMapDataRow>> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateRowsVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<Offset<Proto.OrbitMapDataRow>>(dataPtr, sizeInBytes); return builder.EndVector(); }
  public static void StartRowsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<Proto.OrbitMapData> EndOrbitMapData(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<Proto.OrbitMapData>(o);
  }
  public static void FinishOrbitMapDataBuffer(FlatBufferBuilder builder, Offset<Proto.OrbitMapData> offset) { builder.Finish(offset.Value); }
  public static void FinishSizePrefixedOrbitMapDataBuffer(FlatBufferBuilder builder, Offset<Proto.OrbitMapData> offset) { builder.FinishSizePrefixed(offset.Value); }
  public OrbitMapDataT UnPack() {
    var _o = new OrbitMapDataT();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(OrbitMapDataT _o) {
    _o.Rows = new List<Proto.OrbitMapDataRowT>();
    for (var _j = 0; _j < this.RowsLength; ++_j) {_o.Rows.Add(this.Rows(_j).HasValue ? this.Rows(_j).Value.UnPack() : null);}
  }
  public static Offset<Proto.OrbitMapData> Pack(FlatBufferBuilder builder, OrbitMapDataT _o) {
    if (_o == null) return default(Offset<Proto.OrbitMapData>);
    var _rows = default(VectorOffset);
    if (_o.Rows != null) {
      var __rows = new Offset<Proto.OrbitMapDataRow>[_o.Rows.Count];
      for (var _j = 0; _j < __rows.Length; ++_j) { __rows[_j] = Proto.OrbitMapDataRow.Pack(builder, _o.Rows[_j]); }
      _rows = CreateRowsVector(builder, __rows);
    }
    return CreateOrbitMapData(
      builder,
      _rows);
  }
}

public class OrbitMapDataT
{
  public List<Proto.OrbitMapDataRowT> Rows { get; set; }

  public OrbitMapDataT() {
    this.Rows = null;
  }
  public static OrbitMapDataT DeserializeFromBinary(byte[] fbBuffer) {
    return OrbitMapData.GetRootAsOrbitMapData(new ByteBuffer(fbBuffer)).UnPack();
  }
  public byte[] SerializeToBinary() {
    var fbb = new FlatBufferBuilder(0x10000);
    OrbitMapData.FinishOrbitMapDataBuffer(fbb, OrbitMapData.Pack(fbb, this));
    return fbb.DataBuffer.ToSizedArray();
  }
}


static public class OrbitMapDataVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyVectorOfTables(tablePos, 4 /*Rows*/, Proto.OrbitMapDataRowVerify.Verify, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}
