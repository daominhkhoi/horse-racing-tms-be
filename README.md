# 🏇 Horse Racing Tournament Management System API

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](#)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue.svg)](#)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Swagger UI](https://img.shields.io/badge/Swagger-Integrated-85EA2D.svg)](#)

![Swagger UI Login and Register Demo](https://github.com/daominhkhoi/horse-racing-tms-be/blob/main/screen-shots/Screenshot%202026-05-29%20212116.png)

A robust, scalable Web API for managing horse racing tournaments, built with a clean **Module-based** architecture. The project is currently focused on delivering secure, enterprise-grade account management and system security.

## 🚀 Why This Architecture?

Instead of a traditional monolithic MVC structure, this API leverages a **Module-based approach**. This ensures that as the system scales to handle tournaments, bets, and schedules, the codebase remains highly maintainable, decoupled, and easy to navigate.

## 🛠 Tech Stack

- **Framework:** .NET (ASP.NET Core Web API)
- **Language:** C#
- **Database:** SQL Server
- **ORM:** Entity Framework Core (Code-First Migration)
- **Authentication & Security:** \* **JWT (JSON Web Token)** for stateless Authentication & Authorization.
  - **BCrypt.Net-Next** for state-of-the-art password hashing.

## ✨ Key Features

- **Secure User Authentication:** Fully functional Registration and Login flows.
- **Absolute Data Security:** Passwords are never stored in plain-text, utilizing BCrypt hashing algorithms to ensure data integrity.
- **Stateless Sessions:** Issues secure JWTs with a 2-hour lifespan for valid login sessions.
- **Strict Role-Based Access Control (RBAC):** The system implements 5 distinct, isolated user roles to manage permissions seamlessly:
  1. `Admin` (System Administrator)
  2. `HorseOwner` (Horse Owner)
  3. `Jockey` (Rider)
  4. `Referee` (Race Official)
  5. `Spectator` (General Audience)
- **Interactive API Documentation:** Out-of-the-box Swagger UI integration for easy endpoint testing and exploration.

## 📂 Core Directory Structure

To maintain a clean architecture, the project is organized into self-contained modules rather than standard MVC folders:

```text
HorseRacingTournamentManagementSystem/
├── Modules/
│   └── Auth/
│       ├── Controllers/      # API Request Handlers (e.g., AuthController)
│       ├── DTOs/             # Data Transfer Objects (e.g., LoginDto, RegisterDto)
│       └── Entities/         # Database Models (e.g., User, Role, DbContext)
├── appsettings.json          # Configuration, Connection Strings, and JWT Secret Key
└── Program.cs                # Dependency Injection, JWT Configuration, and Middleware setup
```
## How to add a table to a database
![How to add a table to a database](https://github.com/daominhkhoi/horse-racing-tms-be/blob/main/screen-shots/Screenshot%202026-06-01%20231854.png)
