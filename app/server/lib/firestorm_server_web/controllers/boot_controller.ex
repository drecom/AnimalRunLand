defmodule FirestormServerWeb.BootController do
  use FirestormServerWeb, :controller
  require Logger

  @spec boot(Phoenix.Controller, map) :: Phoenix.Controller | no_return
  def boot(conn, params) do
    dir = "priv/masterdata"
    fileList = File.ls!(dir) |> Enum.filter(fn f -> File.dir?(Path.join(dir, f)) == false end)
    jsonMap = Enum.into(fileList, %{}, fn filename ->
      json = File.read!(Path.join(dir, filename))
      _ = Logger.debug(filename)
      {filename, json}
    end)

    params_map = FirestormServer.Commu.extract_params_map(params)
    client_api_version = params_map["client_api_version"]

    redirect_urls = Masterdata.Boot.Redirect.Redirecturl.load()
    result = Enum.find(redirect_urls, fn x -> x.api_version == client_api_version end)
    url = if result == nil do "" else result.url end

    api_version = Masterdata.Constants.Api_version.ApiVersion.version
    # device_id = conn.assigns.device.device_id
    # IO.inspect(device_id)
    # TODO: device_idでハッシュして切り替える
    asset_urls = ["http://assets.militaryacademy.jp/", "http://develop1.moequiz.techarts.co.jp/assets/"]
    responseMap = %{
      masterdata_json: FirestormServer.Commu.to_json_string(jsonMap),
      api_version: api_version,
      api_url: url,
      asset_bundle_url: Enum.at(asset_urls, 0) # "http://develop1.moequiz.techarts.co.jp/assets/"
    }

    FirestormServer.Commu.response(conn, responseMap, "boot", "boot")
  end
end
