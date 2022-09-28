create table if not exists "user" (
    id text not null constraint user_pk primary key,
    created_at timestamp with time zone default now() not null,
    username text not null,
    avatar_url text not null,
    contributor boolean default false not null,
    supporter integer default 0 not null,
    provider_name text not null,
    provider_id text not null
);

create unique index username_unique on "user" (lower(username));