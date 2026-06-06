using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class FullText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.IsSqlite())
                throw new InvalidOperationException("Operation not supported for non-SQLite databases");

            migrationBuilder.Sql(
                // lang=sql
                """
                CREATE TABLE FullTextIndex
                (
                    Id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Content    TEXT    NOT NULL,
                    SourceType INTEGER NOT NULL,
                    SourceId   INTEGER NOT NULL,
                    ProjectId  INTEGER,

                    CONSTRAINT UX_FullTextIndex_SourceType_SourceId UNIQUE (SourceType, SourceId)
                );

                CREATE INDEX IX_FullTextIndex_ProjectId
                    ON FullTextIndex (ProjectId);

                -- DocumentChunks
                CREATE TRIGGER FullTextIndex_DocumentChunks_Insert
                    AFTER INSERT
                    ON DocumentChunks
                BEGIN
                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId)
                    SELECT NEW.Content,
                           1,
                           NEW.Id,
                           D.ProjectId
                    FROM DocumentSections DS
                             JOIN Documents D ON D.Id = DS.DocumentId
                    WHERE DS.Id = NEW.DocumentSectionId;
                END;
                CREATE TRIGGER FullTextIndex_DocumentChunks_Delete
                    AFTER DELETE
                    ON DocumentChunks
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 1
                      AND SourceId = OLD.Id;
                END;
                CREATE TRIGGER FullTextIndex_DocumentChunks_Update
                    AFTER UPDATE
                    ON DocumentChunks
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 1
                      AND SourceId = OLD.Id;

                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId)
                    SELECT NEW.Content,
                           1,
                           NEW.Id,
                           D.ProjectId
                    FROM DocumentSections DS
                             JOIN Documents D ON D.Id = DS.DocumentId
                    WHERE DS.Id = NEW.DocumentSectionId;
                END;

                -- ChatSummary
                CREATE TRIGGER FullTextIndex_Chats_Insert
                    AFTER INSERT
                    ON Chats
                BEGIN
                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId)
                    SELECT NEW.Summary, 2, NEW.Id, NEW.ProjectId
                    WHERE NEW.Summary IS NOT NULL;
                END;
                CREATE TRIGGER FullTextIndex_Chats_Delete
                    AFTER DELETE
                    ON Chats
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 2
                      AND SourceId = OLD.Id;
                END;
                CREATE TRIGGER FullTextIndex_Chats_Update
                    AFTER UPDATE
                    ON Chats
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 2
                      AND SourceId = OLD.Id;

                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId)
                    SELECT NEW.Summary, 2, NEW.Id, NEW.ProjectId
                    WHERE NEW.Summary IS NOT NULL;
                END;

                -- ChatFact
                CREATE TRIGGER FullTextIndex_ChatFacts_Insert
                    AFTER INSERT
                    ON ChatFacts
                BEGIN
                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId)
                    SELECT NEW.Fact,
                           3,
                           NEW.Id,
                           C.ProjectId
                    FROM Chats C
                    WHERE C.Id = NEW.ChatId;
                END;
                CREATE TRIGGER FullTextIndex_ChatFacts_Delete
                    AFTER DELETE
                    ON ChatFacts
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 3
                      AND SourceId = OLD.Id;
                END;
                CREATE TRIGGER FullTextIndex_ChatFacts_Update
                    AFTER UPDATE
                    ON ChatFacts
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 3
                      AND SourceId = OLD.Id;

                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId)
                    SELECT NEW.Fact,
                           3,
                           NEW.Id,
                           C.ProjectId
                    FROM Chats C
                    WHERE C.Id = NEW.ChatId;
                END;

                -- ChatDecision
                CREATE TRIGGER FullTextIndex_ChatDecisions_Insert
                    AFTER INSERT
                    ON ChatDecisions
                BEGIN
                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId)
                    SELECT 'Decision: ' || NEW.Decision || ' Reason: ' || NEW.Reason,
                           4,
                           NEW.Id,
                           C.ProjectId
                    FROM Chats C
                    WHERE C.Id = NEW.ChatId;
                END;
                CREATE TRIGGER FullTextIndex_ChatDecisions_Delete
                    AFTER DELETE
                    ON ChatDecisions
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 4
                      AND SourceId = OLD.Id;
                END;
                CREATE TRIGGER FullTextIndex_ChatDecisions_Update
                    AFTER UPDATE
                    ON ChatDecisions
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 4
                      AND SourceId = OLD.Id;

                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId)
                    SELECT 'Decision: ' || NEW.Decision || ' Reason: ' || NEW.Reason,
                           4,
                           NEW.Id,
                           C.ProjectId
                    FROM Chats C
                    WHERE C.Id = NEW.ChatId;
                END;

                -- ChatUserPreference
                CREATE TRIGGER FullTextIndex_ChatUserPreferences_Insert
                    AFTER INSERT
                    ON ChatUserPreferences
                BEGIN
                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId)
                    SELECT NEW.Preference,
                           5,
                           NEW.Id,
                           C.ProjectId
                    FROM Chats C
                    WHERE C.Id = NEW.ChatId;
                END;
                CREATE TRIGGER FullTextIndex_ChatUserPreferences_Delete
                    AFTER DELETE
                    ON ChatUserPreferences
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 5
                      AND SourceId = OLD.Id;
                END;
                CREATE TRIGGER FullTextIndex_ChatUserPreferences_Update
                    AFTER UPDATE
                    ON ChatUserPreferences
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 5
                      AND SourceId = OLD.Id;

                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId)
                    SELECT NEW.Preference,
                           5,
                           NEW.Id,
                           C.ProjectId
                    FROM Chats C
                    WHERE C.Id = NEW.ChatId;
                END;

                CREATE VIRTUAL TABLE FullTextIndexVirt USING fts5
                (
                    Content,
                    content='FullTextIndex',
                    content_rowid='Id',
                    tokenize = porter
                );

                CREATE TRIGGER FullTextIndexVirt_Insert
                    AFTER INSERT
                    ON FullTextIndex
                BEGIN
                    INSERT INTO FullTextIndexVirt(ROWID, Content)
                    VALUES (NEW.Id, NEW.Content);
                END;
                CREATE TRIGGER FullTextIndexVirt_Delete
                    AFTER DELETE
                    ON FullTextIndex
                BEGIN
                    INSERT INTO FullTextIndexVirt(FullTextIndexVirt, ROWID)
                    VALUES ('delete', OLD.Id);
                END;
                CREATE TRIGGER FullTextIndexVirt_Update
                    AFTER UPDATE
                    ON FullTextIndex
                BEGIN
                    INSERT INTO FullTextIndexVirt(FullTextIndexVirt, ROWID)
                    VALUES ('delete', OLD.Id);

                    INSERT INTO FullTextIndexVirt(ROWID, Content)
                    VALUES (NEW.Id, NEW.Content);
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.IsSqlite())
                throw new InvalidOperationException("Operation not supported for non-SQLite databases");

            migrationBuilder.Sql(
                // lang=sql
                """
                DROP TRIGGER IF EXISTS FullTextIndexVirt_Insert;
                DROP TRIGGER IF EXISTS FullTextIndexVirt_Delete;
                DROP TRIGGER IF EXISTS FullTextIndexVirt_Update;
                DROP TABLE IF EXISTS FullTextIndexVirt;

                DROP TRIGGER IF EXISTS FullTextIndex_DocumentChunks_Insert;
                DROP TRIGGER IF EXISTS FullTextIndex_DocumentChunks_Delete;
                DROP TRIGGER IF EXISTS FullTextIndex_DocumentChunks_Update;

                DROP TRIGGER IF EXISTS FullTextIndex_Chats_Insert;
                DROP TRIGGER IF EXISTS FullTextIndex_Chats_Delete;
                DROP TRIGGER IF EXISTS FullTextIndex_Chats_Update;

                DROP TRIGGER IF EXISTS FullTextIndex_ChatFacts_Insert;
                DROP TRIGGER IF EXISTS FullTextIndex_ChatFacts_Delete;
                DROP TRIGGER IF EXISTS FullTextIndex_ChatFacts_Update;

                DROP TRIGGER IF EXISTS FullTextIndex_ChatDecisions_Insert;
                DROP TRIGGER IF EXISTS FullTextIndex_ChatDecisions_Delete;
                DROP TRIGGER IF EXISTS FullTextIndex_ChatDecisions_Update;

                DROP TRIGGER IF EXISTS FullTextIndex_ChatUserPreferences_Insert;
                DROP TRIGGER IF EXISTS FullTextIndex_ChatUserPreferences_Delete;
                DROP TRIGGER IF EXISTS FullTextIndex_ChatUserPreferences_Update;

                DROP INDEX IF EXISTS IX_FullTextIndex_ProjectId;
                DROP TABLE IF EXISTS FullTextIndex;
                """);
        }
    }
}
