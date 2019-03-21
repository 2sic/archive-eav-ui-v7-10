// This is a temporary bridge component
// it lets us open the new angular UIs which are in a separate solution
//
// The goal is to one day move all dialogs into that system
// but until that is done, we'll have a hybrid situation
(function () {

  angular.module('Migration', [
    'ng',
    'EavAdminUi',
    'InitParametersFromUrl'
  ]);
}()); 