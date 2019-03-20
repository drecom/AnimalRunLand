defmodule FirestormServer.Repo.Migrations.CreatePlayerData do
  use Ecto.Migration

  def change do
    create table(:player_data, primary_key: false) do
      add :player_id, :string, primary_key: true
      add :savedata, :text
      add :name, :string
      add :passcode, :string
      add :kind, :integer

      timestamps()
    end
    execute("ALTER TABLE player_data PARTITION BY KEY() PARTITIONS 4")
  end
end
