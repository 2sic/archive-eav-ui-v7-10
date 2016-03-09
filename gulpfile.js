var gulp = require("gulp"),
    $ = require("gulp-load-plugins")({ lazy: false }),
    merge = require("merge-stream"),
    debug = true,
    js = "js",
    css = "css";

// General configuration
var config = {
    templateCache: {
        file: "templates.js",
        options: {
            module: "app.core",
            root: "app/",
            standAlone: false
        }
    },
    temp: "./.tmp/"
};
var admin = createConfig("admin");
var edit = createConfig("edit");


gulp.task("build-test-run", function () {
    packageJs(admin);
});

gulp.task('watch-all', function() {
    gulp.watch(admin.cwd + "**/*", createWatchCallback(admin, js));
    gulp.watch(edit.cwd + "**/*", createWatchCallback(edit, js));
});

// assemble a function which will call the desired set
function createWatchCallback(set, part) {
    if (debug) console.log('creating watcher callback for ' + set.name);
    if(part === js)
    return function(event) {
        if(debug) console.log('File ' + event.path + ' was ' + event.type + ', running tasks on set ' + set.name);
        packageJs(set);
        console.log("finished '" + set.name + "'");
    }
    if(part === css)
        return function(event) {
            // todo
        }
}

// deploy the result to the current 2sxc-dev
gulp.task("publish-dist-to-2sxc", function () {
    gulp.src("./dist/**/*")// '*.{ttf,woff,eof,svg}')
    .pipe(gulp.dest("./../2SexyContent/Web/DesktopModules/ToSIC_SexyContent/dist"));
});


//#region basic functions I'll need a lot
function createConfig(key) {
    return {
        name: key,
        cwd: "src/" + key + "/",
        dist: "tmp-gulp/dist/" + key + "/",
        css: {
            cwd: ["src/" + key + "/**/*.css"],
            concat: "eav-" + key + ".css"
        },
        js: {
            cwd: ["src/" + key + "/**/*.js"],
            concat: "eav-" + key + ".js",
            templates: ["src/" + key + "/**/*.html"]
        },
        // tmp: "tmp/" + key + "/",
        // templates: "tmp/" + key + "/html-templates.js",
        // concatFile: "eav-" + key + ".js",
        // uglifyFile: "eav-" + key + ".min.js",
    }
}

function packageJs(set) {
    if (debug) console.log("bundling start: " + set.name);

    var js = gulp.src(set.js.cwd)
        .pipe($.jshint())
        .pipe($.ngAnnotate())
    ;

    var tmpl = gulp.src(set.js.templates)
        //.pipe($.htmlmin({ collapseWhitespace: true }))
        .pipe($.angularTemplatecache("templates.js", {
            module: "eavTemplates"
        }));

    var result = merge(js, tmpl)
        .pipe($.concat(set.js.concat))
        .pipe(gulp.dest(set.dist))
        .pipe($.rename({ extname: ".min.js" }))
        .pipe($.sourcemaps.init({ loadMaps: true }))
            .pipe($.uglify())
            .on("error", $.util.log)
        .pipe($.sourcemaps.write("./"))
        .pipe(gulp.dest(set.dist));

    if (debug) console.log("bundling done: " + set.name);

    return result;
}

function packageCss(set) {
    if (debug) console.log("css packaging start: " + set.name);

    var result = gulp.src(set.css.cwd)
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
    if (debug) console.log("css packaging done: " + set.name);
    return result;
}
//#endregion

gulp.task("test-css", function() {
    packageCss(admin);
});
