-- name: GetPaste :one
select * from paste
where id = $1 limit 1;

-- name: GetPastePasties :many
select * from pasty
where paste_id = $1;

-- name: ExistsPaste :one
select exists(select 1 from paste where id = $1);

-- name: CreatePaste :one
insert into paste (
    id, created_at, expires_in, deletes_at, title, owner_id, private
) values (
    $1, $2, $3, $4, $5, $6, $7
)
returning *;

-- name: CreatePasty :one
insert into pasty (
    id, paste_id, title, content, language
) values (
    $1, $2, $3, $4, $5
)
returning *;

-- name: DeletePaste :exec
delete from paste where id = $1;

-- name: GetPasteCount :one
select count(*) from paste;

-- name: ExistsUserByProvider :one
select exists(select 1 from "user" where provider_name = $1 and provider_id = $2);

-- name: ExistsUserByUsername :one
select exists(select 1 from "user" where lower(username) = lower($1));

-- name: ExistsUserById :one
select exists(select 1 from "user" where id = $1);

-- name: GetUserById :one
select * from "user"
where id = $1 limit 1;

-- name: GetUserByUsername :one
select * from "user"
where lower(username) = lower($1) limit 1;

-- name: GetUserByProvider :one
select * from "user"
where provider_name = $1 and provider_id = $2 limit 1;

-- name: CreateUser :one
insert into "user" (
    id, created_at, username, avatar_url, provider_name, provider_id
) values (
    $1, $2, $3, $4, $5, $6
)
returning *;

-- name: GetUserPublicPastes :many
select * from paste
where owner_id = $1 and private = false
order by paste.created_at desc
limit $2
offset $3;

-- name: GetUserPublicPastesCount :one
select count(*) from paste
where owner_id = $1 and private = false;

-- name: GetUserAllPastes :many
select * from paste
where owner_id = $1
order by paste.created_at desc
limit $2
offset $3;

-- name: GetUserAllPastesCount :one
select count(*) from paste
where owner_id = $1;

-- name: DeleteExpiredPastes :one
with deleted as
    (delete from paste where expires_in != 'never' and deletes_at < now() returning *)
select count(*) from deleted;

-- name: IsPasteStarred :one
select exists(select 1 from star where user_id = $1 and paste_id = $2);

-- name: StarPaste :exec
insert into star (user_id, paste_id) values ($1, $2) returning *;

-- name: UnstarPaste :exec
delete from star where user_id = $1 and paste_id = $2;

-- name: GetPasteStarCount :one
select count(*) from star where paste_id = $1;
