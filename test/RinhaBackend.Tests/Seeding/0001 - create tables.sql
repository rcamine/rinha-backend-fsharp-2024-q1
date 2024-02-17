CREATE TABLE IF NOT EXISTS cliente
(
    id     integer PRIMARY KEY NOT NULL,
    saldo  integer             NOT NULL,
    limite integer             NOT NULL
);

CREATE TABLE IF NOT EXISTS transacao
(
    id          SERIAL PRIMARY KEY,
    valor       integer     NOT NULL,
    descricao   varchar(10) NOT NULL,
    realizadaem timestamp   NOT NULL,
    idcliente   integer     NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_transacao_idcliente ON transacao (idcliente ASC);