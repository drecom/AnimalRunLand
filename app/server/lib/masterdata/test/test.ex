# このコードは、YAMLのマスターデータ仕様から自動生成されたものです。
# 直接編集はしないでください。
defmodule Masterdata.Test.Test do
  @derive [Poison.Encoder]
  defstruct [:id, :name, :description, :value, :relation, :arr]

  @spec load() :: [Masterdata.Test.Test]
  def load() do
    json = File.read!(FirestormServer.Resource.priv_path("masterdata/masterdata-test-test.json"))
    Poison.decode!(json, as: [%Masterdata.Test.Test{}])
  end
end
