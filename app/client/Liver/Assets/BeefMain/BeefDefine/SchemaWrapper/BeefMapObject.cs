

namespace BeefDefine.SchemaWrapper
{
using System.Collections.Generic;
using System.IO;
using BeefDefine.FlatBuffers;
using BeefDefine.FlatBufferSchema;

public class PositionModel
{
    public Position _position;

    private bool _hasInit = false;
    private float _eastLon;
    private float _northLat;

    public float EastLon
    {
        get
        {
            if (!_hasInit)
            {
                Init();
            }

            return _eastLon;
        }
        set
        {
            _eastLon = value;
        }
    }

    public float NorthLat
    {
        get
        {
            if (!_hasInit)
            {
                Init();
            }

            return _northLat;
        }
        set
        {
            _hasInit = true;
            _northLat = value;
        }
    }

    private void Init()
    {
        _eastLon = _position.EastLon;
        _northLat = _position.NorthLat;
        _hasInit = true;
    }

    public PositionModel(float eastLon, float northLat)
    {
        _eastLon = eastLon;
        _northLat = northLat;
        _hasInit = true;
    }

    public PositionModel(Position position)
    {
        _position = position;
    }

    public Offset<Position> Serialize(FlatBufferBuilder flatBufferBuilder)
    {
        Position.StartPosition(flatBufferBuilder);
        Position.AddEastLon(flatBufferBuilder, EastLon);
        Position.AddNorthLat(flatBufferBuilder, NorthLat);
        var offset = Position.EndPosition(flatBufferBuilder);
        return offset;
    }
};

public class InfoListModel
{
    public StringDictionary dictionary;

    private bool _hasInit = false;
    private Dictionary<string, string> _dict = null;

    public bool HasInit
    {
        get
        {
            return _hasInit;
        }
    }

    public Dictionary<string, string> FullInfoList
    {
        get
        {
            if (!_hasInit)
            {
                Init();
            }

            return _dict;
        }
        set
        {
            _hasInit = true;
            _dict = value;
        }
    }

    public bool ExistKey(string key)
    {
        return dictionary.KeyValueListByKey(key) != null;
    }

    public string GetValue(string key)
    {
        return dictionary.KeyValueListByKey(key).Value.StringValue;
    }

    private void Init()
    {
        _dict = new Dictionary<string, string>();

        if (dictionary.ByteBuffer != null)
        {
            for (var i = 0; i < dictionary.KeyValueListLength; i++)
            {
                var key = dictionary.KeyValueList(i).Value.StringKey;
                var value = dictionary.KeyValueList(i).Value.StringValue;
                _dict[key] = value;
            }
        }

        _hasInit = true;
    }

    public InfoListModel(Dictionary<string, string> infoList)
    {
        _dict = infoList;
        _hasInit = true;
    }

    public InfoListModel(StringDictionary dictionary)
    {
        this.dictionary = dictionary;
    }



    public Offset<StringDictionary> Serialize(FlatBufferBuilder flatBufferBuilder)
    {
        var list = new List<Offset<StringKeyValue>>();

        foreach (var key in FullInfoList.Keys)
        {
            var val = FullInfoList[key];
            var keyOffset = flatBufferBuilder.CreateString(key);
            var valOffset = flatBufferBuilder.CreateString(val);
            var keyValue = StringKeyValue.CreateStringKeyValue(flatBufferBuilder, keyOffset, valOffset);
            list.Add(keyValue);
        }

        var listOffset = StringDictionary.CreateKeyValueListVector(flatBufferBuilder, list.ToArray());
        StringDictionary.StartStringDictionary(flatBufferBuilder);
        StringDictionary.AddKeyValueList(flatBufferBuilder, listOffset);
        var endOffset = StringDictionary.EndStringDictionary(flatBufferBuilder);
        return endOffset;
    }
};

public class NodeDataModel
{
    public NodeData nodeData;

    private string id = null;
    private PositionModel position = null;
    private InfoListModel infoList = null;

    public string Id
    {
        get
        {
            if (id == null)
            {
                id = nodeData.Id;
            }

            return id;
        }
        set
        {
            id = value;
        }
    }

    public PositionModel Position
    {
        get
        {
            if (position == null)
            {
                position = new PositionModel(nodeData.Pos.Value);
            }

            return position;
        }
        set
        {
            position = value;
        }
    }

    public InfoListModel InfoList
    {
        get
        {
            if (infoList == null)
            {
                infoList = new InfoListModel(nodeData.Info.Value);
            }

            return infoList;
        }
        set
        {
            infoList = value;
        }
    }

    public NodeDataModel(NodeData nodeData)
    {
        this.nodeData = nodeData;
    }

    public NodeDataModel(string id, PositionModel position, InfoListModel infoList)
    {
        this.id = id;
        this.position = position;
        this.infoList = infoList;
    }

    public Offset<NodeData> Serialize(FlatBufferBuilder flatBufferBuilder)
    {
        var idOffset = flatBufferBuilder.CreateString(Id);
        var positionOffset = Position.Serialize(flatBufferBuilder);
        var infoOffset = InfoList.Serialize(flatBufferBuilder);
        NodeData.StartNodeData(flatBufferBuilder);
        NodeData.AddId(flatBufferBuilder, idOffset);
        NodeData.AddPos(flatBufferBuilder, positionOffset);
        NodeData.AddInfo(flatBufferBuilder, infoOffset);
        var endOffset = NodeData.EndNodeData(flatBufferBuilder);
        return endOffset;
    }
};


public class BuildingDataModel
{
    public BuildingData buildingData;

    private string id = null;
    private List<PositionModel> exteriorPositions = null;
    private InfoListModel infoList = null;

    public string Id
    {
        get
        {
            if (id == null)
            {
                id = buildingData.Id;
            }

            return id;
        }
        set
        {
            id = value;
        }
    }

    public List<PositionModel> ExteriorPositions
    {
        get
        {
            if (exteriorPositions == null)
            {
                exteriorPositions = new List<PositionModel>();

                for (int i = 0; i < buildingData.ExteriorPositionsLength; i++)
                {
                    exteriorPositions.Add(new PositionModel(buildingData.ExteriorPositions(i).Value));
                }
            }

            return exteriorPositions;
        }
        set
        {
            exteriorPositions = value;
        }
    }

    public InfoListModel InfoList
    {
        get
        {
            if (infoList == null)
            {
                infoList = new InfoListModel(buildingData.Info.Value);
            }

            return infoList;
        }
        set
        {
            infoList = value;
        }
    }

    public BuildingDataModel(BuildingData buildingData)
    {
        this.buildingData = buildingData;
    }

    public BuildingDataModel(string id, List<PositionModel> exteriorPositions, InfoListModel infoList)
    {
        this.id = id;
        this.exteriorPositions = exteriorPositions;
        this.infoList = infoList;
    }

    public Offset<BuildingData> Serialize(FlatBufferBuilder flatBufferBuilder)
    {
        var str = flatBufferBuilder.CreateString(Id);
        var list = new List<Offset<Position>>();

        for (int i = 0; i < ExteriorPositions.Count; i++)
        {
            list.Add(ExteriorPositions[i].Serialize(flatBufferBuilder));
        }

        var dataOffset = BuildingData.CreateExteriorPositionsVector(flatBufferBuilder, list.ToArray());
        var infoOffset = InfoList.Serialize(flatBufferBuilder);
        BuildingData.StartBuildingData(flatBufferBuilder);
        BuildingData.AddId(flatBufferBuilder, str);
        BuildingData.AddExteriorPositions(flatBufferBuilder, dataOffset);
        BuildingData.AddInfo(flatBufferBuilder, infoOffset);
        var endOffset = BuildingData.EndBuildingData(flatBufferBuilder);
        return endOffset;
    }
};

public class RoadDataModel
{
    public RoadData roadData;

    private string id = null;
    private List<NodeDataModel> positions = null;
    private InfoListModel infoList = null;

    public string Id
    {
        get
        {
            if (id == null)
            {
                id = roadData.Id;
            }

            return id;
        }
        set
        {
            id = value;
        }
    }

    public List<NodeDataModel> Positions
    {
        get
        {
            if (positions == null)
            {
                var PositionsLength = roadData.PositionsLength;
                positions = new List<NodeDataModel>(PositionsLength);

                for (int i = 0; i < PositionsLength; i++)
                {
                    positions.Add(new NodeDataModel(roadData.Positions(i).Value));
                }
            }

            return positions;
        }
        set
        {
            positions = value;
        }
    }

    public InfoListModel InfoList
    {
        get
        {
            if (infoList == null)
            {
                infoList = new InfoListModel(roadData.Info.Value);
            }

            return infoList;
        }
        set
        {
            infoList = value;
        }
    }

    public RoadDataModel(RoadData roadData)
    {
        this.roadData = roadData;
    }

    public RoadDataModel(string id, List<NodeDataModel> positions, InfoListModel infoList)
    {
        this.id = id;
        this.positions = positions;
        this.infoList = infoList;
    }


    public Offset<RoadData> Serialize(FlatBufferBuilder flatBufferBuilder)
    {
        var idOffset = flatBufferBuilder.CreateString(Id);
        var list = new List<Offset<NodeData>>();

        for (int i = 0; i < Positions.Count; i++)
        {
            list.Add(Positions[i].Serialize(flatBufferBuilder));
        }

        var positionsOffset = RoadData.CreatePositionsVector(flatBufferBuilder, list.ToArray());
        var infoOffset = InfoList.Serialize(flatBufferBuilder);
        RoadData.StartRoadData(flatBufferBuilder);
        RoadData.AddId(flatBufferBuilder, idOffset);
        RoadData.AddPositions(flatBufferBuilder, positionsOffset);
        RoadData.AddInfo(flatBufferBuilder, infoOffset);
        var endOffset = RoadData.EndRoadData(flatBufferBuilder);
        return endOffset;
    }
};



public class BeefMapObjectModel
{
    public BeefMapObject beefMapObject;

    public List<RoadDataModel> RoadDataModels = new List<RoadDataModel>();
    public List<BuildingDataModel> BuildingDataModels = new List<BuildingDataModel>();
    public List<NodeDataModel> NodeDataModels = new List<NodeDataModel>();
    private InfoListModel infoList = null;

    public InfoListModel InfoList
    {
        get
        {
            if (infoList == null)
            {
                infoList = new InfoListModel(beefMapObject.Info.Value);
            }

            return infoList;
        }
        set
        {
            infoList = value;
        }
    }

    public BeefMapObjectModel(BeefMapObject beefMapObject)
    {
        this.beefMapObject = beefMapObject;
        var BuildingDatasLength = beefMapObject.BuildingDatasLength;
        BuildingDataModels = new List<BuildingDataModel>(BuildingDatasLength);

        for (int i = 0; i < BuildingDatasLength; i++)
        {
            BuildingDataModels.Add(new BuildingDataModel(beefMapObject.BuildingDatas(i).Value));
        }

        var RoadDatasLength = beefMapObject.RoadDatasLength;
        RoadDataModels = new List<RoadDataModel>(RoadDatasLength);

        for (int i = 0; i < RoadDatasLength; i++)
        {
            RoadDataModels.Add(new RoadDataModel(beefMapObject.RoadDatas(i).Value));
        }

        var NodeDatasLength = beefMapObject.NodeDatasLength;
        NodeDataModels = new List<NodeDataModel>(NodeDatasLength);

        for (int i = 0; i < NodeDatasLength; i++)
        {
            NodeDataModels.Add(new NodeDataModel(beefMapObject.NodeDatas(i).Value));
        }
    }

    public BeefMapObjectModel()
    {
    }


    public Offset<BeefMapObject> Serialize(FlatBufferBuilder flatBufferBuilder)
    {
        var buildingList = new List<Offset<BuildingData>>();

        for (int i = 0; i < BuildingDataModels.Count; i++)
        {
            buildingList.Add(BuildingDataModels[i].Serialize(flatBufferBuilder));
        }

        var buldingOffset = BeefMapObject.CreateBuildingDatasVector(flatBufferBuilder, buildingList.ToArray());
        var RoadDataList = new List<Offset<RoadData>>();

        for (int i = 0; i < RoadDataModels.Count; i++)
        {
            RoadDataList.Add(RoadDataModels[i].Serialize(flatBufferBuilder));
        }

        var RoadDataListOffset = BeefMapObject.CreateRoadDatasVector(flatBufferBuilder, RoadDataList.ToArray());
        var list = new List<Offset<NodeData>>();

        for (int i = 0; i < NodeDataModels.Count; i++)
        {
            list.Add(NodeDataModels[i].Serialize(flatBufferBuilder));
        }

        var infoOffset = InfoList.Serialize(flatBufferBuilder);
        var nodeListOffset = BeefMapObject.CreateNodeDatasVector(flatBufferBuilder, list.ToArray());
        BeefMapObject.StartBeefMapObject(flatBufferBuilder);
        BeefMapObject.AddBuildingDatas(flatBufferBuilder, buldingOffset);
        BeefMapObject.AddRoadDatas(flatBufferBuilder, RoadDataListOffset);
        BeefMapObject.AddNodeDatas(flatBufferBuilder, nodeListOffset);
        BeefMapObject.AddInfo(flatBufferBuilder, infoOffset);
        var endOffset = BeefMapObject.EndBeefMapObject(flatBufferBuilder);
        return endOffset;
    }

    public static BeefMapObjectModel LoadByData(byte[] bytes)
    {
        var beefMapObject = BeefMapObject.GetRootAsBeefMapObject(new ByteBuffer(bytes));
        return new BeefMapObjectModel(beefMapObject);
    }

    public static BeefMapObjectModel LoadByFile(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        var beefMapObject = BeefMapObject.GetRootAsBeefMapObject(new ByteBuffer(bytes));
        return new BeefMapObjectModel(beefMapObject);
    }

    public static void SaveToFile(string filePath, BeefMapObjectModel beefMapObjectModel)
    {
        var fbb = new FlatBufferBuilder(1024);
        var offset = beefMapObjectModel.Serialize(fbb);
        fbb.Finish(offset.Value);
        var buf = fbb.DataBuffer;
        File.WriteAllBytes(filePath, buf.ToArray(buf.Length - fbb.Offset, fbb.Offset));
    }


};

}