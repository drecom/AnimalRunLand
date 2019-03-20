defmodule FirestormServerWeb.PlayerControllerTest do
  use FirestormServerWeb.ConnCase
  require FirestormServerWeb.ControllerTestHelper

  test "POST /player/signup", %{conn: conn} do
    # param api_version in masterdata
    params = %{player_name: "player name", savedata: "savedata", passcode: "passcode", kind: 0 }
    {response_map, access_token} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/player/signup", params)
    #IO.puts "signup"
    #IO.inspect(response_map)
    #IO.inspect(access_token)
  end

  test "POST /player/save_playerdata", %{conn: conn} do
    # param api_version in masterdata
    params = %{player_id: "hogefuga", player_name: "player name", savedata: "savedata", passcode: "passcode", kind: 1 }
    {response_map, access_token} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/player/save_playerdata", params)
    IO.inspect(response_map)
    #IO.inspect(access_token)
  end

  test "POST /player/save_score", %{conn: conn} do
    pl1_params = %{player_name: "player1", savedata: "savedata", passcode: "passcode", kind: 0 }
    {_, at1} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/player/signup", pl1_params)
    pl2_params = %{player_name: "player2", savedata: "savedata", passcode: "passcode", kind: 0 }
    {_, at2} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/player/signup", pl2_params)

#    IO.puts "token1: #{at1}"
#    IO.puts "token2: #{at2}"
    params1 = %{stage_id: "stage1234", stage_hash: 0, score: 1234, kind1: 0, kind2: 0, kind3: 0 }
    params2 = %{stage_id: "stage1234", stage_hash: 0, score: 3456, kind1: 0, kind2: 0, kind3: 0 }
    params3 = %{stage_id: "stage1234", stage_hash: 0, score: 7890, kind1: 0, kind2: 0, kind3: 0 }
    {_response_map, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/player/save_score", at1, params1)
    {_response_map, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/player/save_score", at2, params2)
    {_response_map, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/player/save_score", at2, params3)
    {_response_map, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/player/save_score", at1, params1)
    {_response_map, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/player/save_score", at1, params2)
    #IO.inspect(response_map)
  end
end
