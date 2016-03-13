// Initial variables, constants, etc.
var gulp = require("gulp"),
    $ = require("gulp-load-plugins")({ lazy: false }),
    packageJSON = require('./package'),
    jshintConfig = packageJSON.jshintConfig,
    merge = require("merge-stream"),
    js = "js",
    css = "css",
    config = {
        debug: true,
        autostart: true,
        autopublish: true,
        rootDist: "dist/",// "tmp-gulp/dist/"
    };

// setup admin, exclude pipeline css (later also exclude pipeline js)
var admin = createConfig("admin", "eavTemplates");
admin.css.files.push("!" + admin.cwd + "**/pipeline*.css");

// setup edit & extended
var edit = createConfig("edit", "eavEditTemplates");
var editExtGps = createConfig("edit-extended");

// pipeline-designer (CSS only)
var pDesigner = createConfig("admin", "");
pDesigner.css.files = [admin.cwd + "**/pipeline*.css"];
pDesigner.css.concat = "pipeline-designer.css";

// extension: gps-field
editExtGps.cwd = editExtGps.cwd.replace("/edit-extended/", "/edit-extended/fields/custom-gps/");
editExtGps.dist = editExtGps.dist.replace("/edit-extended/", "/edit/extensions/field-custom-gps/");
editExtGps.js.concat = "custom-gps.js";
editExtGps.js.libs = [
    "bower_components/lodash/lodash.min.js",
    "bower_components/angular-google-maps/dist/angular-google-maps.min.js",
    "bower_components/angular-simple-logger/dist/index.js"
];

// register all watches & run them
gulp.task("watch-all", function () {
    gulp.watch(admin.cwd + "**/*", createWatchCallback(admin, js));
    gulp.watch(admin.cwd + "**/*", createWatchCallback(admin, css));
    gulp.watch(edit.cwd + "**/*", createWatchCallback(edit, js));
    gulp.watch(edit.cwd + "**/*", createWatchCallback(edit, css));

    gulp.watch(pDesigner.files, createWatchCallback(pDesigner, css));

    gulp.watch(editExtGps.cwd + "**/*", createWatchCallback(editExtGps, js));
    //no css yet: gulp.watch(editExtGps.cwd + "**/*", createWatchCallback(editExtGps, css));
});

// test something - add your code here to test it
gulp.task("test-something", function () {

});

gulp.task("clean-dist", function () {
    // disabled 2016-03-13 to prevent mistakes as gulp doesn't generate everything yet
    //gulp.src(config.rootDist)
    //    .pipe($.clean());
});

// deploy the result to the current 2sxc-dev
gulp.task("publish-dist-to-2sxc", function () {
    gulp.src("./dist/**/*")// '*.{ttf,woff,eof,svg}')
    .pipe(gulp.dest("./../2SexyContent/Web/DesktopModules/ToSIC_SexyContent/dist"));
});


//#region basic functions I'll need a lot
function createConfig(key, tmplSetName) {
    var cwd = "src/" + key + "/";
    return {
        name: key,
        cwd: cwd,
        dist: config.rootDist + key + "/",
        css: {
            files: [cwd + "**/*.css"],
            concat: "eav-" + key + ".css"
        },
        js: {
            files: [cwd + "**/*.js", "!" + cwd + "**/*spec.js", "!" + cwd + "**/tests/**"],
            libs: [],
            concat: "eav-" + key + ".js",
            templates: ["src/" + key + "/**/*.html"],
            templateSetName: tmplSetName
        }
    }
}

// package a JS set
function packageJs(set) {
    if (config.debug) console.log("bundling start: " + set.name);

    var js = gulp.src(set.js.files)
        .pipe($.sort())
        .pipe($.jshint(jshintConfig))
        .pipe($.jshint.reporter('jshint-stylish'))
        //.pipe($.jshint.reporter('fail'))
        .pipe($.ngAnnotate())
    ;

    var tmpl = set.js.templates ? gulp.src(set.js.templates)
        //.pipe($.htmlmin({ collapseWhitespace: true }))
        .pipe($.angularTemplatecache("templates.js", { // set.js.templateSetName + ".js", { //"templates.js", {
            standalone: true,
            module: set.js.templateSetName // "eavTemplates"
        })) : null;

    var libs = gulp.src(set.js.libs);

    var prelib = merge(js, tmpl).pipe($.sort());

    var result = merge(libs, prelib)
        .pipe($.concat(set.js.concat))
        .pipe(gulp.dest(set.dist))
        .pipe($.rename({ extname: ".min.js" }))
        .pipe($.sourcemaps.init({ loadMaps: true }))
            .pipe($.uglify())
            .on("error", $.util.log)
        .pipe($.sourcemaps.write("./"))
        .pipe(gulp.dest(set.dist));

    if (config.debug) console.log("bundling done: " + set.name);

    return result;
}

// package a set of CSS
function packageCss(set) {
    if (config.debug) console.log("css packaging start: " + set.name);

    var result = gulp.src(set.css.files)
        // lint the css - not enabled right now, too many fix-suggestions
        //.pipe($.csslint())
        //.pipe($.csslint.reporter())

        // concat & save concat-only (for debugging)
        .pipe($.concat(set.css.concat))
        .pipe(gulp.dest(set.dist))

        // minify and save
        .pipe($.rename({ extname: ".min.css" }))
        .pipe($.sourcemaps.init())
        .pipe($.cleanCss({ compatibility: "*" /* ie9 compatibility */ }))
        .pipe($.sourcemaps.write("./"))
        .pipe(gulp.dest(set.dist));
    ;
    if (config.debug) console.log("css packaging done: " + set.name);
    return result;
}

// assemble a function which will call the desired set - this is a helper for the watch-sequence. 
function createWatchCallback(set, part) {
    if (config.debug) console.log("creating watcher callback for " + set.name);
    var run = function (event) {
        if (config.debug) console.log("File " + event.path + " was " + event.type + ", running tasks on set " + set.name);
        var call = (part === js) ? packageJs : packageCss;
        call(set);
        console.log("finished '" + set.name + "'" + new Date());
        if (config.autopublish) {
            console.log("publishing...");
            gulp.start("publish-dist-to-2sxc");
            console.log("publishing done...");
        }
    }
    if (config.autostart)
        run({ path: "[none]", type: "autostart" });
    return run;
}


//#endregion


