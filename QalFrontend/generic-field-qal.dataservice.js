(function () {
  'use strict';

  angular
      .module('forceinspect.genericField.QAL')
      .factory('qAlGenericConfigDataService', qAlGenericConfigDataService);

  qAlGenericConfigDataService.$inject = ['dataService'];

    function qAlGenericConfigDataService(dataService) {
        var service = {
                getGenericConfigTables: dataService.createGetFunction('T_GenericConfigTable'),
            getGenericConfigFields: dataService.createGetFunction('T_GenericConfigField'),
            getGenericValueRow: dataService.createGetFunction('T_GenericValueRow'),
            getGenericValueFields: dataService.createGetFunction('T_GenericValueField'),
            getGenericConfigLookupField: dataService.createGetFunction('T_GenericConfigLookupField'),
            getGenericConfigLookupFieldMapping: dataService.createGetFunction('T_GenericConfigLookupFieldMapping'),
            getGenericConfigLookupTable: dataService.createGetFunction('T_GenericConfigLookupTable'),
            getContacts: dataService.createGetFunction('T_Contact'),
            getObjects: dataService.createGetFunction('T_Object'),
                getInspections: dataService.createGetFunction('T_Inspection')
            };

      return service;
  }
})();