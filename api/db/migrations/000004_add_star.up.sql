create table if not exists star (
    user_id text not null constraint star_user_id_fk references "user"(id) on delete cascade,
    paste_id text not null constraint star_paste_id_fk references paste(id) on delete cascade,
    constraint star_pk primary key (user_id, paste_id)
);