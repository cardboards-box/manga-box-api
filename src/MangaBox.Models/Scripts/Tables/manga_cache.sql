CREATE TABLE IF NOT EXISTS manga_cache (
    id BIGSERIAL PRIMARY KEY,

    title text not null,
    source_id text not null,
    provider text not null,
    url text not null,
    cover text not null,
    tags text[] not null default '{}',
    alt_titles text[] not null default '{}',
    description text not null,
    nsfw boolean not null default false,
    hash_id text not null,
    referer text,
    source_created timestamp,
    uploader BIGINT,
    display_title TEXT,
    ordinal_volume_reset BOOLEAN NOT NULL DEFAULT FALSE,

    attributes manga_attribute[] not null default '{}',

    created_at timestamp not null default CURRENT_TIMESTAMP,
    updated_at timestamp not null default CURRENT_TIMESTAMP,
    deleted_at timestamp,

    CONSTRAINT uiq_manga_cache_title_hash UNIQUE(source_id, provider)
);