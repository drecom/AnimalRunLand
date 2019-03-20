defmodule FirestormServer.Player.Device do
  use Ecto.Schema
  import Ecto.Changeset


  @primary_key false
  schema "device" do
    field :player_id, :string, primary_key: true
    field :secret_key, :string
    field :udid, :string
    field :access_level, :integer

    timestamps()
  end

  @doc false
  def changeset(device, attrs) do
    device
    |> cast(attrs, [:player_id, :secret_key, :udid, :access_level])
    |> validate_required([:player_id, :secret_key, :udid, :access_level])
    |> unique_constraint(:player_id)
  end
end
