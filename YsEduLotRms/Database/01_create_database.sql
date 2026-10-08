/* ============================================================================
   2차 시스템 구축 과제 — 생산 이력 추적 및 RMS Recipe 검증
   01. 데이터베이스 생성
   실행: SSMS에서 localhost 접속 후 F5 (또는 sqlcmd -S localhost -E -i 01_create_database.sql)
   ============================================================================ */
IF DB_ID(N'YsEduLotDB') IS NULL
    CREATE DATABASE YsEduLotDB;
GO
