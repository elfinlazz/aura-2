<?php
// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder
// 
// This is a simple upload script that takes the XML file the client
// sends for saving hotkey and window settings. For retrieving the data
// it tries to access the files directly.
// --------------------------------------------------------------------------

if(empty($_POST) || empty($_FILES) || !isset($_FILES['ui']))
	exit;

// Only allow plain text files named '################.xml'
if(!preg_match('~[0-9]{16}.xml~', $_FILES['ui']['name']) || $_FILES['ui']['type'] !== 'text/plain')
	exit;

$charId = $_POST['char_id'];
$server = $_POST['name_server'];

// TODO: Safety checks

// Check paramters
if(!preg_match('~^[0-9]{16}$~', $charId) || !preg_match('~^[0-9a-z_ ]+$~i', $server))
	exit;

// Last 3 numbers of the id make up the name of the group folder
$key = substr($charId, -3);

// Create folder structure
$folder = 'storage/';
if(!is_dir($folder) && (!mkdir($folder) || !touch($folder . 'index.php')))
	exit;
$folder .= $server . '/';
if(!is_dir($folder) && (!mkdir($folder) || !touch($folder . 'index.php')))
	exit;
$folder .= $key . '/';
if(!is_dir($folder) && (!mkdir($folder) || !touch($folder . 'index.php')))
	exit;

// Try to move file
$filepath = $folder . $charId . '.xml';
if(!@move_uploaded_file($_FILES['ui']['tmp_name'], $filepath))
	exit;

chmod($filepath, 0644);
