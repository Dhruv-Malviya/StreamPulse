using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace catalog_service.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Video_VideoStatus",
                table: "videos",
                sql: "video_status >= 0 AND video_status <= 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VideoAsset_VideoAssetStatus",
                table: "video_assets",
                sql: "video_asset_status >= 0 AND video_asset_status <= 3");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Channel_ChannelStatus",
                table: "channels",
                sql: "channel_status >= 0 AND channel_status <= 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Video_VideoStatus",
                table: "videos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VideoAsset_VideoAssetStatus",
                table: "video_assets");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Channel_ChannelStatus",
                table: "channels");
        }
    }
}
