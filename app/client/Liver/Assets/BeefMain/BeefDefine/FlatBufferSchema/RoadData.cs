// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace BeefDefine.FlatBufferSchema
{

using global::System;
using global::BeefDefine.FlatBuffers;

public struct RoadData : IFlatbufferObject
{
    private Table __p;
    public ByteBuffer ByteBuffer
    {
        get
        {
            return __p.bb;
        }
    }
    public static RoadData GetRootAsRoadData(ByteBuffer _bb)
    {
        return GetRootAsRoadData(_bb, new RoadData());
    }
    public static RoadData GetRootAsRoadData(ByteBuffer _bb, RoadData obj)
    {
        return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb));
    }
    public void __init(int _i, ByteBuffer _bb)
    {
        __p.bb_pos = _i;
        __p.bb = _bb;
    }
    public RoadData __assign(int _i, ByteBuffer _bb)
    {
        __init(_i, _bb);
        return this;
    }

    public string Id
    {
        get
        {
            int o = __p.__offset(4);
            return o != 0 ? __p.__string(o + __p.bb_pos) : null;
        }
    }
    public ArraySegment<byte>? GetIdBytes()
    {
        return __p.__vector_as_arraysegment(4);
    }
    public NodeData ? Positions(int j)
    {
        int o = __p.__offset(6);
        return o != 0 ? (NodeData ?)(new NodeData()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null;
    }
    public int PositionsLength
    {
        get
        {
            int o = __p.__offset(6);
            return o != 0 ? __p.__vector_len(o) : 0;
        }
    }
    public StringDictionary ? Info
    {
        get
        {
            int o = __p.__offset(8);
            return o != 0 ? (StringDictionary ?)(new StringDictionary()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null;
        }
    }

    public static Offset<RoadData> CreateRoadData(FlatBufferBuilder builder,
            StringOffset IdOffset = default(StringOffset),
            VectorOffset PositionsOffset = default(VectorOffset),
            Offset<StringDictionary> InfoOffset = default(Offset<StringDictionary>))
    {
        builder.StartObject(3);
        RoadData.AddInfo(builder, InfoOffset);
        RoadData.AddPositions(builder, PositionsOffset);
        RoadData.AddId(builder, IdOffset);
        return RoadData.EndRoadData(builder);
    }

    public static void StartRoadData(FlatBufferBuilder builder)
    {
        builder.StartObject(3);
    }
    public static void AddId(FlatBufferBuilder builder, StringOffset IdOffset)
    {
        builder.AddOffset(0, IdOffset.Value, 0);
    }
    public static void AddPositions(FlatBufferBuilder builder, VectorOffset PositionsOffset)
    {
        builder.AddOffset(1, PositionsOffset.Value, 0);
    }
    public static VectorOffset CreatePositionsVector(FlatBufferBuilder builder, Offset<NodeData>[] data)
    {
        builder.StartVector(4, data.Length, 4);

        for (int i = data.Length - 1; i >= 0; i--)
        {
            builder.AddOffset(data[i].Value);
        }

        return builder.EndVector();
    }
    public static void StartPositionsVector(FlatBufferBuilder builder, int numElems)
    {
        builder.StartVector(4, numElems, 4);
    }
    public static void AddInfo(FlatBufferBuilder builder, Offset<StringDictionary> InfoOffset)
    {
        builder.AddOffset(2, InfoOffset.Value, 0);
    }
    public static Offset<RoadData> EndRoadData(FlatBufferBuilder builder)
    {
        int o = builder.EndObject();
        return new Offset<RoadData>(o);
    }
};


}
