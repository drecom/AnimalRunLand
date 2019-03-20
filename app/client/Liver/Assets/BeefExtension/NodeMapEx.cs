using System.Collections.Generic;
using System.Linq;

namespace BeefMain.Runtime
{
public static class NodeMapEx
{
    public static NodeMap FilterByGroup(this NodeMap @self, int group)
    {
        var groupDict = new NodeMap.GroupDict();
        NodeMap.Group(@self, out groupDict);
        var objs = groupDict.Where((obj) => obj.Value == group);
        var res = new Dictionary<string, NodeMap.NodeData>();

        foreach (var obj in objs)
        {
            res[obj.Key] = @self.nodeMap[obj.Key];
        }

        @self.nodeMap = res;
        return @self;
    }
}
}
