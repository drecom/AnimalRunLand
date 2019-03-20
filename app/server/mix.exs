defmodule FirestormServer.Mixfile do
  use Mix.Project

  def project do
    [
      app: :firestorm_server,
      version: "0.0.1",
      elixir: "~> 1.4",
      elixirc_paths: elixirc_paths(Mix.env),
      compilers: [:phoenix, :gettext] ++ Mix.compilers,
      start_permanent: Mix.env == :prod,
      aliases: aliases(),
      deps: deps()
    ]
  end

  defp apps do
    [:phoenix, :phoenix_pubsub, :phoenix_html, :cowboy, :logger, :gettext,
     :phoenix_ecto, :mariaex, :ex_json_schema, :redis_poolex, :exredis, :uuid, :logger_file_backend,
     :timex, :receipt_verifier]
  end
  
  # Configuration for the OTP application.
  #
  # Type `mix help compile.app` for more information.
  def application do
    [
      mod: {FirestormServer.Application, []},
      extra_applications: [:logger, :runtime_tools]
    ]
  end

  # Specifies which paths to compile per environment.
  defp elixirc_paths(:test), do: ["lib", "test/support"]
  defp elixirc_paths(_),     do: ["lib"]

  # Specifies your project dependencies.
  #
  # Type `mix help deps` for examples and options.
  defp deps do
    [
      {:phoenix, "~> 1.3.0"},
      {:phoenix_pubsub, "~> 1.0"},
      {:phoenix_ecto, "~> 3.2"},
      {:mariaex, ">= 0.0.0"},
      {:phoenix_html, "~> 2.10"},
      {:phoenix_live_reload, "~> 1.0", only: :dev},
      {:gettext, "~> 0.11"},
      {:cowboy, "~> 1.0"},
      {:dialyxir, "~> 0.3", only: [:dev]},
      {:distillery, "~> 1.3.4"},
      {:ex_json_schema, "~> 0.2.0"},
      {:redis_poolex, "~> 0.0.5"},
      {:uuid, "~> 1.1"},
      {:logger_file_backend, "~> 0.0.9"},
      {:receipt_verifier, "~> 0.4.0"},
      {:timex, "~> 3.1"},
      {:plug_cowboy, "~> 1.0"},
    ]
  end

  # Aliases are shortcuts or tasks specific to the current project.
  # For example, to create, migrate and run the seeds file at once:
  #
  #     $ mix ecto.setup
  #
  # See the documentation for `Mix` for more info on aliases.
  defp aliases do
    [
      "ecto.setup": ["ecto.create", "ecto.migrate", "run priv/repo/seeds.exs"],
      "ecto.reset": ["ecto.drop", "ecto.setup"],
      "test": ["ecto.create --quiet", "ecto.migrate", "test"]
    ]
  end
end
