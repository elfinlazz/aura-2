UPDATE `creatures` SET `levelTotal` = `level` WHERE `rebirthCount` = 0;
UPDATE `creatures` SET `levelTotal` = `levelTotal` + `level` - 1 WHERE NOT `rebirthCount` = 0;
