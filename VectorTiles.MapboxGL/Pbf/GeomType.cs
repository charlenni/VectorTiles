namespace VectorTiles.MapboxGL.Pbf
{
    [ProtoBuf.ProtoContract(Name = @"GeomType")]
    public enum GeomType
    {
        [ProtoBuf.ProtoEnum(Name = @"Unknown", Value = 0)]
        Unknown = 0,

        [ProtoBuf.ProtoEnum(Name = @"Point", Value = 1)]
        Point = 1,

        [ProtoBuf.ProtoEnum(Name = @"LineString", Value = 2)]
        LineString = 2,

        [ProtoBuf.ProtoEnum(Name = @"Polygon", Value = 3)]
        Polygon = 3
    }
}
