// このコードは、YAMLのマスターデータ仕様から自動生成されたものです。
// 直接編集はしないでください。

namespace Api.Net.Boot
{
public partial class Boot
{
    static partial void OnResponse(Response response)
    {
        string jsonStr = response.MasterdataJson;
        LitJson.JsonData jsonData = LitJson.JsonMapper.ToObject(jsonStr);
        Masterdata.Test.Test.Load((string)jsonData["masterdata-test-test.json"]);
        Masterdata.Test.RelationData.Load((string)jsonData["masterdata-test-relation_data.json"]);
    }
}
}
