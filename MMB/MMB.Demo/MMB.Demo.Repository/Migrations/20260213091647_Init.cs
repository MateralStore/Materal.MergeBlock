using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMB.Demo.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MyCategory",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false, comment: ""),
                    ParentID = table.Column<Guid>(type: "TEXT", nullable: true, comment: ""),
                    Index = table.Column<int>(type: "INTEGER", nullable: false, comment: ""),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyCategory", x => x.ID);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "MyList",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false, comment: ""),
                    Index = table.Column<int>(type: "INTEGER", nullable: false, comment: ""),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyList", x => x.ID);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "MyTree",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false, comment: ""),
                    ParentID = table.Column<Guid>(type: "TEXT", nullable: true, comment: ""),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyTree", x => x.ID);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, comment: "姓名"),
                    Role = table.Column<int>(type: "INTEGER", nullable: false, comment: "角色"),
                    Account = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, comment: "账号"),
                    Password = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, comment: "密码"),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.ID);
                },
                comment: "用户");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MyCategory");

            migrationBuilder.DropTable(
                name: "MyList");

            migrationBuilder.DropTable(
                name: "MyTree");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
