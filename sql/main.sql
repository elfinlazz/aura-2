CREATE DATABASE `aura` DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;
USE `aura`;

CREATE TABLE IF NOT EXISTS `accounts` (
  `accountId` varchar(50) NOT NULL,
  `password` varchar(128) NOT NULL,
  `secondaryPassword` varchar(128) DEFAULT NULL,
  `sessionKey` bigint(20) NOT NULL DEFAULT '0',
  `authority` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `creation` datetime DEFAULT NULL,
  `lastLogin` datetime DEFAULT NULL,
  `banExpiration` datetime DEFAULT NULL,
  `banReason` varchar(255) DEFAULT NULL,
  `loggedIn` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`accountId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

INSERT INTO `accounts` (`accountId`, `password`, `secondaryPassword`, `sessionKey`, `authority`, `creation`, `lastLogin`, `banExpiration`, `banReason`, `loggedIn`) VALUES
('AuraSystem', '', NULL, 0, 0, NULL, NULL, NULL, NULL, 0);

CREATE TABLE IF NOT EXISTS `cards` (
  `cardId` bigint(20) NOT NULL AUTO_INCREMENT,
  `accountId` varchar(50) NOT NULL,
  `type` int(11) NOT NULL,
  `race` int(11) NOT NULL DEFAULT '0',
  `isGift` tinyint(1) NOT NULL DEFAULT '0',
  `message` varchar(200) DEFAULT NULL,
  `sender` varchar(50) DEFAULT NULL,
  `senderServer` varchar(100) DEFAULT NULL,
  `receiver` varchar(50) DEFAULT NULL,
  `receiverServer` varchar(100) DEFAULT NULL,
  `added` datetime DEFAULT NULL,
  PRIMARY KEY (`cardId`),
  KEY `account` (`accountId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

CREATE TABLE IF NOT EXISTS `characters` (
  `entityId` bigint(20) NOT NULL AUTO_INCREMENT,
  `accountId` varchar(50) NOT NULL,
  `creatureId` bigint(20) NOT NULL,
  PRIMARY KEY (`entityId`),
  KEY `accountId` (`accountId`),
  KEY `creatureId` (`creatureId`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=4503599627370498 ;

INSERT INTO `characters` (`entityId`, `accountId`, `creatureId`) VALUES
(4503599627370497, 'AuraSystem', 1);

CREATE TABLE IF NOT EXISTS `creatures` (
  `creatureId` bigint(20) NOT NULL AUTO_INCREMENT,
  `server` varchar(50) NOT NULL,
  `name` varchar(50) NOT NULL,
  `race` int(11) NOT NULL,
  `skinColor` tinyint(3) unsigned NOT NULL,
  `eyeType` smallint(6) NOT NULL,
  `eyeColor` tinyint(3) unsigned NOT NULL,
  `mouthType` tinyint(3) unsigned NOT NULL,
  `height` float NOT NULL DEFAULT '1',
  `weight` float NOT NULL DEFAULT '1',
  `upper` float NOT NULL DEFAULT '1',
  `lower` float NOT NULL DEFAULT '1',
  `color1` int(11) unsigned NOT NULL DEFAULT '0',
  `color2` int(11) unsigned NOT NULL DEFAULT '0',
  `color3` int(11) unsigned NOT NULL DEFAULT '0',
  `region` int(11) NOT NULL DEFAULT '1',
  `x` int(11) NOT NULL DEFAULT '12800',
  `y` int(11) NOT NULL DEFAULT '38100',
  `direction` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `level` smallint(6) NOT NULL DEFAULT '1',
  `levelTotal` int(11) NOT NULL DEFAULT '0',
  `exp` bigint(20) NOT NULL DEFAULT '0',
  `ap` smallint(6) NOT NULL DEFAULT '0',
  `age` smallint(6) NOT NULL DEFAULT '1',
  `lifeMax` float NOT NULL,
  `lifeDelta` float NOT NULL DEFAULT '0',
  `injuries` float NOT NULL DEFAULT '0',
  `manaMax` float NOT NULL,
  `manaDelta` float NOT NULL DEFAULT '0',
  `staminaMax` float NOT NULL,
  `staminaDelta` float NOT NULL DEFAULT '0',
  `hunger` float NOT NULL DEFAULT '0',
  `str` float NOT NULL,
  `int` float NOT NULL,
  `dex` float NOT NULL,
  `will` float NOT NULL,
  `luck` float NOT NULL,
  `lifeFood` float NOT NULL DEFAULT '0',
  `manaFood` float NOT NULL DEFAULT '0',
  `staminaFood` float NOT NULL DEFAULT '0',
  `strFood` float NOT NULL DEFAULT '0',
  `intFood` float NOT NULL DEFAULT '0',
  `dexFood` float NOT NULL DEFAULT '0',
  `willFood` float NOT NULL DEFAULT '0',
  `luckFood` float NOT NULL DEFAULT '0',
  `defense` smallint(6) NOT NULL,
  `protection` float NOT NULL,
  `deletionTime` datetime DEFAULT NULL,
  `weaponSet` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `title` smallint(5) unsigned NOT NULL DEFAULT '0',
  `optionTitle` smallint(5) unsigned NOT NULL DEFAULT '0',
  `state` int(10) unsigned NOT NULL DEFAULT '0',
  `creationTime` datetime NOT NULL,
  `lastAging` datetime NOT NULL,
  `lastRebirth` datetime DEFAULT NULL,
  `lastLogin` datetime DEFAULT NULL,
  PRIMARY KEY (`creatureId`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=2 ;

INSERT INTO `creatures` (`creatureId`, `server`, `name`, `race`, `skinColor`, `eyeType`, `eyeColor`, `mouthType`, `height`, `weight`, `upper`, `lower`, `color1`, `color2`, `color3`, `region`, `x`, `y`, `direction`, `level`, `levelTotal`, `exp`, `ap`, `age`, `lifeMax`, `lifeDelta`, `injuries`, `manaMax`, `manaDelta`, `staminaMax`, `staminaDelta`, `hunger`, `str`, `int`, `dex`, `will`, `luck`, `lifeFood`, `manaFood`, `staminaFood`, `strFood`, `intFood`, `dexFood`, `willFood`, `luckFood`, `defense`, `protection`, `deletionTime`, `weaponSet`, `title`, `optionTitle`, `state`, `creationTime`, `lastAging`, `lastRebirth`, `lastLogin`) VALUES
(1, 'AuraSystem', '_Dummy', 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 12800, 38100, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, NULL, 0, 0, 0, 0, '0000-00-00 00:00:00', '0000-00-00 00:00:00', NULL, NULL);

CREATE TABLE IF NOT EXISTS `coupons` (
  `code` varchar(19) NOT NULL,
  `expiration` datetime DEFAULT NULL,
  `script` varchar(128) NOT NULL,
  `used` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `items` (
  `entityId` bigint(20) NOT NULL AUTO_INCREMENT,
  `creatureId` bigint(20) NOT NULL,
  `itemId` int(11) NOT NULL,
  `pocket` tinyint(3) unsigned NOT NULL,
  `x` int(11) NOT NULL DEFAULT '0',
  `y` int(11) NOT NULL DEFAULT '0',
  `color1` int(10) unsigned NOT NULL DEFAULT '0',
  `color2` int(10) unsigned NOT NULL DEFAULT '0',
  `color3` int(10) unsigned NOT NULL DEFAULT '0',
  `price` int(11) NOT NULL DEFAULT '0',
  `sellPrice` int(11) NOT NULL DEFAULT '0',
  `amount` int(11) NOT NULL DEFAULT '1',
  `linkedPocket` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `state` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `durability` int(11) NOT NULL DEFAULT '0',
  `durabilityMax` int(11) NOT NULL DEFAULT '0',
  `durabilityOriginal` int(11) NOT NULL DEFAULT '0',
  `attackMin` smallint(6) unsigned NOT NULL DEFAULT '0',
  `attackMax` smallint(6) unsigned NOT NULL DEFAULT '0',
  `balance` tinyint(4) NOT NULL DEFAULT '0',
  `critical` tinyint(4) NOT NULL DEFAULT '0',
  `defense` int(11) NOT NULL DEFAULT '0',
  `protection` smallint(6) NOT NULL DEFAULT '0',
  `range` smallint(6) NOT NULL DEFAULT '0',
  `attackSpeed` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `experience` smallint(6) NOT NULL DEFAULT '0',
  `meta1` varchar(2048) DEFAULT NULL,
  `meta2` varchar(2048) DEFAULT NULL,
  PRIMARY KEY (`entityId`),
  KEY `creatureId` (`creatureId`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=22517998136852482 ;

INSERT INTO `items` (`entityId`, `creatureId`, `itemId`, `pocket`, `x`, `y`, `color1`, `color2`, `color3`, `price`, `sellPrice`, `amount`, `linkedPocket`, `state`, `durability`, `durabilityMax`, `durabilityOriginal`, `attackMin`, `attackMax`, `balance`, `critical`, `defense`, `protection`, `range`, `attackSpeed`, `experience`, `meta1`, `meta2`) VALUES
(22517998136852481, 1, 0, 2, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, NULL, NULL);

CREATE TABLE IF NOT EXISTS `keywords` (
  `creatureId` bigint(20) NOT NULL,
  `keywordId` smallint(6) NOT NULL,
  PRIMARY KEY (`creatureId`,`keywordId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `partners` (
  `entityId` bigint(20) NOT NULL AUTO_INCREMENT,
  `accountId` varchar(50) NOT NULL,
  `creatureId` bigint(20) NOT NULL,
  PRIMARY KEY (`entityId`),
  KEY `accountId` (`accountId`),
  KEY `creatureId` (`creatureId`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=4506898162253826 ;

INSERT INTO `partners` (`entityId`, `accountId`, `creatureId`) VALUES
(4506898162253825, 'AuraSystem', 1);

CREATE TABLE IF NOT EXISTS `pets` (
  `entityId` bigint(20) NOT NULL AUTO_INCREMENT,
  `accountId` varchar(50) NOT NULL,
  `creatureId` bigint(20) NOT NULL,
  PRIMARY KEY (`entityId`),
  KEY `accountId` (`accountId`),
  KEY `creatureId` (`creatureId`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=4504699138998274 ;

INSERT INTO `pets` (`entityId`, `accountId`, `creatureId`) VALUES
(4504699138998273, 'AuraSystem', 1);

CREATE TABLE IF NOT EXISTS `quest_progress` (
  `progressId` bigint(20) NOT NULL AUTO_INCREMENT,
  `creatureId` bigint(20) NOT NULL,
  `questIdUnique` bigint(20) NOT NULL,
  `objective` varchar(50) NOT NULL,
  `count` int(11) NOT NULL,
  `done` tinyint(1) NOT NULL,
  `unlocked` tinyint(1) NOT NULL,
  PRIMARY KEY (`progressId`),
  KEY `creatureId` (`creatureId`,`questIdUnique`),
  KEY `questIdUnique` (`questIdUnique`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

CREATE TABLE IF NOT EXISTS `quests` (
  `questIdUnique` bigint(20) NOT NULL AUTO_INCREMENT,
  `creatureId` bigint(20) NOT NULL,
  `questId` int(11) NOT NULL,
  `state` int(11) NOT NULL,
  `itemEntityId` bigint(20) NOT NULL,
  PRIMARY KEY (`questIdUnique`),
  KEY `creatureId` (`creatureId`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=27022628556374018 ;

INSERT INTO `quests` (`questIdUnique`, `creatureId`, `questId`, `state`, `itemEntityId`) VALUES
(27022628556374017, 1, 0, 1, 0);

CREATE TABLE IF NOT EXISTS `updates` (
  `path` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

INSERT INTO `updates` (`path`) VALUES
('main.sql'),
('update_2014-02-26.sql'),
('update_2014-02-27.sql'),
('update_2014-07-26.sql'),
('update_2014-07-27.sql'),
('update_2014-07-28.sql'),
('update_2014-07-31.sql');

ALTER TABLE `updates`
 ADD PRIMARY KEY (`path`);
 
CREATE TABLE IF NOT EXISTS `skills` (
  `skillId` smallint(5) unsigned NOT NULL,
  `creatureId` bigint(20) NOT NULL,
  `rank` tinyint(3) unsigned NOT NULL,
  `condition1` smallint(5) NOT NULL DEFAULT '0',
  `condition2` smallint(5) NOT NULL DEFAULT '0',
  `condition3` smallint(5) NOT NULL DEFAULT '0',
  `condition4` smallint(5) NOT NULL DEFAULT '0',
  `condition5` smallint(5) NOT NULL DEFAULT '0',
  `condition6` smallint(5) NOT NULL DEFAULT '0',
  `condition7` smallint(5) NOT NULL DEFAULT '0',
  `condition8` smallint(5) NOT NULL DEFAULT '0',
  `condition9` smallint(5) NOT NULL DEFAULT '0',
  PRIMARY KEY (`skillId`,`creatureId`),
  KEY `creatureId` (`creatureId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `titles` (
  `creatureId` bigint(20) NOT NULL,
  `titleId` smallint(5) unsigned NOT NULL,
  `usable` tinyint(1) NOT NULL,
  PRIMARY KEY (`creatureId`,`titleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `vars` (
  `accountId` varchar(50) NOT NULL,
  `creatureId` bigint(20) NOT NULL,
  `name` varchar(64) NOT NULL,
  `type` char(2) NOT NULL,
  `value` mediumtext NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


ALTER TABLE `cards`
  ADD CONSTRAINT `cards_ibfk_2` FOREIGN KEY (`accountId`) REFERENCES `accounts` (`accountId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `cards_ibfk_1` FOREIGN KEY (`accountId`) REFERENCES `accounts` (`accountId`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `characters`
  ADD CONSTRAINT `characters_ibfk_3` FOREIGN KEY (`accountId`) REFERENCES `accounts` (`accountId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `characters_ibfk_4` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `characters_ibfk_1` FOREIGN KEY (`accountId`) REFERENCES `accounts` (`accountId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `characters_ibfk_2` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `items`
  ADD CONSTRAINT `items_ibfk_2` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `items_ibfk_1` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `keywords`
  ADD CONSTRAINT `keywords_ibfk_2` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `keywords_ibfk_1` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `partners`
  ADD CONSTRAINT `partners_ibfk_3` FOREIGN KEY (`accountId`) REFERENCES `accounts` (`accountId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `partners_ibfk_4` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `partners_ibfk_1` FOREIGN KEY (`accountId`) REFERENCES `accounts` (`accountId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `partners_ibfk_2` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `pets`
  ADD CONSTRAINT `pets_ibfk_3` FOREIGN KEY (`accountId`) REFERENCES `accounts` (`accountId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `pets_ibfk_4` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `pets_ibfk_1` FOREIGN KEY (`accountId`) REFERENCES `accounts` (`accountId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `pets_ibfk_2` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `quest_progress`
  ADD CONSTRAINT `quest_progress_ibfk_2` FOREIGN KEY (`questIdUnique`) REFERENCES `quests` (`questIdUnique`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `quest_progress_ibfk_1` FOREIGN KEY (`questIdUnique`) REFERENCES `quests` (`questIdUnique`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `quests`
  ADD CONSTRAINT `quests_ibfk_2` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `quests_ibfk_1` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `skills`
  ADD CONSTRAINT `skills_ibfk_2` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `skills_ibfk_1` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `titles`
  ADD CONSTRAINT `titles_ibfk_2` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `titles_ibfk_1` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE;
