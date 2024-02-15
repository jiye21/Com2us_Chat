CREATE DATABASE IF NOT EXISTS AccountDB;

USE AccountDB;

CREATE TABLE IF NOT EXISTS AccountDB.`Account`
(
    Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '계정번호',
    LoginId VARCHAR(50) NOT NULL UNIQUE COMMENT '계정',
    SaltValue VARCHAR(100) NOT NULL COMMENT  '?�호??�?,
    HashedPassword VARCHAR(100) NOT NULL COMMENT '?�싱??비�?번호',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP  COMMENT '?�성 ?�짜'
) COMMENT '계정 ?�보 ?�이�?;


INSERT INTO AccountDB.`Account`(LoginId, SaltValue, HashedPassword)
VALUES('user1', 1234, 1234);

INSERT INTO AccountDB.`Account`(LoginId, SaltValue, HashedPassword)
VALUES('user2', 1234, 1234);

INSERT INTO AccountDB.`Account`(LoginId, SaltValue, HashedPassword)
VALUES('user3', 1234, 1234);