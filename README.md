Login Rules
===========

Login Rules is a simple application used in the Commerce Computer Labs at student logon. It is a full screen application that displays a rules page and a background advertising page. The student needs to scroll down the entire rules window before being able to accept the rules and continuing on to Windows. On acceptance of the rules, a copy of the file is made to their personal network folder.

Both the rules and background page are pulled from the Commerce webserver (http://www.commerce.uct.ac.za/services/newsfeed).

Login_Rules_Distributor is a small application written in VB6 that is responsible for copying the files to C:\Login_Rules and launching the application from there.

Created by Craig Lotter, March 2007

Note: Created for Commerce IT

*********************************

Project Details:

Coded in Visual Basic .NET using Visual Studio .NET 2005
Implements concepts such as threading and file manipulation.
Level of Complexity: Very Simple

*********************************

Update 20070405.02:

- Added an option not to accept the laid out rules. If selected, the application logs out of Windows. 

*********************************

Update 20080416.03:

- Bug Fix: Application attempts to copy rules.htm to F:\ drive without checking ReadOnly status.

*********************************

Update 20080910.04:

- Commerce I.T. Requirement: Track all logins by using Login Rules application. Application now submits user details via webcall to Lab Usage Tracker database on the Commerce webserver.
