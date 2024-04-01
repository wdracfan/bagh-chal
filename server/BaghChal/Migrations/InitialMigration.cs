using FluentMigrator;

namespace Migrations;

[Migration(1)]
public class InitialMigration : Migration
{
    public override void Up()
    {
        /*
        -- auto-generated definition
        create database baghchal
            with owner postgres;

        create schema if not exists game;

        create table if not exists game.boards (
            board_guid text primary key,
            host_guid text,
            guest_guid text
        );

        create table if not exists game.users (
            user_guid text primary key,
            username text
        );
        */
        Create.Schema("game");

        Create.Table("boards").InSchema("game")
            .WithColumn("board_guid").AsString().PrimaryKey()
            .WithColumn("host_guid").AsString().Nullable()
            .WithColumn("guest_guid").AsString().Nullable()
            .WithColumn("piece").AsString();

        Create.Table("users").InSchema("game")
            .WithColumn("user_guid").AsString().PrimaryKey()
            .WithColumn("username").AsString().Nullable();
    }

    public override void Down()
    {
        Delete.Table("users").InSchema("game");
        Delete.Table("boards").InSchema("game");
        Delete.Schema("game");
        
        //Delete.Table("VersionInfo").InSchema("public");
    }

}