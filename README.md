# questlists_manager
This is basically the same as the previous version, with the following differences.
- New UI (which I personally wanted to try).
- To be able to load more quests. Note that the game will only load a maximum of 512 quests.
- Rename repository (as it is no longer just a reader).
- Blank quests can be added(especially for CQ).
- Quest files no longer need to be converted in ReFrontier in advance.
- Added the ability to swap the order of quests.
- Changed the way to temporarily save loaded files (hope it works faster than before)

# Build
Put `data` and `output` folder in the same folder as the exe.

# WIP things
- Quest type
- Equipment information

# What's database 
The database is a file that stores only the information needed for questlists from the quest file.  
You can easily know the detail about each quests and add them to the list.  
It is not included in this repository and must be created by the user himself.   
The created database is automatically placed in the `data` folder.  
When creating, an option appears, and if you select yes, all quests in the folder will be read. This means that the same quests but slightly different, such as `22051d0` and `22051n2`, will also be read. This will increase the size of the database.  

The input box above the database is for searching quest names.

# How to create database file
(optional) Find the file named `55583d0.bin` and right-click to open its properties. If the file is read-only, uncheck.

1. Download ReFrontier(made by mhvuze) from Github, and build it.  
You'll get `ReFrontier.exe`.  
https://github.com/mhvuze/ReFrontier  
  
2. Drag and drop `Erupe/bin/quests` folder to `ReFrontier.exe`.  
This will dectypt all quest files in folder.  
The converted files can be used in the game as before without any problems.  
But if you don't like this, duplicate the quests folder in another directory and use it.  
  
3. On questlist_manager, inside Database section, click database icon.  
A message box appears. Normally, select no, 
      
4. A folder browser will be launched. Select the quests folder used in step 2.  
  
5. Upon successful creation, the file is automatically placed in the data folder.  
You will be asked if you want to continue loading the database, select yes if you want to use it.  

# Misc.
- Only the first time you try to switch to page 2, it is very slow for about 10 seconds. This is because it is adding a large number of weapon and armor names to the list.

# Credits
- SephVII
- Malckyor
- ezemania2

# Changelog
## 1.0
- Initial release.

## 1.1
- Added database function.

## 1.2
- Changed some text position and order.
- Expaneded width of database listbox.
- Added the ability to add the currently selected quest in the database to the list using the enter key.
- Adjusted search box to function properly.
- Fixed an issue where quests were not being restored correctly when the search box was empty.

## 1.21
- Window size can be changed.
- Fixed an issue where an error message was displayed twice when a database file could not be found.
