DO $$ BEGIN

    IF NOT EXISTS (
		SELECT 1
		FROM pg_type
		WHERE typname = 'manga_attribute'
	) THEN
		CREATE TYPE manga_attribute AS (
			name text,
			value text
		);
	END IF;

END $$;