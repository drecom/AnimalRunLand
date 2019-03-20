defmodule  FirestormServerWeb.PlayerController do
  use FirestormServerWeb, :controller
  require Logger

  def index(conn, _params) do
    render conn, "index.html"
  end

  def get_player_id(conn) do
    conn.assigns.device.player_id
  end


  @spec signup(Phoenix.Controller, map) :: Phoenix.Controller | no_return
  def signup(conn, params) do
    params_map = FirestormServer.Commu.extract_params_map(params)
    FirestormServer.Commu.validate_request(conn, params_map, "player", "signup")

    player_id = get_player_id(conn)
    player_name = params_map["player_name"]
    savedata = params_map["savedata"]
    passcode = params_map["passcode"]
    kind = params_map["kind"]
    # IO.puts "signup : " <> player_name
    IO.puts "signup #{player_id} #{inspect(params_map)}"
    ##   TODO : 文字列の有無効チェック
    change = %{ savedata: savedata, name: player_name, passcode: passcode, kind: kind};
    player_data = %FirestormServer.Player.PlayerData{player_id: player_id}

    {result, _} =
      case FirestormServer.Player.PlayerData |> FirestormServer.Repo.get(player_id) do
        nil -> %FirestormServer.Player.PlayerData{player_id: player_id}
        data -> data
      end
      |> FirestormServer.Player.PlayerData.changeset(change)
      |> FirestormServer.Repo.insert_or_update

    response_map =
    if result == :ok do
      Logger.info("player_data registered : #{player_id}")
      %{accepted: true}
    else
      Logger.error("player_data register failed : #{player_id}")
      %{accepted: false}
    end
    FirestormServer.Commu.response(conn, response_map, "player", "signup")
  end

  # player_id指定でのキャラのデータをセーブする
  @spec save_playerdata(Phoenix.Controller, map) :: Phoenix.Controller | no_return
  def save_playerdata(conn, params) do
    params_map = FirestormServer.Commu.extract_params_map(params)
    FirestormServer.Commu.validate_request(conn, params_map, "player", "save_playerdata")

    player_id = params_map["player_id"]
    player_name = params_map["player_name"]
    savedata = params_map["savedata"]
    passcode = params_map["passcode"]
    kind = params_map["kind"]
    change = %{ savedata: savedata, name: player_name, passcode: passcode, kind: kind};
    player_data = %FirestormServer.Player.PlayerData{player_id: player_id}

    {result, _} =
      case FirestormServer.Player.PlayerData |> FirestormServer.Repo.get(player_id) do
        nil -> %FirestormServer.Player.PlayerData{player_id: player_id}
        data -> data
      end
      |> FirestormServer.Player.PlayerData.changeset(change)
      |> FirestormServer.Repo.insert_or_update

    response_map =
    if result == :ok do
      Logger.info("player_data registered : #{player_id}")
      %{accepted: true}
    else
      Logger.error("player_data register failed : #{player_id}")
      %{accepted: false}
    end

    FirestormServer.Commu.response(conn, response_map, "player", "save_playerdata")
  end

  @spec save_score(Phoenix.Controller, map) :: Phoenix.Controller | no_return
  def save_score(conn, params) do
    import Ecto.Query
    params_map = FirestormServer.Commu.extract_params_map(params)
    FirestormServer.Commu.validate_request(conn, params_map, "player", "save_score")

    player_id = get_player_id(conn)
    score = params_map["score"]
    stage_id = params_map["stage_id"]
    stage_hash = String.to_charlist(stage_id) |> Enum.sum
    kind1 = params_map["kind1"]
    kind2 = params_map["kind2"]
    kind3 = params_map["kind3"]

    #プレイヤー名を引っ張る
    player = FirestormServer.Player.PlayerData |> FirestormServer.Repo.get(player_id)
    player_name = player.name
    IO.puts "player name is  #{inspect(player.name)}"

    changes = %{ score: score, stage_id: stage_id, stage_hash: stage_hash, kind1: kind1, kind2: kind2, kind3: kind3, player_name: player_name }
    {result,_} =
      %FirestormServer.Ranking.RankingData{ player_id: player_id }
      |> FirestormServer.Ranking.RankingData.changeset(changes)
      |> FirestormServer.Repo.insert

    response_map =
    if result == :ok do
      Logger.info("save_score succeeded : #{player_id} #{stage_id} #{score}")
      # 自分のハイスコア
      highscore =
      FirestormServer.Ranking.RankingData
      |> Ecto.Query.where([q], q.stage_hash == ^stage_hash and q.stage_id == ^stage_id)
      |> Ecto.Query.where([q], q.player_id == ^player_id)
      |> Ecto.Query.select([q], max(q.score))
#      |> Ecto.Query.first(desc: :score)
      |> FirestormServer.Repo.one
      IO.inspect(highscore)
      # ステージの順位
      higher =
      FirestormServer.Ranking.RankingData
      |> Ecto.Query.where([q], q.stage_hash == ^stage_hash and q.stage_id == ^stage_id)
      |> Ecto.Query.group_by([q], q.player_id)
      |> Ecto.Query.select([q], %{player_id: q.player_id, score: max(q.score)})
      |> Ecto.Query.where([q], q.score > ^score and q.player_id != ^player_id)
      |> FirestormServer.Repo.all
      order = Enum.count(higher)
      IO.puts "order=#{order}"

      %{accepted: true, order_in_stage: order + 1, my_high_score: highscore }
    else
      Logger.error("save_score failed : #{player_id}")
      %{accepted: false, order_in_stage: 0, my_high_score: 0}
    end
    FirestormServer.Commu.response(conn, response_map, "player", "save_score")
  end


  def get_playername_by_id(player_id) do
    import Ecto.Query
    # TODO: Redis等でキャッシュする
    case FirestormServer.Player.PlayerData |> FirestormServer.Repo.get(player_id) do
      nil -> "no name"
      data -> data.name
    end
  end

end
