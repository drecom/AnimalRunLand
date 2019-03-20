# このコードは、YAMLのマスターデータ仕様から自動生成されたものです。
# 直接編集はしないでください。
defmodule Masterdata.Boot.Redirect.Redirecturl do
  @derive [Poison.Encoder]
  defstruct [:id, :api_version, :url]

  @spec load() :: [Masterdata.Boot.Redirect.Redirecturl]
  def load() do
    json = File.read!(FirestormServer.Resource.priv_path("masterdata/priv/masterdata-boot-redirect-redirecturl.json"))
    Poison.decode!(json, as: [%Masterdata.Boot.Redirect.Redirecturl{}])
  end
end
