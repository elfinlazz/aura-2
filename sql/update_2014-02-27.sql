CREATE TABLE IF NOT EXISTS `coupons` (
  `code` varchar(19) NOT NULL,
  `expiration` datetime DEFAULT NULL,
  `script` varchar(128) NOT NULL,
  `used` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
