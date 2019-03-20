import Ecto.Query

defmodule  FirestormServerWeb.RankingController do
  use FirestormServerWeb, :controller
  require Logger
  require FirestormServerWeb.PlayerController

  def index(conn, _params) do
    render conn, "index.html"
  end

  def get_player_id(conn) do
    conn.assigns.device.player_id
  end


  @spec retrieve(Phoenix.Controller, map) :: Phoenix.Controller | no_return
  def retrieve(conn, params) do
    params_map = FirestormServer.Commu.extract_params_map(params)
    FirestormServer.Commu.validate_request(conn, params_map, "ranking", "retrieve")

    player_id = get_player_id(conn)
    ranking_type = params_map["ranking_type"]
    stage_id = params_map["stage_id"]
    param1 = params_map["kind1"];
    param2 = params_map["kind2"];
    term = params_map["term"];
    count = params_map["count"];

    sid =
      case String.length(stage_id) do
        0 -> nil
        _ -> stage_id
      end

    # ランキング種別 0:score全体 1:stage毎のハイスコア 2:kind1毎の累計score 3:stage1位数
    response_map =
    case ranking_type do
      0 -> all_ranking(player_id, sid, count)
      1 -> stage_ranking(player_id, stage_id, count)
      2 -> kind1_ranking(player_id, param1, count)
      3 -> stage_top_count_ranking(player_id, count)
      _ -> %{success: false, order: 0, total_count: 0, rankings: []}
    end
    IO.puts "### retrieve #{inspect(response_map)}"
    #response_map = %{ success: true, order: 1, total_count: 100, rankings: [%{player_id: player_id, player_name: "pn", score: 1, order: 1}]}
    # player_nameのチェック
    FirestormServer.Commu.response(conn, response_map, "ranking", "retrieve")
  end

  # プレイヤーごとの累計スコアランキング (stage_idがnilでない場合はステージ限定)
  defp all_ranking(player_id, stage_id, count) do
    import Ecto.Query
    # TODO: rankingsは、日時処理でキャッシュする
    rankings =
    if is_nil(stage_id) do
      FirestormServer.Ranking.RankingData
    else
      FirestormServer.Ranking.RankingData
      |> Ecto.Query.where([q], q.stage_id == ^stage_id)
    end
    |> Ecto.Query.group_by([q], q.player_id)
    |> Ecto.Query.select([q], %{player_id: q.player_id,
                               player_name: q.player_name,
                               score: fragment("sum(?) as score", q.score),
                               order: "0"})
    |> Ecto.Query.order_by([q], desc: fragment("score"))
    |> FirestormServer.Repo.all

    IO.inspect(rankings)
    my_data = rankings |> Enum.find(fn x -> x.player_id == player_id end)
    [score, my_order] =
    if is_nil(my_data) do
      [0, -1]
    else
      IO.inspect(my_data)
      sc = my_data.score
      myo = rankings |> Enum.count(fn x -> x.score > sc end)
      [sc, myo]
    end

    rankingResult = set_names(Enum.take(rankings, count))

    IO.inspect(rankingResult)
    %{success: true, score: score, order: my_order, total_count: Enum.count(rankings), rankings: rankingResult}
  end

  # ステージの順位(ハイスコアベース)
  defp stage_ranking(player_id, stage_id, count) do
    import Ecto.Query
    stage_hash = String.to_charlist(stage_id) |> Enum.sum

    rankings =
    FirestormServer.Ranking.RankingData
    |> Ecto.Query.where([q], q.stage_hash == ^stage_hash and q.stage_id == ^stage_id)
    |> Ecto.Query.group_by([q], q.player_id)
    |> Ecto.Query.select([q], %{player_id: q.player_id, player_name: q.player_name, score: fragment("max(?) as score", q.score), order: "0"})
    |> Ecto.Query.order_by([q], desc: fragment("score"))
    |> FirestormServer.Repo.all

    my_data = rankings |> Enum.find(fn x -> x.player_id == player_id end)

    [score, my_order] =
    if is_nil(my_data) do
      [0, -1]
    else
      IO.inspect(my_data)
      sc = my_data.score
      myo = rankings |> Enum.count(fn x -> x.score > sc end)
      [sc, myo]
    end

    rankingResult =
      Enum.take(rankings, count)
      |> set_names

    %{success: true, score: "#{score}", order: my_order, total_count: Enum.count(rankings), rankings: rankingResult}
  end

  # kind1のランキング
  defp kind1_ranking(player_id, kind1, count) do
    import Ecto.Query

    rankings =
    FirestormServer.Ranking.RankingData
    |> Ecto.Query.where([q], q.kind1 == ^kind1)
    |> Ecto.Query.group_by([q], q.player_id)
    |> Ecto.Query.select([q], %{player_id: q.player_id, player_name: q.player_name, score: fragment("sum(?) as score", q.score), order: "0"})
    |> Ecto.Query.order_by([q], desc: fragment("score"))
    |> FirestormServer.Repo.all

    my_data = rankings |> Enum.find(fn x -> x.player_id == player_id end)

    [score, my_order] =
    if is_nil(my_data) do
      [0, -1]
    else
      IO.inspect(my_data)
      sc = my_data.score
      myo = rankings |> Enum.count(fn x -> x.score > sc end)
      [sc, myo]
    end

    result =
    Enum.take(rankings, count) |> set_names

    %{success: true, score: score, order: my_order, total_count: Enum.count(rankings), rankings: result}
  end

  # ステージ一位の数ランキング
  # TODO: これも重いからdaylyにしたい
  defp stage_top_count_ranking(player_id, count) do
    stage_tops =
     FirestormServer.Ranking.RankingData
    # stage_id毎のハイスコアを取得するSQLをJOINする
    |> Ecto.Query.join(:inner, [q], p in fragment("select stage_id, max(score) as maxscore from ranking_data as p group by stage_id"))
    # ハイスコアを持つレコードに絞り込む
    |> Ecto.Query.where([q,p], q.stage_id == p.stage_id and q.score == p.maxscore)
    # プレイヤーごとにする
    |> Ecto.Query.group_by([q], q.player_id)
    # プレイヤーの数で集計する
    |> Ecto.Query.select([q], %{ player_id: q.player_id, topcount: count(q.player_id), player_name: q.player_name })
    |> Ecto.Query.order_by([q], desc: count(q.player_id))
    |> FirestormServer.Repo.all


    IO.puts "--------------------------------------"
    IO.puts "mydata: #{player_id}"
    IO.inspect(stage_tops)

    total_count = Enum.count(stage_tops)

    order = case stage_tops |> Enum.find_index(fn x -> x.player_id == player_id end) do
              nil -> -1
              data -> data
              _ -> -1
            end

    rankings =
      stage_tops
      |> Enum.map(fn q -> %{player_id: q.player_id,player_name: q.player_name, score: q.topcount, order: "0"} end)
      |> set_names


    %{success: true, order: order, total_count: total_count, rankings: rankings}
  end


  # ランキングの名前を最新のものにする
  defp set_names(rankings) do
    import Ecto.Query
    r =
      rankings |> Enum.map(fn x ->
      name = FirestormServerWeb.PlayerController.get_playername_by_id(x.player_id)
      %{player_id: x.player_id, player_name: name, score: "#{x.score}", order: x.order } end)
  end

end
