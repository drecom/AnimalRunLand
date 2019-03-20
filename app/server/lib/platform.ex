defmodule FirestormServer.Platform.Plug do
  import Plug.Conn
  require Logger

  @behaviour Plug

  def init(opts) do
    opts
  end

  def call(conn, _opts) do
    case platform_from_conn(conn) do
      {:ok, platform} -> put_and_conn(conn, platform)
      {:error, message} -> do_error(conn, message)
    end
  end

  defp put_and_conn(conn, platform) do
    assign(conn, :platform, platform)
  end

  defp do_error(conn, message) do
    _ = Logger.info("Platform error : #{message}")
    send_resp(conn, 400, Poison.encode!(%{error: message})) |> halt
  end

  defp platform_from_conn(conn) do
    platform = Plug.Conn.get_req_header(conn, "x-techarts-platform")
    case platform do
      ["android"] -> {:ok, :android}
      ["ios"] -> {:ok, :ios}
      [] -> {:error, "platform not present"}
      _ -> {:error, 'unknown platform "#{platform}"'}
    end
  end
end
