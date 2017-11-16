using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Database.Migrations
{
    public partial class update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SysActions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    ActionDisplayName = table.Column<string>(maxLength: 40, nullable: false),
                    ActionName = table.Column<string>(maxLength: 40, nullable: false),
                    ActionType = table.Column<int>(nullable: false),
                    ButtonIcon = table.Column<string>(maxLength: 50, nullable: true),
                    ButtonStyle = table.Column<string>(maxLength: 50, nullable: true),
                    ButtonType = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    SystemId = table.Column<string>(maxLength: 50, nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SysAreas",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    AreaDisplayName = table.Column<string>(nullable: true),
                    AreaName = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    Display = table.Column<bool>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    NeedAuth = table.Column<bool>(nullable: false),
                    SortWeight = table.Column<int>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SysRoles",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    RoleName = table.Column<string>(maxLength: 50, nullable: false),
                    SystemId = table.Column<string>(maxLength: 50, nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SysUserInfos",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    Enable = table.Column<bool>(nullable: false),
                    HeadIcon = table.Column<string>(nullable: true),
                    LastLoginTime = table.Column<DateTime>(nullable: true),
                    Login = table.Column<string>(maxLength: 30, nullable: true),
                    Password = table.Column<string>(maxLength: 256, nullable: true),
                    RealName = table.Column<string>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UserName = table.Column<string>(nullable: true),
                    sex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysUserInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SysControllers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    ActionName = table.Column<string>(maxLength: 50, nullable: true),
                    ControllerDisplayName = table.Column<string>(maxLength: 50, nullable: false),
                    ControllerName = table.Column<string>(maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    Display = table.Column<bool>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    Ico = table.Column<string>(nullable: true),
                    Parameter = table.Column<string>(maxLength: 50, nullable: true),
                    SysAreaId = table.Column<long>(nullable: true),
                    SystemId = table.Column<string>(maxLength: 50, nullable: false),
                    TargetBlank = table.Column<bool>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysControllers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysControllers_SysAreas_SysAreaId",
                        column: x => x.SysAreaId,
                        principalTable: "SysAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SysRoleSysUserInfo",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    SysRoleId = table.Column<long>(nullable: false),
                    SysUserId = table.Column<long>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysRoleSysUserInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysRoleSysUserInfo_SysRoles_SysRoleId",
                        column: x => x.SysRoleId,
                        principalTable: "SysRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SysRoleSysUserInfo_SysUserInfos_SysUserId",
                        column: x => x.SysUserId,
                        principalTable: "SysUserInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SysControllerSysActions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    SysActionId = table.Column<long>(nullable: false),
                    SysControllerId = table.Column<long>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysControllerSysActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysControllerSysActions_SysActions_SysActionId",
                        column: x => x.SysActionId,
                        principalTable: "SysActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SysControllerSysActions_SysControllers_SysControllerId",
                        column: x => x.SysControllerId,
                        principalTable: "SysControllers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SysRoleSysControllerSysAction",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    SysControllerSysActionId = table.Column<long>(nullable: false),
                    SysRoleId = table.Column<long>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysRoleSysControllerSysAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysRoleSysControllerSysAction_SysControllerSysActions_SysControllerSysActionId",
                        column: x => x.SysControllerSysActionId,
                        principalTable: "SysControllerSysActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SysRoleSysControllerSysAction_SysRoles_SysRoleId",
                        column: x => x.SysRoleId,
                        principalTable: "SysRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SysControllers_SysAreaId",
                table: "SysControllers",
                column: "SysAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_SysControllerSysActions_SysActionId",
                table: "SysControllerSysActions",
                column: "SysActionId");

            migrationBuilder.CreateIndex(
                name: "IX_SysControllerSysActions_SysControllerId",
                table: "SysControllerSysActions",
                column: "SysControllerId");

            migrationBuilder.CreateIndex(
                name: "IX_SysRoleSysControllerSysAction_SysControllerSysActionId",
                table: "SysRoleSysControllerSysAction",
                column: "SysControllerSysActionId");

            migrationBuilder.CreateIndex(
                name: "IX_SysRoleSysControllerSysAction_SysRoleId",
                table: "SysRoleSysControllerSysAction",
                column: "SysRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SysRoleSysUserInfo_SysRoleId",
                table: "SysRoleSysUserInfo",
                column: "SysRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SysRoleSysUserInfo_SysUserId",
                table: "SysRoleSysUserInfo",
                column: "SysUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SysRoleSysControllerSysAction");

            migrationBuilder.DropTable(
                name: "SysRoleSysUserInfo");

            migrationBuilder.DropTable(
                name: "SysControllerSysActions");

            migrationBuilder.DropTable(
                name: "SysRoles");

            migrationBuilder.DropTable(
                name: "SysUserInfos");

            migrationBuilder.DropTable(
                name: "SysActions");

            migrationBuilder.DropTable(
                name: "SysControllers");

            migrationBuilder.DropTable(
                name: "SysAreas");
        }
    }
}
