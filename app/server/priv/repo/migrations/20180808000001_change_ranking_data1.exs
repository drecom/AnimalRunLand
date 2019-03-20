defmodule FirestormServer.Repo.Migrations.ChangeRankingData1 do
  use Ecto.Migration

  def change do
    alter table(:ranking_data, primary_key: false) do
      add :player_name, :string
    end
    flush()
    create index(:ranking_data, :kind1, name: :ranking_data_index3, unique: false)
  end

  def up do
    change()
  end

  def down do
    alter table(:ranking_data, primary_key: false) do
      remove :player_name
    end
  end

end
