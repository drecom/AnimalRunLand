defmodule FirestormServer.Repo.Migrations.CreateDevice do
  use Ecto.Migration

  def change do
    create table(:device, primary_key: false) do
      add :player_id, :string, primary_key: true
      add :secret_key, :string
      add :udid, :string
      add :access_level, :integer

      timestamps()
    end
    execute("ALTER TABLE device PARTITION BY KEY() PARTITIONS 4")
  end
end
