# This file is responsible for configuring your application
# and its dependencies with the aid of the Mix.Config module.
#
# This configuration file is loaded before any dependency and
# is restricted to this project.
use Mix.Config

# General application configuration
config :firestorm_server,
  ecto_repos: [FirestormServer.Repo]

# Configures the endpoint
config :firestorm_server, FirestormServerWeb.Endpoint,
  url: [host: "localhost"],
  secret_key_base: "43O2oyekP1vY9EUWZKXt4i+8hFjrb2k91rZl4FhksyaswiW81G4lHwf4Y82fIHDu",
  render_errors: [view: FirestormServerWeb.ErrorView, accepts: ~w(html json)],
  pubsub: [name: FirestormServer.PubSub,
           adapter: Phoenix.PubSub.PG2]

# Configures Elixir's Logger
config :logger, :console,
  format: "$time $metadata[$level] $message\n",
  metadata: [:request_id]

# Import environment specific config. This must remain at the bottom
# of this file so it overrides the configuration defined above.
import_config "#{Mix.env}.exs"
