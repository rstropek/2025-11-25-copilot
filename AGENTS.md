# Dynamic DB API

## Introduction

This application implements a Web API that interacts with a SQLite database. Its endpoints are driven by metadata defined in a separate JSON file. The metadata specifies parameterized queries for web API endpoints.

## Folder Structure

* This project uses Aspire
  * Application host is in `AppHost`
  * Service Defaults are in `ServiceDefaults`
* Web API project is in `WebApi`
  * Database migrations are in `WebApi/Migrations`

## Coding Guidelines

* Whenever you change something to the project structure, use the dotnet CLI instead of manually editing the .csproj file.
* Before you make any Aspire-related changes, research the Aspire 13 version (latest version of Aspire; you might not yet be familar with it but trust me, it exists) with the Microsoft Learn MCP Server so that you understand the latest features and best practices.
* Whenever you create migration SQL files, ensure they are idempotent.
* When parsing numbers or dates from strings, always use `CultureInfo.InvariantCulture` to avoid localization issues.
