defmodule FirestormServerWeb.RankingControllerTest do
  use FirestormServerWeb.ConnCase
  require FirestormServerWeb.ControllerTestHelper

  test "POST /ranking/retrieve_first", %{conn: conn} do
    IO.puts "/ranking/retrieve_noscore"
    stage = UUID.uuid4()
    pl1_params = %{player_name: "player0x", savedata: "savedata", passcode: "passcode", kind: 0 }
    {_, at1} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/player/signup", pl1_params)
     params1 = %{stage_id: stage, stage_hash: 0, score: 1, kind1: 0, kind2: 0, kind3: 0 }
    {response_map, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/player/save_score", at1, params1)
    IO.inspect(response_map)
  end

  test "POST /ranking/retrieve_noscore", %{conn: conn} do
    IO.puts "/ranking/retrieve_noscore"
    stage = UUID.uuid4()
    pl1_params = %{player_name: "player0x", savedata: "savedata", passcode: "passcode", kind: 0 }
    {_, at1} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/player/signup", pl1_params)
    # params1 = %{stage_id: stage, stage_hash: 0, score: 1, kind1: 0, kind2: 0, kind3: 0 }
    # {_response_map, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/player/save_score", at1, params1)
    rparams0 = %{ranking_type: 0, stage_id: stage, kind1: 0, kind2: 0, term: 0, count: 100 }
    {response_map0, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/ranking/retrieve", at1, rparams0)
    IO.inspect(response_map0)
    rparams1 = %{ranking_type: 1, stage_id: stage, kind1: 0, kind2: 0, term: 0, count: 100 }
    {response_map1, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/ranking/retrieve", at1, rparams1)
    IO.inspect(response_map1)
    rparams2 = %{ranking_type: 2, stage_id: stage, kind1: 0, kind2: 0, term: 0, count: 100 }
    {response_map2, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/ranking/retrieve", at1, rparams2)
    IO.inspect(response_map2)
    rparams3 = %{ranking_type: 3, stage_id: stage, kind1: 0, kind2: 0, term: 0, count: 100 }
    {response_map3, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/ranking/retrieve", at1, rparams3)
    IO.inspect(response_map3)
  end

  test "POST /ranking/retrieve", %{conn: conn} do
    Enum.map(0..9, fn x ->
      pl1_params = %{player_name: "player0#{x}", savedata: "savedata", passcode: "passcode", kind: 0 }
      {_, at1} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/player/signup", pl1_params)
      params1 = %{stage_id: "stage1234", stage_hash: 0, score: (x+1)*1234, kind1: 0, kind2: 0, kind3: 0 }
      {_response_map, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/player/save_score", at1, params1)
      end)
    Enum.map(0..9, fn x ->
      pl1_params = %{player_name: "player1#{x}", savedata: "savedata", passcode: "passcode", kind: 0 }
      {_, at1} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/player/signup", pl1_params)
      params1 = %{stage_id: "stage2", stage_hash: 0, score: (x+1)*1234, kind1: 0, kind2: 0, kind3: 0 }
      {_response_map, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/player/save_score", at1, params1)
      end)

    pl2_params = %{player_name: "my player", savedata: "savedata", passcode: "passcode", kind: 0 }
    {_, at2} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/player/signup", pl2_params)
    params2 = %{stage_id: "stage1234", stage_hash: 0, score: 99123, kind1: 0, kind2: 0, kind3: 0 }
    {_response_map, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/player/save_score", at2, params2)
    params3 = %{stage_id: "stage2", stage_hash: 0, score: 9123, kind1: 0, kind2: 0, kind3: 0 }
    {_response_map, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/player/save_score", at2, params3)

    rparams0 = %{ranking_type: 0, stage_id: "stage1234", kind1: 0, kind2: 0, term: 0, count: 100 }
    {response_map0, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/ranking/retrieve", at2, rparams0)
    IO.inspect(response_map0)

    rparams1 = %{ranking_type: 1, stage_id: "stage1234", kind1: 0, kind2: 0, term: 0, count: 100 }
    {response_map1, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/ranking/retrieve", at2, rparams1)
    IO.inspect(response_map1)

    rparams2 = %{ranking_type: 2, stage_id: "stage1234", kind1: 0, kind2: 0, term: 0, count: 100 }
    {response_map2, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/ranking/retrieve", at2, rparams2)
    IO.inspect(response_map2)

    rparams3 = %{ranking_type: 3, stage_id: "stage1234", kind1: 0, kind2: 0, term: 0, count: 100 }
    {response_map3, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/ranking/retrieve", at2, rparams3)
    IO.inspect(response_map3)
  end

  test "POST /ranking/retrieve_total", %{conn: conn} do
    IO.puts "/ranking/retrieve_total"
    stage = UUID.uuid4()
    pl1_params = %{player_name: "player0x", savedata: "savedata", passcode: "passcode", kind: 0 }
    {_, at1} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/player/signup", pl1_params)
    rparams0 = %{ranking_type: 0, stage_id: "", kind1: 0, kind2: 0, term: 0, count: 100 }
    {response_map0, _} = FirestormServerWeb.ControllerTestHelper.test_post_with_token(conn, "api/ranking/retrieve", at1, rparams0)
    IO.inspect(response_map0)
  end

end
