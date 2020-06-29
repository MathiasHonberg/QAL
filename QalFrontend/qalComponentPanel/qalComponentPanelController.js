(function () {
  'use strict';

  angular
    .module('forceinspect.genericField.QAL')
      .controller('QalComponentPanelController', QalComponentPanelController);


    QalComponentPanelController.$inject = ['$scope', '$routeParams', 'breeze', 'queryService', 'authService', 'dataService', 'qAlGenericConfigDataService','FI_ENUMS'];

    function QalComponentPanelController($scope, $routeParams, breeze, queryService, authService, dataService, qAlGenericConfigDataService, FI_ENUMS) {
        var vm = this;
        vm.ItemId = $scope.itemIdentifier;
        vm.genericGroupTypeId = 2;//$scope.genericGroupTypeId;
        vm.typeId = 21;//$scope.typeId;
        console.log(vm.ItemId);
        console.log($scope.genericGroupTypeId);
        console.log($scope.typeId);

        vm.IsReadOnly = '1';
        if (vm.isReadOnly !== undefined) {
            vm.IsReadOnly = vm.isReadOnly;
        }

        console.log($routeParams.objectId);
      console.log(vm.item);
      
        vm.LocalManager = void 0;


        //vm.ItemId = $routeParams.itemid;
        vm.selectedGenericConfigTableVersion = void 0;
        vm.isNew = false;
        vm.isEdit = false;
        vm.GroupTypeName = '';
        //vm.GroupTypeId = '3';
        vm.itemFieldDesc = 'ItemId';

        //TEST DATA
        //vm.ObjectId = 867124;
        //vm.InspectionKey = 'D40C0813-3EAF-4707-A635-82FD38DE31B2';
        //A59247D0-8F37-4D9E-AD67-6E9414AD3D10
        //vm.ItemId = vm.InspectionKey;
        //END TEST DATA

        vm.GenericConfigTableKey = '00000000-0000-0000-0000-000000000000';
        

        vm.showConfigFieldBlock = false;       
        vm.showVersionMismatchError = false;

        //constant data
        vm.GenericGroupType = {
            Container: '1',
            Object: '2',
            Inspection: '3',
            Report: '4'
        };

        vm.DataType = {
            List: 1,
            Boolean: 2,
            DateTime: 3,
            Decimal: 4,
            Int: 5,
            Text: 6,
            Lookup:7
        };


        vm.genericConfigTable = void 0;
        vm.selectedGenericConfigTable = void 0;
        vm.genericConfigFields = void 0;
        
        vm.ListField = [];
        vm.entities = [];
        vm.entitiesDictionary = {};
       
        vm.GenericLookUpFields = {};
        vm.GenericLookUpTableKeysMapping = {};
        vm.GenericLookUpTableKeysDictionary = {};

        //vm.LookUpMappingTables = [];        
        vm.LookUpFieldsKeysLoaded = {};
        vm.lookupFieldSelectedItem = {};
        vm.lookupSubFieldSelectedItem = {};
        vm.DependentFieldsDictionary = {};
        vm.GenericValueFields = {};

        //function declaration
        vm.getIndexWithTagId = getIndexWithTagId;
        vm.LoadGenericConfigTable = loadGenericConfigTable;
        vm.GetValues = getValues;
        vm.DependentFields = dependentFields;
        vm.GetValueField = GetValueField;
        vm.GetConfigField = GetConfigField; 
        vm.IsEntityExists = IsEntityExists;
        vm.regexEscape = regexEscape;
        vm.components = {};
        vm.CreateObject = createObject;
        vm.CreateComponent = createComponent;
        vm.IsSelect = void 0;
        vm.styles = void 0;
        queryService.delay($scope, 400, vm.LoadGenericConfigTable);

        /*
         * Load configurations
         */
        vm.LocalManager = void 0;
      

        function loadGenericConfigTable() {
            if ($routeParams.objectId === null || $routeParams.objectId === undefined) return;
            vm.styles = 'border: 1px solid gray; padding: 1px; margin: 1px;';
            if (vm.GenericConfigTableKey === undefined) return;
            vm.showVersionMismatchError = false;

            dataService.getLocalManager().then(function (manager) {
                vm.LocalManager = manager;
                var query = new breeze.EntityQuery()
                        .where('GenericConfigTableKey', breeze.FilterQueryOp.Equals, vm.GenericConfigTableKey)
                        .where('GenericGroupTypeId', breeze.FilterQueryOp.Equals, vm.genericGroupTypeId)
                        .where('VersionId', breeze.FilterQueryOp.Equals, vm.selectedGenericConfigTableVersion)
                        .where('GroupId', breeze.FilterQueryOp.Equals, vm.typeId)
                        .expand(
                            'T_GenericConfigField.T_GenericConfigLookupTable.T_GenericConfigLookupField.T_GenericConfigLookupFieldMapping1.T_GenericConfigLookupField');

                    vm.genericConfigurations =
                        queryService.createQueryHelper(
                            qAlGenericConfigDataService.getGenericConfigTables(query, manager));
                    vm.genericConfigurations.promise.then(load);
            });
      }

        function load(data) {
            vm.genericConfigTable = data.results;
            vm.selectedGenericConfigTable = data.results[0];
            vm.showVersionMismatchError =
                vm.selectedGenericConfigTable === undefined
                || vm.selectedGenericConfigTable.VersionId !== vm.selectedGenericConfigTableVersion;

            //populateConfigFields();
            loadGenericValueRows();
        }

      function loadGenericValueRows() {
              if (vm.GenericConfigTableKey === undefined) return;
              vm.showVersionMismatchError = false;
              vm.ItemIdentifiers = [];

              var itemIdColumn = 'ItemId';
              vm.ItemIdentifiers.push(vm.ItemId);
              if (vm.genericGroupTypeId === '3' || vm.genericGroupTypeId === '4') {
                  itemIdColumn = 'ItemKey';
              }
              dataService.getLocalManager().then(function (manager) {
                  vm.LocalManager = manager;
                  var query = void 0;
                  if (vm.ItemId > 0) {
                      query = new breeze.EntityQuery()
                          .where('GenericConfigTableKey', breeze.FilterQueryOp.Equals, vm.GenericConfigTableKey)
                          .where('VersionId', breeze.FilterQueryOp.Equals, vm.selectedGenericConfigTableVersion)
                          .where(itemIdColumn, breeze.FilterQueryOp.Equals, vm.ItemId)
                          .expand('T_GenericValueField');

                      vm.genericConfigurations =
                          queryService.createQueryHelper(qAlGenericConfigDataService.getGenericValueRow(query, manager));
                      vm.genericConfigurations.promise.then(populateConfigFields);
                  } else if (vm.genericGroupTypeId === '3' || vm.genericGroupTypeId === '4') {
                      query = new breeze.EntityQuery()
                          .where('GenericConfigTableKey', breeze.FilterQueryOp.Equals, vm.GenericConfigTableKey)
                          .where('VersionId', breeze.FilterQueryOp.Equals, vm.selectedGenericConfigTableVersion)
                          .where(itemIdColumn, breeze.FilterQueryOp.Equals, vm.ItemId)
                          .expand('T_GenericValueField');

                      vm.genericConfigurations =
                          queryService.createQueryHelper(qAlGenericConfigDataService.getGenericValueRow(query, manager));
                      vm.genericConfigurations.promise.then(populateConfigFields);
                  }
                  else {
                      populateConfigFields([]);
                  }
              });
          
      }

        /*
         * populate config field fields
         */

        function populateConfigFields(rows) {
            if (vm.selectedGenericConfigTable === undefined ||
                vm.selectedGenericConfigTable.GenericConfigTableKey === undefined ||
                vm.selectedGenericConfigTable.GenericConfigTableKey === null) return;

            vm.GenericValueFields = {}; //initialize GenericValueField list
            vm.valueRowKey = void 0;
            vm.genericConfigFields = void 0;
            vm.entities = [];
            vm.cFields = [];

            //assign T_GenericConfigField for selected configuration
            var data = vm.selectedGenericConfigTable.T_GenericConfigField;

            var valueRows = rows.results;//vm.selectedGenericConfigTable.T_GenericValueRow;

            //this condition determines, whether do we have to insert the data or edit
            if (valueRows===undefined || valueRows === null || valueRows[0] === undefined) {
                vm.genericFieldValues = void 0;
                vm.showConfigFieldBlock = true;
                vm.isNew = true;
                vm.IsEdit = false;
            } else {
                vm.valueRowKey = valueRows[0].GenericValueRowKey;
                vm.GenericValueFields = valueRows[0].T_GenericValueField;
                vm.showConfigFieldBlock = false;
                vm.isEdit = true;
                vm.isNew = false;
            }

            var i = 0;
           // print(data, 'populateConfigFields');

            //prepare few list, so insted of navigation, we can fetch data faster
            //data is generic config field
            _.forEach(data, function (v) {
                
                if (vm.isEdit) {
                    vm.entities[i] = v.T_GenericValueField[0];
                }

                vm.cFields[i] = v;

                vm.entitiesDictionary[v.TagId] = i++;
                if (v.GenericConfigLookupTableKey === null) return;                
                vm.GenericLookUpTableKeysMapping[v.TagId] = v.GenericConfigLookupTableKey; //mapping config fieldkey and lookuptable key

                //mapping lookuptable with posible list of data/LookUpTables
                if (vm.GenericLookUpFields[v.GenericConfigLookupTableKey] === undefined && v.T_GenericConfigLookupTable !== undefined) {
                    if (v.T_GenericConfigLookupTable.T_GenericConfigLookupField === null) return;
                    _.forEach(v.T_GenericConfigLookupTable.T_GenericConfigLookupField, function (v1) {
                        if (vm.GenericLookUpFields[v1.GenericConfigLookupTableKey] === undefined) {
                            vm.GenericLookUpFields[v1.GenericConfigLookupTableKey] = [];
                        }
                        vm.GenericLookUpFields[v1.GenericConfigLookupTableKey].push(v1);
                    });
                }

            });


            //select value in dropdown if we have already inserted value in database
            if (vm.isEdit) {
                _.forEach(data,
                    function(fv) {
                        //vm.lookupFieldSelectedItem[fv.GenericConfigFieldKey] = fv;
                        if (fv.DataTypeId === vm.DataType.Lookup)
                            dependentFields(fv.TagId);
                    });
            }


            //subscribe auto change
            if (!vm.isEdit) {
                create();
                var cFieldKey = vm.item.T_GenericConfigLookupTable.T_GenericConfigField[0].GenericConfigFieldKey;
                var field = vm.GetValueField(cFieldKey);
                field.GenericConfigLookupFieldKey = vm.item.GenericConfigLookupFieldKey;
                dependentFields(cFieldKey);
                //_.forEach(vm.entities, function (entity) {
                //    entity.entityAspect.propertyChanged.subscribe();
                //});
            }
            
            vm.entities[i] = '';
        }
         

      function getIndexWithTagId(tagId) {
          var index = vm.entitiesDictionary[tagId];
          if (index === undefined) {
              index = vm.entitiesDictionary[tagId.toUpperCase()];
          }
          return index;
      }

      /*
       * refresh dependent dropdown values based on parent dropdown selected value
       *  GenericLookUpFields - mapping of [GenericLookUpTableKey][List of Lookupfields]
       *  ----T_GenericConfigLookupFieldMapping1 (contains the mapping of dependent child lookupfields)
       *  -------T_GenericConfigLookupField (dependent child lookupfields)
       *  ------------T_GenericConfigLookupTable
       *  -------------------T_GenericConfigField (control in html needs to refresh with value)
       *
       * id is generic config field key
       */
      function dependentFields(id) {
          var fieldValue = vm.entities[vm.getIndexWithTagId(id)];
          if (fieldValue.GenericConfigLookupFieldKey === undefined) return;
          var lookUpFieldKey = fieldValue.GenericConfigLookupFieldKey;
          var lookUpTableKey = vm.GenericLookUpTableKeysMapping[id];
          if (lookUpTableKey === undefined) {
              lookUpTableKey = vm.GenericLookUpTableKeysMapping[id.toUpperCase()];
          }
            var initialize = true;
            console.log('lookuptable key ' + lookUpTableKey);

            if (vm.GenericLookUpFields[lookUpTableKey] === undefined) return;
            _.forEach(vm.GenericLookUpFields[lookUpTableKey], function (v) { //v is lookupfield list for lookuptable or parent lookupfield
                if (v.GenericConfigLookupFieldKey === lookUpFieldKey) {                    
                    _.forEach(v.T_GenericConfigLookupFieldMapping1, function (vf) { //dependent or child lookup field
                        if (vf.T_GenericConfigLookupField.T_GenericConfigLookupTable!== null) {
                        _.forEach(vf.T_GenericConfigLookupField.T_GenericConfigLookupTable.T_GenericConfigField, function (vcf) { //child lookupfield's configfield
                            var tagId = vcf.TagId;
                            if (vcf.ParentGenericFieldConfigKey.toUpperCase() !== id
                                && vcf.ParentGenericFieldConfigKey.toUpperCase() !== id.toUpperCase()) return;
                            if (vm.DependentFieldsDictionary[tagId] === undefined || initialize) {
                                vm.DependentFieldsDictionary[tagId] = [];
                                initialize = false;
                            }

                            vm.DependentFieldsDictionary[tagId].push(vf.T_GenericConfigLookupField);
                        });
                    }
                    });
                }
            });

            console.log(vm.DependentFieldsDictionary);
        }

      function getValues(id) {
            return vm.GenericLookUpFields[vm.GenericLookUpTableKeysMapping[id]];
        }
      
        function regexEscape(decimal) {
            if (decimal === undefined || decimal===null) return;
            return decimal.toString().replace(',', '.');
        }

        vm.valueRow = {};
        function create() {
            vm.valueRow = {};
            var createdDatetime = new Date();
            if (vm.selectedGenericConfigTable === undefined) return;
            vm.user = authService.getUserInfo();
            if (vm.user === undefined) return;
            var LastModifiedBy = vm.user.InitiatorGuid;

            console.log(vm.LocalManager);
            var valueRowKey = breeze.core.getUuid();
            vm.valueRow = //vm.LocalManager.createEntity('T_GenericValueRow',
            {
                GenericValueRowKey: valueRowKey,
                GenericConfigTableKey: vm.selectedGenericConfigTable.GenericConfigTableKey,
                VersionId: vm.selectedGenericConfigTableVersion,
                ReadOnly: true,
                LastModifiedBy: LastModifiedBy,
                LastModifiedDate: createdDatetime
            };

            // 3 => Inspection. 4 => Report.
            if (vm.genericGroupTypeId === '3' || vm.genericGroupTypeId === '4') {
                vm.valueRow.ItemKey = vm.ItemId;
            } else {
                vm.valueRow.ItemId = vm.ItemId;
            }

            var i = 0;
            vm.cFields.forEach(function (element) {
                if (element === '') return;
                var genericValueFieldKey = breeze.core.getUuid();
                var fieldRow = //vm.LocalManager.createEntity('T_GenericValueField',
                {
                    GenericValueFieldKey: genericValueFieldKey,
                    GenericValueRowKey: valueRowKey,
                    GenericConfigFieldKey: element.GenericConfigFieldKey,
                    ReadOnly: true,
                    LastModifiedDate: createdDatetime,
                    LastModifiedBy: LastModifiedBy//'66EFD2FE-3A20-E311-9401-005056A203BC'                                
                };
                vm.entities[i++] = fieldRow;
            });
        }


        function createComponent() {
            if (vm.IsSelect) {
                vm.valueRow=vm.LocalManager.createEntity('T_GenericValueRow', vm.valueRow);
                var i = 0;
                vm.entities.forEach(function (element) {
                    if (element === '') {
                        i++;
                         return;
                    }
                    vm.entities[i++]=vm.LocalManager.createEntity('T_GenericValueField', element);
                });
                createObject();

            } else {
                vm.LocalManager.detachEntity(vm.currentObject);
                vm.LocalManager.detachEntity(vm.objectObjectTypeRow);
                vm.LocalManager.detachEntity(vm.T_Object_Actor);
                
                vm.LocalManager.detachEntity(vm.valueRow);
                vm.entities.forEach(function (element) {
                    if (element === '') return;
                    vm.LocalManager.detachEntity(element);
                });
            }

        }

        function createObject()
            {
            vm.currentObject = vm.LocalManager.createEntity('T_Object', {
                    //ObjectID: 
                    ParentObjectID: $routeParams.objectId,
                    OldID: null,
                    ManufacturerID: null,
                    ManufacturerYear: null,
                    ManufacturerNo: null,
                    ExternalNo: vm.valueRow.GenericValueRowKey,
                    ApprovalNo: null,
                    DrawingNo: null,
                    UnitNo: null,
                    CreatedByUserID: authService.getUserInfo().LegacyUserId,
                    CreatedDateTime: new Date(), // TODO: Ensure this is the correct way to get current datetime. It currently uses the browser's timezome iirc. - THM
                    LastModifiedByUserID: null,
                    LastModifiedDateTime: null,
                    CheckedOutByUserID: null,
                    InternalDescription: null,
                    ExternalDescription: null,
                    ObjectStatusID: null,
                    LocalLocation: null,
                    LocalLocationShort: null
                });

                vm.objectId = vm.currentObject.ObjectID;

                vm.objectObjectTypeRow = {
                    ObjectID: vm.objectId,
                    ObjectTypeID: 21,
                    ObjectSubTypeGuid: null
            };

            vm.T_Object_Actor=  vm.currentObject.entityAspect.entityManager.createEntity('T_Object_Actor', {
                    ActorID: 30786,//facilityId,
                    ActorTypeID: FI_ENUMS.ACTOR_TYPE.FACILITY,
                    ObjectID: vm.currentObject.ObjectID
                    });

            vm.objectObjectTypeRow=vm.LocalManager.createEntity('T_Object_ObjectTypes', vm.objectObjectTypeRow);
        }

      //get config field index from mapping dictionary

      function GetValueField(id) {
          return vm.entities[vm.getIndexWithTagId(id)];
      }

      function GetConfigField(id) {
          return vm.cFields[vm.getIndexWithTagId(id)];
      }


      function IsEntityExists(id) {
          return vm.entities.length > vm.getIndexWithTagId(id);
      }
  }

})();
