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
                    ChatId     INTEGER
                );

                CREATE INDEX IX_FullTextIndex_ProjectId
                    ON FullTextIndex (ProjectId);

                CREATE INDEX IX_FullTextIndex_ChatId
                    ON FullTextIndex (ChatId);

                -- DocumentChunks
                CREATE TRIGGER FullTextIndex_DocumentChunks_Insert
                    AFTER INSERT
                    ON DocumentChunks
                BEGIN
                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId, ChatId)
                    SELECT NEW.Content,
                           1,
                           NEW.Id,
                           D.ProjectId,
                           D.ChatId
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

                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId, ChatId)
                    SELECT NEW.Content,
                           1,
                           NEW.Id,
                           D.ProjectId,
                           D.ChatId
                    FROM DocumentSections DS
                             JOIN Documents D ON D.Id = DS.DocumentId
                    WHERE DS.Id = NEW.DocumentSectionId;
                END;

                -- ChatSummary
                CREATE TRIGGER FullTextIndex_Chats_Insert
                    AFTER INSERT
                    ON Chats
                BEGIN
                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId, ChatId)
                    SELECT NEW.Summary, 2, NEW.Id, NEW.ProjectId, NULL
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

                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId, ChatId)
                    SELECT NEW.Summary, 2, NEW.Id, NEW.ProjectId, NULL
                    WHERE NEW.Summary IS NOT NULL;
                END;

                -- ChatFact
                CREATE TRIGGER FullTextIndex_ChatFacts_Insert
                    AFTER INSERT
                    ON ChatFacts
                BEGIN
                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId, ChatId)
                    SELECT NEW.Fact,
                           4,
                           NEW.Id,
                           C.ProjectId,
                           NULL
                    FROM Chats C
                    WHERE C.Id = NEW.ChatId;
                END;
                CREATE TRIGGER FullTextIndex_ChatFacts_Delete
                    AFTER DELETE
                    ON ChatFacts
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 4
                      AND SourceId = OLD.Id;
                END;
                CREATE TRIGGER FullTextIndex_ChatFacts_Update
                    AFTER UPDATE
                    ON ChatFacts
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 4
                      AND SourceId = OLD.Id;

                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId, ChatId)
                    SELECT NEW.Fact,
                           4,
                           NEW.Id,
                           C.ProjectId,
                           NULL
                    FROM Chats C
                    WHERE C.Id = NEW.ChatId;
                END;

                -- ChatDecision
                CREATE TRIGGER FullTextIndex_ChatDecisions_Insert
                    AFTER INSERT
                    ON ChatDecisions
                BEGIN
                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId, ChatId)
                    SELECT 'Decision: ' || NEW.Decision || ' Reason: ' || NEW.Reason,
                           8,
                           NEW.Id,
                           C.ProjectId,
                           NULL
                    FROM Chats C
                    WHERE C.Id = NEW.ChatId;
                END;
                CREATE TRIGGER FullTextIndex_ChatDecisions_Delete
                    AFTER DELETE
                    ON ChatDecisions
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 8
                      AND SourceId = OLD.Id;
                END;
                CREATE TRIGGER FullTextIndex_ChatDecisions_Update
                    AFTER UPDATE
                    ON ChatDecisions
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 8
                      AND SourceId = OLD.Id;

                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId, ChatId)
                    SELECT 'Decision: ' || NEW.Decision || ' Reason: ' || NEW.Reason,
                           8,
                           NEW.Id,
                           C.ProjectId,
                           NULL
                    FROM Chats C
                    WHERE C.Id = NEW.ChatId;
                END;

                -- ChatUserPreference
                CREATE TRIGGER FullTextIndex_ChatUserPreferences_Insert
                    AFTER INSERT
                    ON ChatUserPreferences
                BEGIN
                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId, ChatId)
                    SELECT NEW.Preference,
                           16,
                           NEW.Id,
                           C.ProjectId,
                           NULL
                    FROM Chats C
                    WHERE C.Id = NEW.ChatId;
                END;
                CREATE TRIGGER FullTextIndex_ChatUserPreferences_Delete
                    AFTER DELETE
                    ON ChatUserPreferences
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 16
                      AND SourceId = OLD.Id;
                END;
                CREATE TRIGGER FullTextIndex_ChatUserPreferences_Update
                    AFTER UPDATE
                    ON ChatUserPreferences
                BEGIN
                    DELETE
                    FROM FullTextIndex
                    WHERE SourceType = 16
                      AND SourceId = OLD.Id;

                    INSERT INTO FullTextIndex(Content, SourceType, SourceId, ProjectId, ChatId)
                    SELECT NEW.Preference,
                           16,
                           NEW.Id,
                           C.ProjectId,
                           NULL
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
                DROP INDEX IF EXISTS IX_FullTextIndex_ChatIdId;
                DROP TABLE IF EXISTS FullTextIndex;
                """);
        }
    }
}
