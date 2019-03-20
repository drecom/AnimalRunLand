defmodule FirestormServerWeb.ControllerTestHelper do
  require Phoenix.ConnTest
  @endpoint FirestormServerWeb.Endpoint

  @spec get_response(Phoenix.Controller) :: map
  def get_response(conn) do
    response = Phoenix.ConnTest.text_response(conn, 200)
    # plane_response = FirestormServer.Crypto.decode(response)
    # FirestormServer.Commu.json_to_map(plane_response)
    FirestormServer.Commu.json_to_map(response)
  end

  @spec register(Phoenix.Controller) :: {String.t, String.t}
  def register(conn) do
    secret_key = "1234567890"
    udid = "0123456789"
    player_id = "123"

    params = %{p: %{secret_key: secret_key, udid: udid, player_id: player_id}}
    conn = Phoenix.ConnTest.post(conn, "/api/auth/register", params)
    response_map = get_response(conn)
    {response_map["player_id"], secret_key}
  end

  @spec get_access_token(Phoenix.Controller, String.t, String.t) :: String.t
  def get_access_token(conn, player_id, secret_key) do
    params = %{p: %{player_id: player_id, secret_key: secret_key}}
    conn = Phoenix.ConnTest.post(conn, "/api/auth/accesstoken", params)
    response_map = get_response(conn)
    response_map["access_token"]
  end

  @spec set_platform(Phoenix.Controller, String.t | nil) :: Phoenix.Controller
  def set_platform(conn, platform \\ nil) do
    platform = if platform == nil do "android" else platform end
    Plug.Conn.put_req_header(conn, "x-techarts-platform", platform)
  end

  @spec test_post(Phoenix.Controller, String.t, map | nil, String.t | nil) :: {map, String.t}
  def test_post(conn, api_name, params \\ nil, api_version \\ nil) do
    conn = set_platform(conn)
    {player_id, secret_key} = register(conn)
    access_token = get_access_token(conn, player_id, secret_key)
    test_post_with_token(conn, api_name, access_token, params, api_version)
  end

  @spec test_post_with_token(Phoenix.Controller, String.t, String.t, map | nil, String.t | nil) :: {map, String.t}
  def test_post_with_token(conn, api_name, access_token, params \\ nil, api_version \\ nil) do
    params_map = if params == nil do nil else %{p: params} end
    api_version = if api_version == nil do Masterdata.Constants.Api_version.ApiVersion.version else api_version end

    conn = conn |> set_platform
                |> Plug.Conn.put_req_header("authorization", "techarts " <> access_token <> " " <> api_version)
                |> Phoenix.ConnTest.post(api_name, params_map)
    {get_response(conn), access_token}
  end
end
