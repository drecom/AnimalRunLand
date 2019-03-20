use Mix.Config

# We don't run a server during test. If one is required,
# you can enable the server option below.
config :firestorm_server, FirestormServerWeb.Endpoint,
  http: [port: 4011],
  server: false

# Print only warnings and errors during test
config :logger, level: :warn

# Configure your database
config :firestorm_server, FirestormServer.Repo,
  adapter: Ecto.Adapters.MySQL,
  username: "root",
  password: "",
  database: "firestorm_test",
  hostname: "localhost",
  pool: Ecto.Adapters.SQL.Sandbox

config :firestorm_server, :crypto, false
