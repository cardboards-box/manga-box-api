CREATE OR REPLACE function get_touched_manga(platformid text)
    RETURNS TABLE(
        manga_id bigint, 
        manga_chapter_id bigint, 
        first_chapter_id bigint, 
        progress_chapter_id bigint, 
        progress_id bigint, 
        max_chapter_num bigint, 
        chapter_num bigint, 
        page_count integer, 
        chapter_progress numeric, 
        page_progress numeric, 
        favourite boolean, 
        bookmarks integer[], 
        has_bookmarks boolean, 
        completed boolean
    )
    LANGUAGE plpgsql
AS
$$
BEGIN
 RETURN QUERY WITH touched_manga AS (
    SELECT DISTINCT m.*, p.id as profile_id FROM manga m
    JOIN manga_bookmarks mb on m.id = mb.manga_id
    JOIN profiles p on mb.profile_id = p.id
    WHERE p.platform_id = platformId

    UNION

    SELECT DISTINCT m.*, p.id as profile_id FROM manga m
    JOIN manga_favourites mb on m.id = mb.manga_id
    JOIN profiles p on mb.profile_id = p.id
    WHERE p.platform_id = platformId

    UNION

    SELECT DISTINCT m.*, p.id as profile_id FROM manga m
    JOIN manga_progress mb on m.id = mb.manga_id
    JOIN profiles p on mb.profile_id = p.id
    WHERE p.platform_id = platformId
), chapter_numbers AS (
    SELECT
        c.*,
        row_number() over (
            PARTITION BY c.manga_id
            ORDER BY c.ordinal ASC
        ) as row_num
    FROM manga_chapter c
    JOIN touched_manga m ON m.id = c.manga_id
), max_chapter_numbers AS (
    SELECT
        c.manga_id,
        MAX(c.row_num) as max,
        MIN(c.id) as first_chapter_id
    FROM chapter_numbers c
    GROUP BY c.manga_id
), progress AS (
    SELECT
        mp.*
    FROM manga_progress mp
    JOIN profiles p ON p.id = mp.profile_id
    WHERE
        p.platform_id = platformId
), records AS (
    SELECT DISTINCT
        m.id as manga_id,
        mc.id as manga_chapter_id,
        mmc.first_chapter_id as first_chapter_id,
        mp.manga_chapter_id as progress_chapter_id,
        mp.id as progress_id,
        mmc.max as max_chapter_num,
        mc.row_num as chapter_num,
        coalesce(array_length(mc.pages, 1), 0) as page_count,
        (
            CASE
                WHEN mmc.first_chapter_id = mc.id AND mp.page_index IS NULL THEN 0
                ELSE round(mc.row_num / CAST(mmc.max as decimal) * 100, 2)
            END
        ) as chapter_progress,
        coalesce(round(mp.page_index / CAST(array_length(mc.pages, 1) as decimal), 2), 0) * 100 as page_progress,
        coalesce((
            SELECT true
            FROM manga_favourites mf
            WHERE mf.profile_id = m.profile_id AND mf.manga_id = m.id
        ), false) as favourite,
        coalesce(mb.pages, '{}') as bookmarks,
        coalesce((
            SELECT true
            FROM manga_bookmarks mbc
            WHERE mbc.manga_id = m.id
            LIMIT 1
        ), false) as has_bookmarks
    FROM touched_manga m
    LEFT JOIN progress mp ON mp.manga_id = m.id
    LEFT JOIN max_chapter_numbers mmc ON mmc.manga_id = m.id
    LEFT JOIN chapter_numbers mc ON
        (mp.id IS NOT NULL AND mc.id = mp.manga_chapter_id) OR
        (mp.id IS NULL AND mmc.first_chapter_id = mc.id)
    LEFT JOIN manga_bookmarks mb ON mb.manga_chapter_id = mc.id
    WHERE
        m.deleted_at IS NULL
)
SELECT
    DISTINCT
    r.*,
    (CASE WHEN r.chapter_progress >= 100 THEN true ELSE false END) as completed
FROM records r;
END
$$;