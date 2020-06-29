(function () {
  'use strict';

    angular.module('forceinspect.genericField.QAL')
      .component('qalModal',
    {
        templateUrl: 'app/sections/qal/qalModal/qalModal.html',
        controller: 'QalModalController',
      controllerAs: 'vm',
        bindings: {
            itemId: '=',
            itemType: '=',
            operationName: '@'
            }
    });
})();