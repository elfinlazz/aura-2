CREATE TABLE IF NOT EXISTS `updates` (
  `path` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

INSERT INTO `updates` (`path`) VALUES
('main.sql'),
('update_2014-02-26.sql'),
('update_2014-02-27.sql'),
('update_2014-07-26.sql');

ALTER TABLE `updates`
 ADD PRIMARY KEY (`path`);
