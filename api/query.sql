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
    id, created_at, title
) values (
    $1, $2, $3
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
