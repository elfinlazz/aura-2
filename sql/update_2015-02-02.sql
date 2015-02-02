ALTER TABLE `creatures` CHANGE `levelTotal` `levelTotal` INT(11) NOT NULL DEFAULT '1';
UPDATE `creatures` SET `levelTotal` = 1 WHERE `levelTotal` = 0;
