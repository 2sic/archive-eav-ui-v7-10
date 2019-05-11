# Language Packs (i18n) for eav & 2sxc
This github repository is for managing and sharing the language packs (i18n) for the following project:

* [eav](https://github.com/2sic/eav) - the data storage system and data-api used in 2sxc
* [2sxc](https://github.com/2sic/2sxc) - the sexy content management system for DNN

Note that we chose to create an own repository because it's easier to handle with many collaborators. So the core projects only contain the newest EN language pack, which is copied to here every few changes. 

## Background
### Angular-Translate and the Dev-Process

To understand i18n (internationalization) in these projects it helps to understand how the i18n implementation work. Here's the stack - beginning from the browser:

The browser runs [AngularJS](https://angularjs.org/) and [Angular-Translate](https://angular-translate.github.io/). It's configured to retrieve language files in this pattern:

* `[root-folder]/dist/i18n/[part-name]-[2lettercode].json`

Note that 

* we're actually using a JSON file (not a JS), but many web servers will refuse to deliver a file with a `.json` extension, so we're publishing them as `.js`
* if the desired language is missing, the server will deliver a http 404 but that's ok, angular-translate will revert back to en

Editing a JSON with a .js extension is not fun in a normal code editor as it will always marke everything as invalid. So when editing/working with the files, we're using JSON as it should be. These are stored here:

* `[root-folder]/src/i18n/[part-name]-[2lettercode].js`

This way it's easy to work with in visual studio. 

Note that from the src-folder it is automatically copied/renamed to the dist-folder using the grunt-job in this folder (see `gruntfile.js`).

### Distribution of the Packages

Whenever we create a new distribution of 2sxc we will download the latest files from here and include them in the package. In case you want to know how that works - here's the brief answer: 2sxc contains a bower-dependency to this project and will automatically retrieve it whenever we work on it. To then inject it into the distribution we must run a grunt-job in 2sxc which will extract all these resources and merge them into 2sxc.

## How you can Contribute Translations

### Preferred: Translation by Forking

The normal way to contribute is by 

1. forking the project 
2. copy the `/src/i18n/[part-name]-en.json` files and rename to your language code
2. adding your translations in these new json files 
3. creating a pull-request so your changes get merged into our branch.

### Alternative: Translation by Mail

Use this method if you don't understand github and forking.

1. download, copy & rename  the `/src/i18n/....-en.json` files
2. translate
3. send me a mail containing these (if you don't know how to reach me, open an issue)

## Help for Common Translation Actions

### Comparing Changes Across Versions

Often when the core language pack changes, it would help translators see what changed. This can be done easily using the Github change-tool. It works as follows:

1. Whenever we release a new version, we add a tag like [2sxc_08.00.04](https://github.com/2sic/2sxc-eav-languages/tree/2sxc_08.00.04) to mark that version
2. So to compare what changed, you can just use two tags and compare them - like this comparison between [08.00.04 and 08.00.05](https://github.com/2sic/2sxc-eav-languages/compare/2sxc_08.00.04...2sxc_08.00.05)

As you can see in the previous example, the main difference (except for some .sln files) is a single line in the `edit-en.json` which also appears in the .js. You can now just tweak the URL of the compare link to comper any versions you need. 

### Synchronizing GitHub Forks

1. watch this [quick tutorial to re-sync your master with our updated master](http://www.hpique.com/2013/09/updating-a-fork-directly-from-github/) before you add your changes.

### Testing your Translation in 2sxc

To test your translation in 2sxc you must simply place them in a 2sxc in the `[2sxc]/dist/i18n/` folder and use the .js extension. Typically you would do this by running the grunt-script in this project but you can also do it manually. 

## Common Problems and Issues

### Language Files are in the /dist/18n but don't get picked up by 2sxc

The first thing you must check is if the browser is actually trying to retrieve them, and if they are being served by the server. 
For this you should watch the http-trafic on your browser, either using debug-tools in your browser (F12) or by using [fiddler](http://www.telerik.com/fiddler). 

1. Check if the browser is actually making requests to your files when working in 2sxc. 
If not, then apparently 2sxc doesn't know that it should load these languages. 
Make sure they are enabled in DNN & 2sxc, and that they are the current language.

2. Check if you see 404-messages when your server responds. If yes, then the files must be in the wrong location.

### Language Files are Delivered but don't work

This usually happens if the JSON file is corrupt. Most common mistakes are:

* bracket open/close missmatch { }
* you forgot the quotes (") in the key - remember that a JSON always needs quotes, and they must be double quotes - like `"Label": "Save"`