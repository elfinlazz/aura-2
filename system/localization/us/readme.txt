// Aura
// Documentation
// --------------------------------------------------------------------------

Localization in Aura is done through a "gettext"-like system, which is
commonly used for this type of work. The server reads all ".po" files found
in the localization folder specified in the configuration and uses the
translated lines. If a line is not translated it uses the original, English
version instead.

Since we frequently add and change lines in the core and NPCs we don't
put prepared ".po" files into the core ourselves. But you can generate
them from the source files yourself, using various tools.
We recommand the free "Poedit": http://poedit.net/

Here's a short description of how to create a new translation using Poedit:

* Open Poedit

Preparation (this is a one time thing after installing Poedit):

* Click "File > Preferences..."
* Switch to "Extractors"
* Select "C#" and click "Edit"
* In the input field "Command to extract translations:" add the following
  at the end: -k
  It should look something like this now:
  xgettext --language=C# --add-comments=TRANSLATORS: [...] %C %K %F -k
* Close both dialogs with "OK" and continue.

Creating a new translation:

* Click "File > New..."
* Select a language (which one is unimportant for Aura)
* Save file, for example inside "user/localization/de/" for a German
  translation. You can name the file whatever you want, Aura reads all po
  files found in the folder.
* Click "Extract from sources"
* In the first list click the "New Item" button and enter the path to your
  Aura folder.
* Switch to the "Translation properties" and enter any project name and
  select "UTF-8" as source code charset.
* Switch to the "Source keywords" and add the following new items,
  without quotation:
  "Localization.Get"
  "Msg:1"
  "Msg:2"
  "Msg:3"
  "Msg:4"
  "Button:1"
  "Intro:1"
  "AddPhrase:1"
  "L"
  These are the function names the program will look for to get strings.
  Localization.Get is usually used inside the core, e.g. for the Eweca
  messages. The other methods are used in scripts for various things.
  You can also create multiple po files and group those a little,
  e.g. make one file for the core, where you only look for Localization.Get.
* Poedit will now search all source files and NPCs for translatable texts.
* Once it's done searching you can start translating, by selecting a line
  and adding the translation at the bottom.
* Finally save the file and you're done, after changing the localization to
  load "de", if you've put the file(s) in a folder called "de", it should use
  your strings instead of the English ones. Naturally you can also put them in
  a "us" folder, to simply replace default phrases, without changing the
  language.

After creating the ".po" file once you can simply open it with Poedit again,
fix translations, or update the catalogue, which makes it read the source
again, adding new lines to it for you to translate while keeping what you've
done so far.
