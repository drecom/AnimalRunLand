defmodule FirestormServerWeb.AuthController do
  use FirestormServerWeb, :controller
  require Logger

  @spec register(Phoenix.Controller, map()) :: Phoenix.Controller | no_return
  def register(conn, params) do
    params_map = FirestormServer.Commu.extract_params_map(params)

    FirestormServer.Commu.validate_request(conn, params_map, "auth", "register")
    secret_key = params_map["secret_key"]
    udid = params_map["udid"]
    player_id = if String.length(params_map["player_id"]) < 10 do UUID.uuid4() else params_map["player_id"] end

    device = %FirestormServer.Player.Device{player_id: player_id, secret_key: secret_key, udid: udid, access_level: 0}
    _ = case FirestormServer.Repo.insert(device) do
      {:ok, _} -> Logger.info("device registered : #{player_id}")
      {:error, _} -> Logger.error("device register failed : #{player_id}")
    end

    response_map = %{player_id: player_id}
    FirestormServer.Commu.response(conn, response_map, "auth", "register")
  end

  @spec accesstoken(Phoenix.Controller, map()) :: Phoenix.Controller | no_return
  def accesstoken(conn, params) do
    params_map = FirestormServer.Commu.extract_params_map(params)

    FirestormServer.Commu.validate_request(conn, params_map, "auth", "access_token")
    player_id = params_map["player_id"]
    secret_key = params_map["secret_key"]

    access_token = "accesstoken-" <> Phoenix.Token.sign(conn, secret_key, player_id)
    expires_in = 180

    FirestormServer.Redis.setex(access_token, expires_in, player_id)

    response_map = %{access_token: access_token, expires_in: expires_in}
    FirestormServer.Commu.response(conn, response_map, "auth", "access_token")
  end
end
