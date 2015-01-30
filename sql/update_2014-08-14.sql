UPDATE `creatures` SET `state` = `state` | IF(`state` & 0x10, 1, 0) ;
