ALTER TABLE `accounts` ADD `autobanCount` INT(10) NOT NULL DEFAULT '0' , ADD `autobanScore` INT(10) NOT NULL DEFAULT '0' , ADD `lastAutobanReduction` DATETIME NULL DEFAULT NULL ;

CREATE TABLE IF NOT EXISTS `log_autoban` (
`id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `accountId` varchar(50) NOT NULL,
  `characterId` bigint(20) DEFAULT NULL,
  `date` datetime NOT NULL,
  `level` int(10) NOT NULL,
  `report` varchar(500) NOT NULL,
  `stacktrace` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;


ALTER TABLE `log_autoban`
 ADD PRIMARY KEY (`id`), ADD KEY `accountId` (`accountId`), ADD KEY `characterId` (`characterId`);

ALTER TABLE `log_autoban`
ADD CONSTRAINT `log_autoban_ibfk_1` FOREIGN KEY (`accountId`) REFERENCES `accounts` (`accountId`) ON DELETE CASCADE ON UPDATE CASCADE;