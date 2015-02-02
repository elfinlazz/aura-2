ALTER TABLE `items` ADD `figureB` TINYINT UNSIGNED NOT NULL DEFAULT '0' AFTER `state`;

CREATE TABLE IF NOT EXISTS `egos` (
  `itemEntityId` bigint(20) NOT NULL,
  `egoRace` tinyint(3) unsigned NOT NULL,
  `name` varchar(255) NOT NULL,
  `strLevel` tinyint(3) unsigned NOT NULL,
  `strExp` int(11) NOT NULL,
  `intLevel` tinyint(3) unsigned NOT NULL,
  `intExp` int(11) NOT NULL,
  `dexLevel` tinyint(3) unsigned NOT NULL,
  `dexExp` int(11) NOT NULL,
  `willLevel` tinyint(3) unsigned NOT NULL,
  `willExp` int(11) NOT NULL,
  `luckLevel` tinyint(3) unsigned NOT NULL,
  `luckExp` int(11) NOT NULL,
  `socialLevel` tinyint(3) unsigned NOT NULL,
  `socialExp` int(11) NOT NULL,
  `awakeningEnergy` tinyint(3) unsigned NOT NULL,
  `awakeningExp` int(11) NOT NULL,
  `lastFeeding` datetime NOT NULL,
  PRIMARY KEY (`itemEntityId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

ALTER TABLE `egos`
  ADD CONSTRAINT `egos_ibfk_1` FOREIGN KEY (`itemEntityId`) REFERENCES `items` (`entityId`) ON DELETE CASCADE ON UPDATE CASCADE;
