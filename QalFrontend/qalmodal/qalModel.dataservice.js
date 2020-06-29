(function () {
  'use strict';

  angular
      .module('forceinspect.genericField.QAL')
      .factory('qalModelDataService', qalModelDataService);

  qalModelDataService.$inject = ['dataService'];

    function qalModelDataService(dataService) {
        var service = {
                getInspections: dataService.createGetFunction('T_Inspection')
            };

      return service;
  }
})();