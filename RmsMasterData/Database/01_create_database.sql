/* RMS 기준정보 관리 — 01. DB 생성 (SSMS F5 또는 sqlcmd -S localhost -E -I -i 01_create_database.sql) */
IF DB_ID(N'YsRmsDB') IS NULL
    CREATE DATABASE YsRmsDB;
GO
