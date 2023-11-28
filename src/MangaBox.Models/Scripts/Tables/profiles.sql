CREATE TABLE IF NOT EXISTS profiles (
    id BIGSERIAL PRIMARY KEY,

    username text not null,
    avatar text not null,
    platform_id text not null,
    admin bool not null,
    email text not null,
    provider text null,
    provider_id text null,
    settings_blob text not null DEFAULT '{}',

    created_at timestamp,
    updated_at timestamp,
    deleted_at timestamp,

    CONSTRAINT profiles_platform_id_uiq UNIQUE(platform_id)
);