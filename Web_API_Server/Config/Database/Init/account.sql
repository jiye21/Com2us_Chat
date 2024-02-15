CREATE DATABASE IF NOT EXISTS AccountDB;

USE AccountDB;

CREATE TABLE IF NOT EXISTS AccountDB.`Account`
(
    Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT 'Í≥ÑÏ†ïÎ≤àÌò∏',
    LoginId VARCHAR(50) NOT NULL UNIQUE COMMENT 'Í≥ÑÏ†ï',
    SaltValue VARCHAR(100) NOT NULL COMMENT  '?îÌò∏??Í∞?,
    HashedPassword VARCHAR(100) NOT NULL COMMENT '?¥Ïã±??ÎπÑÎ?Î≤àÌò∏',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP  COMMENT '?ùÏÑ± ?†Ïßú'
) COMMENT 'Í≥ÑÏ†ï ?ïÎ≥¥ ?åÏù¥Î∏?;


INSERT INTO AccountDB.`Account`(LoginId, SaltValue, HashedPassword)
VALUES('user1', 1234, 1234);

INSERT INTO AccountDB.`Account`(LoginId, SaltValue, HashedPassword)
VALUES('user2', 1234, 1234);

INSERT INTO AccountDB.`Account`(LoginId, SaltValue, HashedPassword)
VALUES('user3', 1234, 1234);