ALTER TABLE `accounts` ADD `autobanCount` INT(10) NOT NULL DEFAULT '0' , ADD `autobanScore` INT(10) NOT NULL DEFAULT '0' , ADD `lastAutobanReduction` DATETIME NULL DEFAULT NULL ;

CREATE TABLE IF NOT EXISTS `log_security` (
  `id` bigint(20) unsigned NOT NULL,
  `accountId` varchar(50) NOT NULL,
  `characterId` bigint(20) DEFAULT NULL,
  `ipAddress` varchar(45) NOT NULL,
  `date` datetime NOT NULL,
  `level` int(10) NOT NULL,
  `report` varchar(500) NOT NULL,
  `stacktrace` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

ALTER TABLE `log_security`
 ADD PRIMARY KEY (`id`), ADD KEY `accountId` (`accountId`), ADD KEY `characterId` (`characterId`),
 MODIFY `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT;

ALTER TABLE `log_security`
ADD CONSTRAINT `log_security_ibfk_1` FOREIGN KEY (`accountId`) REFERENCES `accounts` (`accountId`) ON DELETE CASCADE ON UPDATE CASCADE;
