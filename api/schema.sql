create type expires_in as enum ('never', '1h', '2h', '10h', '1d', '2d', '1w', '1m', '1y');

create table if not exists pastes (
    id text not null constraint paste_pk primary key,
    created_at timestamp with time zone default now() not null,
    expires_in expires_in not null default 'never',
    deletes_at timestamp with time zone,
    title text default '' :: text not null
);

alter table
    pastes owner to pastemyst;

create unique index if not exists paste_id_uindex on pastes (id);

create table if not exists pasties (
    id text not null,
    paste_id text not null constraint pasties_pastes_id_fk references pastes,
    title text default '' :: text not null,
    content text default '' :: text not null,
    language text default 'plaintext' :: text not null,
    constraint pasties_pk primary key (paste_id, id)
);

alter table
    pasties owner to pastemyst;

create table if not exists users (
    id text not null constraint user_pk primary key,
    created_at timestamp with time zone default now() not null,
    username text default '' :: text not null,
    avatar_url text default '' :: text not null,
    provider_name text default '' :: text not null,
    provider_id text default '' :: text not null
);

alter table
    users owner to pastemyst;
