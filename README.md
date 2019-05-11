# eav UI
===
Presented by [2sic](https://www.2sic.com) internet solutions in Switzerland and Liechtenstein

This is the AngularJS based UI for the powerfull EAV (Entity-Attribute-Value) system based on Microsoft SQL Server. 

It's currently mainly used inside [2sxc](http://2sxc.org) ([github](https://github.com/2sic/2sxc)) for [DNN (DotNetNuke)](http://dnnsoftware.com)

We separated the UI from the server components because most people will want to contribute to the UI but won't actually understand the server. 

# How to build

1. npm install as always
1. some parts are loaded with bower - we just never found time to change that. These parts are now in the code repo, because we're not sure if bower still works. So don't run bower, even though it uses it, bowers time is over.
1. run gulp watch to build into dist
1. run gulp dist-to-2sxc to copy changes in dist to the 2sxc on the default path