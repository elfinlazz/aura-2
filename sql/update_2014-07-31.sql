ALTER TABLE `creatures` ADD `creationTime` DATETIME NOT NULL , ADD `lastAging` DATETIME NOT NULL , ADD `lastRebirth` DATETIME NULL DEFAULT NULL , ADD `lastLogin` DATETIME NULL DEFAULT NULL ;
UPDATE `creatures` SET `creationTime` = NOW(), `lastAging` = NOW() ;
