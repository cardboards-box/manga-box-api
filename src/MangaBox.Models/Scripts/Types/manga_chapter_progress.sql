DO $$ BEGIN

    IF NOT EXISTS (
		SELECT 1
		FROM pg_type
		WHERE typname = 'manga_chapter_progress'
	) THEN
		CREATE TYPE manga_chapter_progress AS (
			chapter_id BIGINT,
			page_index INT
		);
	END IF;

END $$;