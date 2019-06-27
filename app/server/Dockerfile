FROM elixir:1.5.1

ENV NODE_VERSION 10
ENV PHOENIX_VERSION 1.4.0

RUN echo "deb http://deb.debian.org/debian jessie main" > /etc/apt/sources.list \
    && echo "deb http://security.debian.org jessie/updates main" >> /etc/apt/sources.list \
    && curl -sL https://deb.nodesource.com/setup_${NODE_VERSION}.x | bash \
    && apt-get update \
    && apt-get install -y nodejs inotify-tools

RUN mkdir /myapp
WORKDIR /myapp

RUN wget http://download.redis.io/releases/redis-5.0.3.tar.gz \
    && tar xzf redis-5.0.3.tar.gz \
    && cd redis-5.0.3 \
    && make

ADD . /myapp

RUN mix local.hex --force
RUN mix local.rebar --force
RUN mix deps.get
RUN mix deps.compile
RUN cd assets && npm install
