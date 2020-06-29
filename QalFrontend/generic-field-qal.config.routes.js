(function () {
  'use strict';

  angular
      .module('forceinspect.genericField.QAL')
      .config(config);

  config.$inject = ['$routeProvider'];

    function config($routeProvider) { 
       // var objectPathPrefix = '/qal_object/';
        var inspectPathPrefix = '/qal_inspection/';
       // var reportPathPrefix = '/qal_report/';

        var templatPathPrefix = 'app/sections/qal/';
        var controllerPath = 'QalGenericFieldController';

        //Route for QALInspectionConfig
        $routeProvider.when(inspectPathPrefix + ':itemid', {
            templateUrl: templatPathPrefix + 'generic-field-qal-inspection.html',
            controller: controllerPath,
            controllerAs: 'vm',
            access: function (hasPermission, p) {
                // TODO Create a permission and change code.
                return hasPermission(p.OBJECT_READ);
            },
            reloadOnSearch: false
        });   

      //Commented later, all are for test purpose
      $routeProvider.when('/genericFieldQal', {
          templateUrl: templatPathPrefix + 'generic-field-qal.html',
          controller: controllerPath,
      controllerAs: 'vm',
      access: function (hasPermission, p) {
        // TODO Create a permission and change code.
        return hasPermission(p.OBJECT_READ);
      },
      reloadOnSearch: false
      });

  }
})();
