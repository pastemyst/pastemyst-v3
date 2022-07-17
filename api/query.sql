-- name: GetPaste :one
select * from pastes
where id = $1 limit 1;

-- name: GetPastePasties :many
select * from pasties
where paste_id = $1;

-- name: ExistsPaste :one
select exists(select 1 from pastes where id = $1);

-- name: CreatePaste :one
insert into pastes (
    id, created_at, expires_in, deletes_at, title
) values (
    $1, $2, $3, $4, $5
)
returning *;

-- name: CreatePasty :one
insert into pasties (
    id, paste_id, title, content
) values (
    $1, $2, $3, $4
)
returning *;

-- name: GetPasteCount :one
select count(*) from pastes;

-- name: ExistsUserByProvider :one
select exists(select 1 from users where provider_name = $1 and provider_id = $2);

-- name: ExistsUserByUsername :one
select exists(select 1 from users where username = $1);

-- name: ExistsUserById :one
select exists(select 1 from users where id = $1);

-- name: GetUserById :one
select * from users
where id = $1 limit 1;

-- name: GetUserByUsername :one
select * from users
where username = $1 limit 1;

-- name: GetUserByProvider :one
select * from users
where provider_name = $1 and provider_id = $2 limit 1;

-- name: CreateUser :one
insert into users (
    id, created_at, username, avatar_url, provider_name, provider_id
) values (
    $1, $2, $3, $4, $5, $6
)
returning *;
