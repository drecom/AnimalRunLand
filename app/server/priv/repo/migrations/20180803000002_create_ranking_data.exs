defmodule FirestormServer.Repo.Migrations.CreateRankingData do
  use Ecto.Migration

  def change do
    create table(:ranking_data, primary_key: false) do
      add :player_id, :string
      add :stage_id, :string
      add :stage_hash, :integer
      add :score, :integer
      add :kind1, :integer
      add :kind2, :integer
      add :kind3, :integer

      timestamps()
    end
    create index(:ranking_data, :stage_id, name: :ranking_data_index, unique: false)
    create index(:ranking_data, :stage_hash, name: :ranking_data_index2, unique: false)
    execute("ALTER TABLE ranking_data PARTITION BY HASH(stage_hash) PARTITIONS 4")
#    execute("ALTER TABLE ranking_data PARTITION BY KEY() PARTITIONS 4")
  end

  def up do
    change()
  end

  def down do
    drop table(:ranking_data)
  end

end
