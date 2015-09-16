(function () {

    angular.module("ContentFormlyTypes")

        .config(function (formlyConfigProvider) {
            var formly = formlyConfigProvider;

            formly.setType({
                name: "file",
                template: "<file-field class='btn' ng-class='{ \"btn-success\": model[options.key] }' ng-model='model[options.key]'><span class='glyphicon glyphicon-open'></span></file-field><br /><span ng-if='model[options.key]'>{{model[options.key].name}}</span>",
                wrapper: ["bootstrapLabel", "bootstrapHasError"]
            });

            formly.setType({
                name: "hidden",
                template: "<input style='display:none' ng-model='model[options.key]' />",
                wrapper: ["bootstrapLabel", "bootstrapHasError"]
            });
    });
}());