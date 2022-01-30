create type expires_in as enum ('never', '1h', '2h', '10h', '1d', '2d', '1w', '1m', '1y');
create type visibility as enum ('public', 'private', 'profile');

-- TODO: owner
-- TODO: tags
-- TODO: edits

create table base_pastes(
    id text primary key unique not null,
    created_at timestamp not null,
    deletes_at timestamp,
    expires_in expires_in not null default 'never',
    visibility visibility not null default 'public',
    stars integer default 0
);

create table pastes(
    title text
) inherits (base_pastes);

create table encrypted_pastes(
    data text not null,
    key text not null,
    salt text not null
) inherits (base_pastes);

create table pasties(
    id text not null,
    paste_id text not null references base_pastes(id),
    title text,
    language text,
    content text,

    primary key(id, paste_id)
);
