create table if not exists example (
    id serial primary key,
    name varchar(100),
    bytes bytea
);