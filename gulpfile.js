(() => {
  'use strict';

  // important: disabled gps as it just modifies the file but never really does anything
  const exportGps = false;

  const gulp = require('gulp');
  const $ = require('gulp-load-plugins')({ lazy: false });
  const merge = require('merge-stream');
  const packageJSON = require('./package');
  const config = {
    debug: true,
    autostart: true,
    autopublish: true,
    rootDist: 'dist/'
  };

  const autopublishTarget = '/DesktopModules/ToSIC_SexyContent/dist';
  const dests = {
    default: './../2sxc-dnn742/Website',
    evoq: '../TestWebsites/Evoq 9.1.0'
  };

  return (() => {
    gulp.task('copy-all-with.data', () => copyAll(dests.default));
    gulp.task('watch', () => watchSets(createSetsForOurCode()));
    gulp.task('watch-input-types', () =>
      watchSets(createSetsForOurCode(), 'json')
    );
    gulp.task('watch-dist-2sxc', ['watch'], () => watchPublish(dests.default));
    gulp.task('watch-dist-evoq', ['watch'], () => watchPublish(dests.evoq));
    gulp.task('watch-libs', () => watchSets(createSetsForLibs()));

    function watchPublish(dest) {
      return $.watch(
        [
          'dist/**/*',
          'dist/.**/*' // note: this one should get .data folders, but it's not working
        ],
        {
          ignoreInitial: false,
          dot: true
        }
      )
        .pipe($.debug())
        .pipe(gulp.dest(dest + autopublishTarget));
    }
  })();

  // special helper - necessary to copy everything incl the ".data" folders
  // which are otherwise skipped
  function copyAll(dest) {
    gulp
      .src(['dist/**/*', 'dist/.**/*'], {
        dot: true
      })
      .pipe($.debug())
      .pipe(gulp.dest(dest + autopublishTarget));
  }

  function createConfig(key, tmplSetName, altDistPath, altJsName, libFiles) {
    const cwd = `src/${key}/`;
    return {
      name: key,
      cwd,
      dist: altDistPath || config.rootDist + key + '/',
      css: {
        run: true,
        alsoRunMin: true,
        files: [cwd + '**/*.css'],
        libs: [],
        concat: 'eav-' + key + '.css'
      },
      js: {
        run: true,
        files: [`${cwd}**/*.js`, `!${cwd}**/*spec.js`, `!${cwd}**/tests/**`],
        libs: libFiles || [],
        concat: altJsName || `eav-${key}.js`,
        templates: [`src/${key}/**/*.html`],
        templateSetName: tmplSetName,
        autoSort: true,
        alsoRunMin: true
      },
      json: {
        run: false,
        files: [`${cwd}**/*.json`, `!${cwd}**/*spec.json`, `!${cwd}**/tests*`]
      }
    };
  }

  function packageJs(set) {
    if (config.debug) console.log(`bundling start: ${set.name}`);
    let js = gulp.src(set.js.files);
    if (set.js.autoSort) js = js.pipe($.sort());
    js = js
      .pipe($.jshint(packageJSON.jshintConfig))
      .pipe($.jshint.reporter('jshint-stylish'))
      .pipe($.ngAnnotate());

    const tmpl = !set.js.templates
      ? null
      : gulp
          .src(set.js.templates)
          .pipe($.sort())
          .pipe(
            $.angularTemplatecache('templates.js', {
              standalone: true,
              module: set.js.templateSetName
            })
          );

    const libs = gulp.src(set.js.libs);
    let prelib = merge(js, tmpl);
    if (set.js.autoSort) prelib = prelib.pipe($.sort());

    let result = merge(libs, prelib);
    if (set.js.autoSort) result = result.pipe($.sort());

    result = result
      .pipe($.concat(set.js.concat))
      .pipe(gulp.dest(set.dist))
      .pipe($.rename({ extname: '.min.js' }));

    if (set.js.alsoRunMin)
      result = result
        .pipe($.uglify())
        .on('error', $.util.log)
        .pipe(gulp.dest(set.dist));

    if (config.debug)
      console.log($.util.colors.cyan(`bundling done: ${set.name}`));

    return result;
  }

  function packageJsonTypes(set) {
    if (config.debug) console.log(`json start: ${set.name}`);
    gulp
      .src(set.json.files)
      .pipe($.flatten())
      .pipe(gulp.dest(set.dist + '.data/contenttypes/'));
  }

  function packageCss(set) {
    if (config.debug) console.log(`css packaging start: ${set.name}`);

    let result = gulp.src(set.css.files).pipe($.sort());
    const libs = gulp.src(set.css.libs); // don't sort libs

    result = merge(result, libs)
      .pipe($.concat(set.css.concat))
      .pipe(gulp.dest(set.dist));

    if (set.css.alsoRunMin)
      result // minify and save
        .pipe($.rename({ extname: '.min.css' }))
        .pipe($.sourcemaps.init())
        .pipe(
          $.cleanCss({
            compatibility: '*',
            processImportFrom: ['!fonts.googleapis.com']
          })
        ) // ie9 compatibility
        .pipe($.sourcemaps.write('./'))
        .pipe(gulp.dest(set.dist));

    if (config.debug)
      console.log($.util.colors.cyan(`css packaging done: ${set.name}`));
    return result;
  }

  // assemble a function which will call the desired set - this is a helper for the watch-sequence.
  function createWatchCallback(set, part) {
    if (config.debug) console.log(`creating watcher callback for ${set.name}`);
    const run = ev => {
      if (config.debug)
        console.log(
          `File ${ev.path} was ${ev.type}, running tasks on set ${set.name}`
        );
      (part === 'js'
        ? packageJs
        : part === 'json'
        ? packageJsonTypes
        : packageCss)(set);
      console.log("finished '" + set.name + "'" + new Date());
    };
    if (config.autostart) run({ path: '[none]', type: 'autostart' });
    return run;
  }

  function createSetsForOurCode() {
    const sets = [];
    const admin = createConfig('admin', 'eavTemplates');
    admin.css.files.push(`!${admin.cwd}*pipeline*.css`);
    sets.push(admin);

    // setup edit & extended
    var edit = createConfig('edit', 'eavEditTemplates');
    edit.json.run = true;
    sets.push(edit);

    // pipeline-designer (CSS only)
    const pDesigner = createConfig('admin', '');
    pDesigner.css.files = [`${admin.cwd}**/pipeline*.css`];
    pDesigner.css.concat = 'pipeline-designer.css';
    pDesigner.js.run = false;
    sets.push(pDesigner);

    // extension: gps-field
    const editExtGps = createConfig('edit-extended');
    editExtGps.cwd = editExtGps.cwd.replace(
      '/edit-extended/',
      '/edit-extended/fields/custom-gps/'
    );
    editExtGps.dist = editExtGps.dist.replace(
      '/edit-extended/',
      '/edit/extensions/field-custom-gps/'
    );
    editExtGps.js.concat = 'custom-gps.js';
    editExtGps.js.libs = [
      'bower_components/lodash/dist/lodash.min.js',
      'bower_components/angular-google-maps/dist/angular-google-maps.min.js',
      'bower_components/angular-simple-logger/dist/angular-simple-logger.js'
    ];
    editExtGps.js.autoSort = false;
    editExtGps.js.templateSetName = 'customGpsTemplates'; // probably not relevant, but not sure
    editExtGps.css.run = false;

    // 2017-11-29 - disabled adding to list, as it always caused non-relevant changes when building
    // must re-enable needed
    if (exportGps) sets.push(editExtGps);

    return sets;
  }

  function createSetsForLibs() {
    // todo sometime: add libs again - removed grunt in commit 2016-10-08 which contained thepaths etc.
    const sets = [];

    // 2017-11-25 2dm disabled this, as angular-translate is in the angular pack - I think this isn't used any more!
    //const i18n = createConfig('i18n', undefined, `${config.rootDist}lib/i18n/`, 'set.min.js', [
    //    'bower_components/angular-translate/angular-translate.min.js',
    //    'bower_components/angular-translate-loader-partial/angular-translate-loader-partial.min.js',
    //]);
    //i18n.js.autoSort = false;
    //i18n.js.alsoRunMin = false;
    //i18n.css.run = false;
    //sets.push(i18n);

    // part: ag-grid library
    const agGrid = createConfig(
      'ag-grid',
      undefined,
      config.rootDist + 'lib/ag-grid/',
      'ag-grid.min.js',
      ['bower_components/ag-grid/dist/ag-grid.min.js']
    );
    agGrid.css.libs = ['bower_components/ag-grid/dist/ag-grid.min.css'];
    agGrid.css.concat = 'ag-grid.min.css';
    agGrid.css.alsoRunMin = false;
    agGrid.js.alsoRunMin = false;
    sets.push(agGrid);

    const jsPlumb = createConfig(
      'jsPlumb',
      undefined,
      `${config.rootDist}lib/pipeline/`,
      'set.min.js',
      ['bower_components/jsplumb/dist/js/jsPlumb-2.1.7.js']
    );
    jsPlumb.js.alsoRunMin = false;
    jsPlumb.css.run = false;
    sets.push(jsPlumb);

    const libAng = createConfig(
      'angular',
      undefined,
      `${config.rootDist}lib/angular/`,
      'set.min.js',
      [
        'bower_components/angular/angular.min.js',
        'bower_components/angular-resource/angular-resource.min.js',
        'bower_components/angular-animate/angular-animate.min.js',
        'bower_components/angular-sanitize/angular-sanitize.min.js', // currently testing, needed for ui-select, maybe will remove again
        'bower_components/oclazyload/dist/oclazyload.min.js',
        'bower_components/angular-bootstrap/ui-bootstrap-tpls.min.js',
        'bower_components/angular-toastr/dist/angular-toastr.tpls.min.js',
        'bower_components/angular-translate/angular-translate.min.js',
        'bower_components/angular-translate-loader-partial/angular-translate-loader-partial.min.js',
        'bower_components/api-check/dist/api-check.min.js',
        'bower_components/angular-base64-upload/dist/angular-base64-upload.min.js',
        'bower_components/angular-formly/dist/formly.min.js',
        'bower_components/angular-formly-templates-bootstrap/dist/angular-formly-templates-bootstrap.min.js',
        'bower_components/angular-ui-tree/dist/angular-ui-tree.min.js',
        'bower_components/angular-ui-select/dist/select.min.js',
        'bower_components/dropzone/dist/min/dropzone.min.js',
        'bower_components/angular-ui-switch/angular-ui-switch.min.js'
      ]
    );
    libAng.css.libs = [
      'bower_components/bootstrap/dist/css/bootstrap.min.css',
      'bower_components/bootflat-for-2sic/bootflat/css/bootflat.min.css',
      'bower_components/angular-ui-tree/dist/angular-ui-tree.min.css',
      'bower_components/angular-ui-switch/angular-ui-switch.min.css',
      'bower_components/angular-toastr/dist/angular-toastr.css',
      'bower_components/angular-ui-select/dist/select.min.css'
    ];
    libAng.css.concat = 'set.min.css';
    libAng.css.alsoRunMin = false;
    libAng.js.autoSort = false;
    libAng.js.alsoRunMin = false;
    sets.push(libAng);

    return sets;
  }

  function watchSets(setList, partOnly) {
    setList.forEach(set => {
      if (set.js.run && (!partOnly || partOnly === 'js'))
        gulp.watch(set.cwd + '**/*', createWatchCallback(set, 'js'));
      if (set.json.run && (!partOnly || partOnly === 'json'))
        gulp.watch(set.cwd + '**/*', createWatchCallback(set, 'json'));
      if (set.css.run && (!partOnly || partOnly === 'css'))
        gulp.watch(set.cwd + '**/*', createWatchCallback(set, 'css'));
    });
  }
})();
