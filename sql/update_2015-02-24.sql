CREATE TABLE IF NOT EXISTS `ptj` (
  `creatureId` bigint(20) NOT NULL,
  `type` int(11) NOT NULL,
  `done` int(11) NOT NULL,
  `success` int(11) NOT NULL,
  `lastChange` datetime NOT NULL,
  KEY `creatureId` (`creatureId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

ALTER TABLE `ptj`
  ADD CONSTRAINT `ptj_ibfk_1` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE;
