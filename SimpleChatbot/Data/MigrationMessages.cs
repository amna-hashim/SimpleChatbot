using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleChatbot.Infrastructure
{
    public partial class AddEmbeddingColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            ALTER TABLE Messages ADD Embedding VECTOR(1536) NULL;
        ");

            migrationBuilder.Sql(@"
            CREATE VECTOR INDEX VIX_Messages_Embedding
            ON Messages(Embedding)
            WITH (METRIC = 'cosine', TYPE = 'diskann');
        ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX VIX_Messages_Embedding ON Messages;");
            migrationBuilder.Sql("ALTER TABLE Messages DROP COLUMN Embedding;");
        }
    }
}
