# このコードは、YAMLのマスターデータ仕様から自動生成されたものです。
# 直接編集はしないでください。
defmodule Masterdata.Test.RelationData do
  @derive [Poison.Encoder]
  defstruct [:id, :name, :value]

  @spec load() :: [Masterdata.Test.RelationData]
  def load() do
    json = File.read!(FirestormServer.Resource.priv_path("masterdata/masterdata-test-relation_data.json"))
    Poison.decode!(json, as: [%Masterdata.Test.RelationData{}])
  end
end
