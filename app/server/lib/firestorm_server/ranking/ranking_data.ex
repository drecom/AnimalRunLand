defmodule FirestormServer.Ranking.RankingData do
  use Ecto.Schema
  import Ecto.Changeset


  @primary_key false
  schema "ranking_data" do
    field :player_id, :string
    field :stage_id, :string
    field :stage_hash, :integer
    field :score, :integer
    field :kind1, :integer
    field :kind2, :integer
    field :kind3, :integer
    field :player_name, :string

    timestamps()
  end

  @doc false
  def changeset(ranking_data, attrs) do
    ranking_data
    |> cast(attrs, [:player_id, :stage_id, :stage_hash, :score, :kind1, :kind2, :kind3, :player_name])
    |> validate_required([:player_id, :stage_id, :stage_hash, :score, :player_name])
    |> unique_constraint(:stage_id,  name: :ranking_data_index)
    |> unique_constraint(:stage_hash,  name: :ranking_data_index2)
    |> unique_constraint(:kind1,  name: :ranking_data_index3)

  end
end
