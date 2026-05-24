using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace catalog_service.Migrations
{
    /// <inheritdoc />
    public partial class CatalogMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "channels",
                columns: table => new
                {
                    channel_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    channel_name = table.Column<string>(type: "varchar(100)", nullable: false),
                    channel_description = table.Column<string>(type: "varchar(2000)", nullable: true),
                    channel_profile_url = table.Column<string>(type: "varchar(2000)", nullable: true),
                    channel_status = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "1"),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    modified_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channels", x => x.channel_id);
                });

            migrationBuilder.CreateTable(
                name: "videos",
                columns: table => new
                {
                    video_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    channel_id = table.Column<int>(type: "int", nullable: false),
                    video_title = table.Column<string>(type: "varchar(100)", nullable: false),
                    video_description = table.Column<string>(type: "varchar(2000)", nullable: true),
                    video_thumbnail_url = table.Column<string>(type: "varchar(2000)", nullable: true),
                    video_status = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0"),
                    video_size = table.Column<long>(type: "bigint", nullable: false),
                    video_duration_seconds = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    modified_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_videos", x => x.video_id);
                    table.ForeignKey(
                        name: "FK_videos_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "channels",
                        principalColumn: "channel_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "video_assets",
                columns: table => new
                {
                    video_asset_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    video_id = table.Column<int>(type: "int", nullable: false),
                    video_quality = table.Column<int>(type: "int", nullable: false),
                    manifest_url = table.Column<string>(type: "varchar(2000)", nullable: false),
                    video_asset_size = table.Column<long>(type: "bigint", nullable: false),
                    video_asset_bitrate = table.Column<int>(type: "int", nullable: false),
                    video_asset_status = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0"),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_video_assets", x => x.video_asset_id);
                    table.ForeignKey(
                        name: "FK_video_assets_videos_video_id",
                        column: x => x.video_id,
                        principalTable: "videos",
                        principalColumn: "video_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_channels_channel_name",
                table: "channels",
                column: "channel_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_channels_user_id",
                table: "channels",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_video_assets_video_id",
                table: "video_assets",
                column: "video_id");

            migrationBuilder.CreateIndex(
                name: "IX_videos_channel_id",
                table: "videos",
                column: "channel_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "video_assets");

            migrationBuilder.DropTable(
                name: "videos");

            migrationBuilder.DropTable(
                name: "channels");
        }
    }
}
