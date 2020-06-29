(function () {
  'use strict';
  angular
      .module('forceinspect.genericField.QAL')
      .controller('QalModalController', QalModalController);
    QalModalController.$inject = ['$window', '$scope', 'breeze', 'qalModelDataService', 'authService','queryService', 'dataService','FI_ENUMS'];

    function QalModalController($window, $scope, breeze, qalModelDataService, authService,queryService,dataService, FI_ENUMS) {
      var vm = this;
      console.log(vm.itemId);
      vm.setInspectionDone = setInspectionDone;
        vm.closeModal = closeModal;
        vm.doOperation = doOperation;
        vm.inspection = void 0;
        function doOperation() {
            if (vm.operationName === 'setInspectionToDone') {
                setInspectionDone();
            }
        }

      function closeModal() {
          $('#qalModal').modal('hide');
      }

        function setInspectionDone() {
            if (vm.itemId === undefined) return;
                var query = new breeze.EntityQuery()
                    .where('InspectionGUID', breeze.FilterQueryOp.Equals, vm.itemId)
                    .expand('InspectionInspectionTypes');
            var promise = qalModelDataService.getInspections(query);
                promise.then(function (data) {
                    vm.currentItem = data.results[0];
                    vm.currentItem.InspectionStatusID = FI_ENUMS.INSPECTION_STATUS.DONE;
                    vm.currentItem.LastModifiedByInitiatorKey = authService.getUserInfo().InitiatorGuid;
                    createInspection();
                });
          
        }

        function createInspection() {
            var cDate = new Date();
         
            dataService.getDefaultManager().then(function (manager) {
                console.log(manager);
                if (vm.inspection === undefined) {
                    vm.inspection = manager.createEntity('T_Inspection',
                        {
                            ObjectID: vm.currentItem.ObjectID,
                            CreatedByUserID: authService.getUserInfo().LegacyUserId,
                            InspectionDateTime: cDate,
                            CreatedDateTime: cDate,
                            JobId: vm.currentItem.JobId,
                            ExternalComment: breeze.core.getUuid(),
                            InspectionStatusID: FI_ENUMS.INSPECTION_STATUS.IN_PROGRESS
                        });

                    // T_Inspection_InspectionTypes
                    manager.createEntity('T_Inspection_InspectionTypes',
                        {
                            InspectionGUID: vm.inspection.InspectionGUID,
                            InspectionTypeID: vm.currentItem.InspectionInspectionTypes[0].InspectionTypeID
                        });
                }

                try {
                    dataService.saveDefaultManager();
                    gotoInspection(vm.inspection);
                    
                    //closeModal();
                } catch (err) {
                    //
                }

            }); 
        }

        function gotoInspection(inspection) {
            setTimeout(function () {
                console.log(inspection);
                var query = new breeze.EntityQuery()
                    .where('ObjectID', breeze.FilterQueryOp.Equals, inspection.ObjectID)
                    .where('CreatedByUserID', breeze.FilterQueryOp.Equals, inspection.CreatedByUserID)
                    .where('JobId', breeze.FilterQueryOp.Equals, inspection.JobId)
                    .where('ExternalComment', breeze.FilterQueryOp.Equals, inspection.ExternalComment);
                var promise = qalModelDataService.getInspections(query);
                promise.then(function (data) {
                    var ins = data.results[0];
                    if (ins === undefined) gotoInspection(inspection);
                    $window.location.href = '/#/inspection/' + ins.InspectionGUID;
                    closeModal();
                });
            }, 2000);

        }
  }
})();