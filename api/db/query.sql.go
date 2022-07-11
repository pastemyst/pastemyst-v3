// Code generated by sqlc. DO NOT EDIT.
// versions:
//   sqlc v1.14.0
// source: query.sql

package db

import (
	"context"
)

const createPaste = `-- name: CreatePaste :one
insert into pastes (
    id, title
) values (
    $1, $2
)
returning id, created_at, title
`

type CreatePasteParams struct {
	ID    string
	Title string
}

func (q *Queries) CreatePaste(ctx context.Context, arg CreatePasteParams) (Paste, error) {
	row := q.db.QueryRowContext(ctx, createPaste, arg.ID, arg.Title)
	var i Paste
	err := row.Scan(&i.ID, &i.CreatedAt, &i.Title)
	return i, err
}

const createPasty = `-- name: CreatePasty :one
insert into pasties (
    id, paste_id, title, content
) values (
    $1, $2, $3, $4
)
returning id, paste_id, title, content
`

type CreatePastyParams struct {
	ID      string
	PasteID string
	Title   string
	Content string
}

func (q *Queries) CreatePasty(ctx context.Context, arg CreatePastyParams) (Pasty, error) {
	row := q.db.QueryRowContext(ctx, createPasty,
		arg.ID,
		arg.PasteID,
		arg.Title,
		arg.Content,
	)
	var i Pasty
	err := row.Scan(
		&i.ID,
		&i.PasteID,
		&i.Title,
		&i.Content,
	)
	return i, err
}

const getPaste = `-- name: GetPaste :one
select id, created_at, title from pastes
where id = $1 limit 1
`

func (q *Queries) GetPaste(ctx context.Context, id string) (Paste, error) {
	row := q.db.QueryRowContext(ctx, getPaste, id)
	var i Paste
	err := row.Scan(&i.ID, &i.CreatedAt, &i.Title)
	return i, err
}

const getPastePasties = `-- name: GetPastePasties :many
select id, paste_id, title, content from pasties
where paste_id = $1
`

func (q *Queries) GetPastePasties(ctx context.Context, pasteID string) ([]Pasty, error) {
	rows, err := q.db.QueryContext(ctx, getPastePasties, pasteID)
	if err != nil {
		return nil, err
	}
	defer rows.Close()
	var items []Pasty
	for rows.Next() {
		var i Pasty
		if err := rows.Scan(
			&i.ID,
			&i.PasteID,
			&i.Title,
			&i.Content,
		); err != nil {
			return nil, err
		}
		items = append(items, i)
	}
	if err := rows.Close(); err != nil {
		return nil, err
	}
	if err := rows.Err(); err != nil {
		return nil, err
	}
	return items, nil
}