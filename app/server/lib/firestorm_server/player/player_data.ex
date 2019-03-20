defmodule FirestormServer.Player.PlayerData do
  use Ecto.Schema
  import Ecto.Changeset


  @primary_key false
  schema "player_data" do
    field :player_id, :string, primary_key: true
    field :savedata, :string
    field :name, :string
    field :passcode, :string
    field :kind, :integer

    timestamps()
  end

  @doc false
  def changeset(player_data, attrs) do
    player_data
    |> cast(attrs, [:player_id, :savedata, :name, :passcode, :kind])
    |> validate_required([:player_id, :savedata, :name, :passcode, :kind])
    |> unique_constraint(:player_id)
  end
end
