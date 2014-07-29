Aura
==============================

Aura is an open-source server emulator written in C#. It's solely being
developed for educational purposes, learning about programming, MMORPGs,
maintaining huge projects, working with other people, and improving knowledge.
It's not about playing a game or competing with any services provided by
Nexon or its partners, and we don't endorse such actions.

Aura is completely free and licensed under the GNU GPL.
As such, every user is free to use Aura and choose how to use it.

Compatibility
------------------------------
Compatibility to all major versions but NA has been dropped on
[2013-09-13 (Aura Legacy)](https://github.com/aura-project/aura_legacy/commit/c6483faace4d79b8f772bee519531718084a243d). Since then Aura is kept compatible only to the latest update of NA.

Requirements
------------------------------
To *run* Aura you need
* .NET 4.5 (Mono 3.0+)
* MySQL 5 compatible database

To *compile* it you need
* Visual Studio 2012+ or any other compiler supporting C# 5

Installation
------------------------------
* Compile Aura
* Run `sql/main.sql` to setup the database
* Copy `system/conf/database.conf` to `user/conf/`,
  adjust the necessary values and remove the rest.

Afterwards you should be able to start Aura via the provided scripts or
directly from the bin directories. If not, or if you need a more detailed guide,
head over to our forums or the wiki.

Contribution
------------------------------
There are 4 ways *you* can help us to improve Aura.

1. Research
2. Bug reports
3. Pull Requests
4. Releases on the forums

### 1. Research
Do research on NPCs, quests, skills, anything really that isn't implemented yet and
post it on our [research forum](http://dev.mabinoger.com/forum/index.php/forum/33-research/).
The information you post will help developers to implement the features. 

### 2. Bug reports
Report bugs on [GitHub](https://github.com/aura-project/aura/issues) or
the [forum](http://dev.mabinoger.com/forum/index.php/forum/39-bugs/),
so they can be fixed asap.

### 3+4. Code
The fastest way to get code contributions into the source are pull requests, which,
if well written, can be merged right in. For this they have to comply with our coding
conventions below.

Alternatively you can make "casual" releases on the forum, which developers might pick up
as research or base to implement the features into the official source.

#### Coding conventions
* Base: [MS Naming Guidelines](http://msdn.microsoft.com/en-us/library/xzf533w0%28v=vs.71%29.aspx),
        [MS C# Coding Conventions](http://msdn.microsoft.com/en-us/library/ff926074.aspx)
* Exceptions:
  * Use `_private` for private fields and `this.Foobar` for properties, public fields, and methods.
  * Use tabs, not spaces.
* Comment lines shouldn't exceed ~80 characters, other lines' lengths are irrevelant.
* Excessive usage of the auto-formatting feature is encouraged. (Default VS settings)
* Avoid regions.

Common problems
------------------------------

### Errors after updates
Usually all errors are solveable by recompiling, running SQL updates from `sql/`, and deleting the cache folder.

### Korean message when trying to connect to channel
This message means that the client wasn't able to connect to the channel,
which is usually caused by configuration mistakes. Make sure you can reach
the IP/Port you've set the channel to run on (`channel.conf`) from the affected computer.

In some rare cases this can also be caused by routers, for example:
if you try to make the server publicly available and the router doesn't
allow you to connect to your own public IP.

Links
------------------------------
* GitHub: https://github.com/aura-project
* Forums: http://dev.mabinoger.com
* Wiki: http://dev.mabinoger.com/wiki
* Trello: https://trello.com/b/qnLaezQf/aura
* [![Gitter chat](https://badges.gitter.im/aura-project/aura.png)](https://gitter.im/aura-project/aura)
