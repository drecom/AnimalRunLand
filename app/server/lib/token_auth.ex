defmodule FirestormServer.TokenAuth.Plug do
  import Plug.Conn
  require Logger

  @behaviour Plug

  def init(opts) do
    opts
  end

  def call(conn, _opts) do
    case token_from_conn(conn) do
      {:ok, token, api_version} -> put_and_conn(conn, token, api_version)
      {:error, message} -> do_error(conn, message)
    end
  end

  defp put_and_conn(conn, token, api_version) do
    player_id = FirestormServer.Redis.get(token)
    if player_id == nil || player_id == :undefined do
      do_error(conn, "token not found")
    else
      server_api_version = Masterdata.Constants.Api_version.ApiVersion.version
      if api_version != server_api_version do
        permit = Masterdata.Constants.Api_version.ApiVersion.permit_version |> String.split(",")
        if Enum.any?(permit, fn x -> x == api_version end) do
          Logger.info("#{api_version} is permitted")
        else
          _ = Logger.info("api version not matched. client(#{api_version}) != #{server_api_version} !")
          message = "API version is not matched. you may need upgrade Client."
          send_resp(conn, 426, Poison.encode!(%{error: message})) |> halt
        end
      end

      device = FirestormServer.Player.Device |> FirestormServer.Repo.get(player_id)
      if device != nil do
        assign(conn, :device, device)
      else
        message = "The user is not found, maybe cleared database. Please initialize client save data."
        send_resp(conn, 410, Poison.encode!(%{error: message})) |> halt
      end
    end
  end

  defp do_error(conn, message) do
    _ = Logger.info("access token error : #{message}")
    send_resp(conn, 401, Poison.encode!(%{error: message})) |> halt
  end

  defp token_from_conn(conn) do
    Plug.Conn.get_req_header(conn, "authorization")
    |> token_from_header
  end
  defp token_from_header(["techarts " <> token_and_api_version]) do 
    [token, api_version] = String.split(token_and_api_version, " ")
    {:ok, token, api_version}
  end
  defp token_from_header(_), do: {:error, :not_present}
end
