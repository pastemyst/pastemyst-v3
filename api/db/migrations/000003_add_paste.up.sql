create table if not exists paste (
    id text not null constraint paste_pk primary key,
    created_at timestamp with time zone default now() not null,
    expires_in expires_in not null default 'never',
    deletes_at timestamp with time zone,
    title text default '' :: text not null,
    owner_id text constraint paste_owner_id_fk references "user"(id),
    private boolean not null default false
);

create unique index if not exists paste_id_uindex on paste(id);

create table if not exists pasty (
    id text not null,
    paste_id text not null constraint pasty_paste_id_fk references paste(id) on delete cascade,
    title text default '' :: text not null,
    content text default '' :: text not null,
    language text default 'plaintext' :: text not null,
    constraint pasties_pk primary key (paste_id, id)
);