create table if not exists pastes
(
    id         text                                      not null
        constraint paste_pk
            primary key,
    created_at timestamp with time zone default now()    not null,
    title      text                     default ''::text not null
);

alter table pastes
    owner to pastemyst;

create unique index if not exists paste_id_uindex
    on pastes (id);

create table if not exists pasties
(
    id       text                  not null,
    paste_id text                  not null
        constraint pasties_pastes_id_fk
            references pastes,
    title    text default ''::text not null,
    content  text default ''::text not null,
    constraint pasties_pk
        primary key (paste_id, id)
);

alter table pasties
    owner to pastemyst;

