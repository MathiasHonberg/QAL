(function () {
  'use strict';

  angular
    .module('forceinspect.genericField.QAL')
      .controller('QalConfigurationPanelController', QalConfigurationPanelController);

  QalConfigurationPanelController.$inject = ['$rootScope','$scope', 'breeze','$q', 'queryService', 'authService', 'dataService', 'GRAPH_URL', 'autosaverService',
    'qAlGenericConfigDataService', '$http', '$routeParams', 'FI_ENUMS', 'QALGRAPH_URL', 'actorDataService', 'jobDataService', 'FAL_ENUMS'];

  function QalConfigurationPanelController($rootScope,$scope, breeze, $q, queryService, authService, dataService, GRAPH_URL, autosaverService,
    qAlGenericConfigDataService, $http, $routeParams, FI_ENUMS, QALGRAPH_URL, actorDataService, jobDataService, FAL_ENUMS) {

    var vm = this;
    vm.ItemId = $scope.itemIdentifier;
    vm.genericGroupTypeId = $scope.genericGroupTypeId;
    vm.typeId = $scope.typeId;
    console.log(vm.ItemId);
    console.log($scope.genericGroupTypeId);
    console.log($scope.typeId);
    console.log($scope.item);
    vm.item = $scope.item;
    vm.pageMode = $scope.pageMode;
    vm.hasEditMode = 1;
    vm.isComponentTag = $scope.isComponentTag;
    vm.cobj = $scope.cobj;
    vm.ActorID = void 0;
    vm.jobId = 37089; //hardcode for now
    vm.FoundConfiguredComponents = [];
    vm.hasDebugInfoReadPermission = hasDebugInfoReadPermission;
        
    vm.LocalManager = void 0;
    vm.QALObjectTypes = {
        19: 'QAL-Position',
        20: 'QAL-Meter',
        21: 'QAL-Component'
    };

    vm.QALInspectionTypes = {
      99: 'Quality-Assurance-Level-1',
      100: 'Quality-Assurance-Level-2',
      101: 'Quality-Assurance-Level-3',
      102: 'Annual-Survailance-Test'
    };

    vm.ShowAdditionalComponentButton = 1;
    $scope.IsReadOnly = true;
    if ($scope.isReadOnly !== undefined) {
      $scope.IsReadOnly = ($scope.isReadOnly === '1' || $scope.isReadOnly === true);
      vm.ShowAdditionalComponentButton = $scope.isReadOnly;
    }

    if (vm.isComponentTag === '1' && vm.item !== undefined) {
      if (vm.item.IsReadOnly !== undefined) {
        $scope.IsReadOnly = vm.item.IsReadOnly;
      } else {
        $scope.IsReadOnly = false;
      }
    }

    $scope.getTemplateUrl = function () {
      var folderPath = '';
      var lastPath = '';

      if (vm.GenericGroupType.Object === vm.genericGroupTypeId) {
        folderPath = 'OBJECT';
        lastPath=vm.QALObjectTypes[$scope.typeId];
      } else if (vm.GenericGroupType.Inspection === vm.genericGroupTypeId) {
        folderPath = 'INSPECTION';
        lastPath = vm.QALInspectionTypes[$scope.typeId];
      }

      var link = 'app/sections/qal/' + folderPath + '/' + lastPath + '.html';
      
      //console.log(link);

      return link;
    };

    vm.selectedGenericConfigTableVersion = void 0;
    vm.isNew = false;
    vm.isEdit = false;
    vm.GroupTypeName = '';
    vm.itemFieldDesc = 'ItemId';

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
      Lookup: 7
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
    vm.createGenericInfoEntity = createGenericInfoEntity;
    vm.CreateObject = createObject;
    vm.CreateComponent = createComponent;
    vm.IsSelect = void 0;
    vm.styles = void 0;
    vm.load = load;
    vm.TryForActor = 0;
    vm.components.forAddtionalInstrumenttype = [];
    vm.addAddtionalComponents = addAddtionalComponents;
    //vm.loadCompnentObjectWithMeterInstrumentType = LoadCompnentObjectWithMeterInstrumentType;
    vm.InstrumentTypeFiledKey = 'FBBE24C0-EB55-4379-905C-58C2DDCBFA8B';
    vm.EmissionTypeLookUpTableKey = '2c69c721-4a56-41de-9158-9a5b27b96ca4';
    vm.MeterComponents = [];
    vm.valueRowsItemIdMapping = void 0;
    vm.LoadMeterComponentsDone = false;
    vm.instrumentTypesForComponent = [];
    vm.user = authService.getUserInfo();
    vm.Actor = void 0;
    vm.Job = void 0;

    vm.hasValidationErrors = false;
    vm.validationErrorMessage = '';

    $scope.$on('hasValidationErrors', function (event, result) {
      vm.hasValidationErrors = result.data.status;
      vm.validationErrorMessage = result.data.text;
    });

    $scope.$watch('vm.LoadMeterComponentsDone', function () {
      if (vm.LoadMeterComponentsDone === true) {
        loadCompnentObjectWithMeterInstrumentType();
      }
    });

    $scope.$watch('vm.ActorID', function () {
      if (vm.ActorID !== undefined) {
        loadActor();
      }
    });

    queryService.delay($scope, 400, vm.LoadGenericConfigTable);

    function loadMetersComponentObjectIds() {
      if (vm.typeId !== FI_ENUMS.OBJECT_TYPE.QAL_METER)
        return;

      vm.FoundConfiguredComponents = [];

      dataService.getLocalManager().then(function (manager) {
        var query = new breeze.EntityQuery()
            .where('ParentObjectID', breeze.FilterQueryOp.Equals, $routeParams.objectId)
            .where('ObjectObjectTypes', breeze.FilterQueryOp.All, 'ObjectTypeID', breeze.FilterQueryOp.Equals, FI_ENUMS.OBJECT_TYPE.QAL_COMPONENT)
            .expand('ObjectObjectTypes');
        queryService.createQueryHelper(qAlGenericConfigDataService.getObjects(query, manager))
          .promise.then(function(data) {
            console.log(data);
            _.forEach(data.results,
              function (v) {
                if (v.ObjectObjectTypes[0]!== undefined
                  && v.ObjectObjectTypes[0].ObjectTypeID === FI_ENUMS.OBJECT_TYPE.QAL_COMPONENT)
                    vm.MeterComponents.push(v.ObjectID);
              });

            vm.LoadMeterComponentsDone=true;
          });
      });
    }

    function addAddtionalComponents() {
      //vm.components.forAddtionalInstrumenttype.push(new Date());
      vm.components.forAddtionalInstrumenttype.push({ 'IsReadOnly': false });
    }

    // Load configurations
    vm.LocalManager = void 0;

    function loadActor() {
      if (vm.Actor !== undefined) return;
      var query = new breeze.EntityQuery().where('ActorID', breeze.FilterQueryOp.Equals, vm.ActorID);

      loadJob();
      var actorPromise = queryService.createQueryHelper(actorDataService.getActors(query, vm.LocalManager));
      actorPromise.promise.then(function (data) {
          vm.Actor = data.results[0];
      });
    }

    function loadJob() {
      getJobPromise().promise.then(function (data) {
        vm.Job = data.results[0];
      });
    }

    function getJobPromise() {
      var query = new breeze.EntityQuery()
        .where('InspectionAreaId', breeze.FilterQueryOp.Equals, 12)
        .where('JobActors', 'ANY','ActorId',breeze.FilterQueryOp.Equals, vm.ActorID)
        .expand('JobActors');//28673

      return queryService.createQueryHelper(jobDataService.getJobs(query, vm.LocalManager));
    }

    function loadActorId(initiatorGuid) {
      if (vm.TryForActor > 2)
        return;

      if (vm.cobj !== null && vm.cobj !== undefined &&vm.cobj.ObjectActors[0] !== undefined) {
        vm.ActorID = vm.cobj.ObjectActors[0].ActorID;
      } else {
        //NOTE : we have to select dynamically
        var LastModifiedBy = initiatorGuid;

        if (initiatorGuid === undefined || initiatorGuid === null) { //'A04AD225-E46D-EA11-A834-005056A73DB9';
          vm.user = authService.getUserInfo();

          if (vm.user !== undefined) {
            LastModifiedBy = vm.user.InitiatorGuid;
          }
        }

        var query = new breeze.EntityQuery()
          .where('InitiatorGuid', breeze.FilterQueryOp.Equals, LastModifiedBy)
          .expand('ContactActor');

        dataService.getLocalManager().then(function(manager) {
          vm.genericConfigurations =
            queryService.createQueryHelper(qAlGenericConfigDataService.getContacts(query, manager));
          vm.genericConfigurations.promise.then(function(data) {
            console.log(data);
                        
            if (data.results.length===1 && data.results[0] !== undefined) {
              if (data.results[0].ContactActor[0] !== undefined) {
                vm.ActorID = data.results[0].ContactActor[0].ActorID;
              } 
            }
          });
        });
      }
    }

    function loadGenericConfigTable() {
      vm.valueRowsItemIdMapping = {};
      if ($routeParams.objectId === null || $routeParams.objectId === undefined) {
        vm.isComponentTag = 0;
        vm.components.forInstrumenttype = {};
        vm.hasEditMode = 0;
      } else {
        loadMetersComponentObjectIds();
      }

      if (vm.isComponentTag === '1') {
        if ($routeParams.objectId === null || $routeParams.objectId === undefined) {
          return;
        }

        vm.styles = 'border: 1px solid gray; padding: 1px; margin: 1px;';
        loadActorId();
      }

      if (vm.GenericConfigTableKey === undefined)
          return;

      vm.showVersionMismatchError = false;

      dataService.getLocalManager().then(function (manager) {
        vm.LocalManager = manager;
        var query = new breeze.EntityQuery()
          .where('GenericConfigTableKey', breeze.FilterQueryOp.Equals, vm.GenericConfigTableKey)
          .where('GenericGroupTypeId', breeze.FilterQueryOp.Equals, vm.genericGroupTypeId)
          .where('VersionId', breeze.FilterQueryOp.Equals, vm.selectedGenericConfigTableVersion)
          .where('GroupId', breeze.FilterQueryOp.Equals, vm.typeId)
          .expand('T_GenericConfigField.T_GenericConfigLookupTable.T_GenericConfigLookupField.T_GenericConfigLookupFieldMapping1.T_GenericConfigLookupField');

        vm.genericConfigurations = queryService.createQueryHelper(
          qAlGenericConfigDataService.getGenericConfigTables(query, manager));
        vm.genericConfigurations.promise.then(load);
      });
    }

    function load(data) {
      vm.genericConfigTable = data.results;
      vm.selectedGenericConfigTable = data.results[0];
      vm.showVersionMismatchError = vm.selectedGenericConfigTable === undefined
        || vm.selectedGenericConfigTable.VersionId !== vm.selectedGenericConfigTableVersion;

      //populateConfigFields();
      loadGenericValueRows();
    }
        
    function loadGenericValueRows() {
      if (vm.GenericConfigTableKey === undefined)
        return;

      vm.showVersionMismatchError = false;
      vm.ItemIdentifiers = [];

      var itemIdColumn = 'ItemId';
      vm.ItemIdentifiers.push(vm.ItemId);

      if (vm.genericGroupTypeId === '3' || vm.genericGroupTypeId === '4') {
        itemIdColumn = 'ItemKey';
      }

      var query = void 0;
      console.log(vm.item);

      if (vm.isComponentTag === '1' && vm.item !== undefined) {
        if (vm.item.ItemId !== undefined) {
          vm.IsSelect = true;
          $scope.IsReadOnly = true;
        } 
        query = new breeze.EntityQuery()
          .where('GenericConfigTableKey', breeze.FilterQueryOp.Equals, vm.GenericConfigTableKey)
          .where('VersionId', breeze.FilterQueryOp.Equals, vm.selectedGenericConfigTableVersion)
          .where(itemIdColumn, breeze.FilterQueryOp.Equals, vm.item.ItemId)
          .expand('T_GenericValueField');
      }
      else if (vm.ItemId > 0) {
        query = new breeze.EntityQuery()
          .where('GenericConfigTableKey', breeze.FilterQueryOp.Equals, vm.GenericConfigTableKey)
          .where('VersionId', breeze.FilterQueryOp.Equals, vm.selectedGenericConfigTableVersion)
          .where(itemIdColumn, breeze.FilterQueryOp.Equals, vm.ItemId)
          .expand('T_GenericValueField');
      } else if (vm.genericGroupTypeId === '3' || vm.genericGroupTypeId === '4') {
        query = new breeze.EntityQuery()
          .where('GenericConfigTableKey', breeze.FilterQueryOp.Equals, vm.GenericConfigTableKey)
          .where('VersionId', breeze.FilterQueryOp.Equals, vm.selectedGenericConfigTableVersion)
          .where(itemIdColumn, breeze.FilterQueryOp.Equals, vm.ItemId)
          .expand('T_GenericValueField');
          if (vm.pageMode === 'perform') {
          loadInspection();
          }
      }

      if (query === undefined || query === null) {
        populateConfigFields([]);
      } else {
        dataService.getLocalManager().then(function (manager) {
          vm.LocalManager = manager;
          vm.genericConfigurations = queryService.createQueryHelper(
            qAlGenericConfigDataService.getGenericValueRow(query, manager));
          vm.genericConfigurations.promise.then(populateConfigFields);
        });
      }
    }
        
    function loadInspection() {
      queryService.createQueryHelper(
        qAlGenericConfigDataService.getInspections(
          new breeze.EntityQuery().where('InspectionGUID', breeze.FilterQueryOp.Equals, vm.ItemId), vm.LocalManager))
            .promise.then(function(data) {
              var inspection = data.results[0];
              if (inspection !== undefined) {
                inspection.LastModifiedByInitiatorKey = vm.user.InitiatorGuid;
              }
            });
    }

    /**
     * Populate config field fields.
     * @param {any} rows
     */
    function populateConfigFields(rows) {
      if (vm.selectedGenericConfigTable === undefined ||
        vm.selectedGenericConfigTable.GenericConfigTableKey === undefined ||
        vm.selectedGenericConfigTable.GenericConfigTableKey === null)
        return;

      vm.GenericValueFields = {}; // Initialize GenericValueField list
      vm.valueRowKey = void 0;
      vm.genericConfigFields = void 0;
      vm.entities = [];
      vm.cFields = [];

      // Assign T_GenericConfigField for selected configuration.
      var data = vm.selectedGenericConfigTable.T_GenericConfigField;

      var valueRows = rows.results;//vm.selectedGenericConfigTable.T_GenericValueRow;

      // This condition determines, whether do we have to insert the data or edit
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
          var valueField = _.find(vm.GenericValueFields,
            function(egvr) {
              return egvr.GenericConfigFieldKey.toUpperCase() === v.GenericConfigFieldKey.toUpperCase();
            });

          if (valueField !== undefined) {
            vm.entities[i] = valueField;
          } else {
            vm.entities[i] = v.T_GenericValueField[0];
          }
        }

        vm.cFields[i] = v;
        vm.entitiesDictionary[v.TagId] = i++;
                
        if (v.GenericConfigLookupTableKey === null)
          return;    

        vm.GenericLookUpTableKeysMapping[v.TagId] = v.GenericConfigLookupTableKey; //mapping config fieldkey and lookuptable key

        // Mapping lookuptable with posible list of data/LookUpTables.
        if (vm.GenericLookUpFields[v.GenericConfigLookupTableKey] === undefined && v.T_GenericConfigLookupTable !== undefined) {
          if (v.T_GenericConfigLookupTable.T_GenericConfigLookupField === null)
            return;

          _.forEach(v.T_GenericConfigLookupTable.T_GenericConfigLookupField, function (v1) {
            if (vm.GenericLookUpFields[v1.GenericConfigLookupTableKey] === undefined) {
              vm.GenericLookUpFields[v1.GenericConfigLookupTableKey] = [];
            }

            vm.GenericLookUpFields[v1.GenericConfigLookupTableKey].push(v1);
          });
        }
      });
            
      // Select value in dropdown if we have already inserted value in database.
      if (vm.isEdit) {
        _.forEach(data, function (fv) {
          //vm.lookupFieldSelectedItem[fv.GenericConfigFieldKey] = fv;
          if (fv.DataTypeId === vm.DataType.Lookup) {
            if (fv.GenericConfigFieldKey.toUpperCase() === vm.InstrumentTypeFiledKey) {
              //meter object and instrument type field
              //loadCompnentObjectWithMeterInstrumentType(fv);
              vm.instrumentTypesForComponent.push(fv);
              //dependentFields(fv.TagId);
            } else {
              dependentFields(fv.TagId);
            }
          }
        });

        if (vm.instrumentTypesForComponent.length > 0) {
          loadCompnentObjectWithMeterInstrumentType();
          dependentFields(vm.instrumentTypesForComponent[0].TagId);
        }
      }

      // Subscribe auto change.
      if (!vm.isEdit) {
        create();

        if (vm.isComponentTag === '1') {
          if (vm.item !== undefined) {
            if (vm.item.T_GenericConfigLookupTable !== undefined) {
              var cFieldKey = vm.item.T_GenericConfigLookupTable.T_GenericConfigField[0]
                .GenericConfigFieldKey;
              var field = vm.GetValueField(cFieldKey);
              field.GenericConfigLookupFieldKey = vm.item.GenericConfigLookupFieldKey;
              dependentFields(cFieldKey);
            } else {
              vm.AllowEnable = true;
            }
          } else {
            vm.AllowEnable = true;
          }
        } else {
          createGenericInfoEntity();
        }

        //_.forEach(vm.entities, function (entity) {
        //  entity.entityAspect.propertyChanged.subscribe();
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

    /**
      * Refresh dependent dropdown values based on parent dropdown selected value
      *  GenericLookUpFields - mapping of [GenericLookUpTableKey][List of Lookupfields]
      *  ----T_GenericConfigLookupFieldMapping1 (contains the mapping of dependent child lookupfields)
      *  -------T_GenericConfigLookupField (dependent child lookupfields)
      *  ------------T_GenericConfigLookupTable
      *  -------------------T_GenericConfigField (control in html needs to refresh with value)
     * @param {any} id A GenericConfigFieldKey
     */
    function dependentFields(id) {
      vm.refreshqalcompnl = true;
      vm.needRefresh = true;
      var fieldValue = vm.entities[vm.getIndexWithTagId(id)];

      if (fieldValue.GenericConfigLookupFieldKey === undefined)
        return;

      var lookUpFieldKey = fieldValue.GenericConfigLookupFieldKey;
      var lookUpTableKey = vm.GenericLookUpTableKeysMapping[id];

      if (lookUpTableKey === undefined) {
        lookUpTableKey = vm.GenericLookUpTableKeysMapping[id.toUpperCase()];
      }

      var initialize = true;
      console.log('lookuptable key ' + lookUpTableKey);

      if (vm.GenericLookUpFields[lookUpTableKey] === undefined)
        return;

      _.forEach(vm.GenericLookUpFields[lookUpTableKey], function (v) { //v is lookupfield list for lookuptable or parent lookupfield
        if (v.GenericConfigLookupFieldKey === lookUpFieldKey) {                    
          _.forEach(v.T_GenericConfigLookupFieldMapping1, function (vf) { //dependent or child lookup field
            if (vf.T_GenericConfigLookupField.T_GenericConfigLookupTable!== null) {
              _.forEach(vf.T_GenericConfigLookupField.T_GenericConfigLookupTable.T_GenericConfigField, function (vcf) { //child lookupfield's configfield
                var tagId = vcf.TagId;

                if (vcf.ParentGenericFieldConfigKey !== null
                  && (vcf.ParentGenericFieldConfigKey.toUpperCase() !== id
                    && vcf.ParentGenericFieldConfigKey.toUpperCase() !== id.toUpperCase()))
                  return;

                if (vm.DependentFieldsDictionary[tagId] === undefined || initialize) {
                  vm.DependentFieldsDictionary[tagId] = [];
                  initialize = false;
                }

                vm.DependentFieldsDictionary[tagId].push(vf.T_GenericConfigLookupField);
              });
            }

            //InstrumentType->EmissionType
            if (vm.hasEditMode === 1 && id === vm.InstrumentTypeFiledKey
              && vf.T_GenericConfigLookupField.GenericConfigLookupTableKey === vm.EmissionTypeLookUpTableKey) {
              if (vm.needRefresh) {
                vm.components.forInstrumenttype = [];
                vm.needRefresh = false;
              }

              //checked if already saved
              var valueRowItemId = vm.valueRowsItemIdMapping[vf.T_GenericConfigLookupField.GenericConfigLookupFieldKey.toUpperCase()];

              if (valueRowItemId !== undefined) {
                vm.FoundConfiguredComponents.push(valueRowItemId);
                vf.T_GenericConfigLookupField.ItemId = valueRowItemId;
                vf.T_GenericConfigLookupField.IsReadOnly = true;
                $scope.IsReadOnly = true;
              } else if (vm.pageMode===undefined) {
                vf.T_GenericConfigLookupField.IsReadOnly = true;
              }

              vm.components.forInstrumenttype.push(vf.T_GenericConfigLookupField);
            }
          });
        }
      });

      console.log(vm.DependentFieldsDictionary);
      console.log(vm.components.forInstrumenttype);
    }

    function getValues(id) {
      return vm.GenericLookUpFields[vm.GenericLookUpTableKeysMapping[id]];
    }
      
    function regexEscape(decimal) {
      if (decimal === undefined || decimal === null)
        return;

      return decimal.toString().replace(',', '.');
    }

    function create() {
      vm.valueRow = {};
      var createdDatetime = new Date();

      if (vm.selectedGenericConfigTable === undefined)
        return;

        //vm.user = vm.user;//authService.getUserInfo();

      if (vm.user === undefined)
        return;

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
        if (element === '')
          return;

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
      vm.hasValidationErrors = false;
      vm.validationErrorMessage = '';
      $rootScope.$broadcast('hasValidationErrors',
      {
        data: {
          status: vm.hasValidationErrors,
          text: vm.validationErrorMessage
        }
      });

      if (vm.IsSelect) {
        if (vm.Actor === undefined) {
          var parentObject = vm.LocalManager.getEntityByKey('T_Object', $routeParams.objectId);

          if (parentObject.ObjectActors.length > 0)
            vm.Actor = parentObject.ObjectActors[0].Actor;

          if (vm.Actor === undefined) {
            vm.IsSelect = false;
            vm.validationErrorMessage = 'select actor';
            vm.hasValidationErrors = true;
            $rootScope.$broadcast('hasValidationErrors',
            {
              data: {
                status: vm.hasValidationErrors,
                text: vm.validationErrorMessage
              }
            });

            return;
          } else {
            vm.ActorID = vm.Actor.ActorID;
          }
        }

        if (vm.Job === undefined) {
          getJobPromise().promise.then(function (data) {
            vm.Job = data.results[0];

            if (vm.Job === undefined) {
              vm.IsSelect = false;
              vm.validationErrorMessage ='No job available for this actor';
              vm.hasValidationErrors = true;
              $rootScope.$broadcast('hasValidationErrors', {
                data: {
                  status: vm.hasValidationErrors,
                  text: vm.validationErrorMessage
                }
              });
            } else {
              vm.IsSelect = true;
              createComponent();
            }
          });
        } else {
          createComponentObject();
          createGenericInfoEntity();
        }
      } else {
        if (vm.currentObject === undefined)
          return;

        vm.LocalManager.detachEntity(vm.currentObject);
        vm.LocalManager.detachEntity(vm.objectObjectTypeRow);
        vm.LocalManager.detachEntity(vm.T_Object_Actor);
        vm.LocalManager.detachEntity(vm.valueRow);

        vm.entities.forEach(function (element) {
          if (element === '')
            return;

          vm.LocalManager.detachEntity(element);
        });

        vm.inspections.forEach(function (element) {
          if (element === '')
            return;

          vm.LocalManager.detachEntity(element);
        });

        vm.inspectionTypes.forEach(function (element) {
          if (element === '')
            return;

          vm.LocalManager.detachEntity(element);
        });

        vm.Calendars.forEach(function (element) {
          if (element === '')
            return;

          vm.LocalManager.detachEntity(element);
        });

        vm.InspectionCalendars.forEach(function (element) {
          if (element === '')
            return;

          vm.LocalManager.detachEntity(element);
        });
      }
    }

    function createGenericInfoEntity() {
      var i = 0;
      if (vm.genericGroupTypeId === '3' && vm.pageMode !== 'perform') {
        
        vm.entities.forEach(function (element) {
          if (element === '') {
            i++;
            return;
          }

          vm.entities[i++] = element;
        });

        return;
      }

      vm.valueRow = vm.LocalManager.createEntity('T_GenericValueRow', vm.valueRow);
      
      vm.entities.forEach(function (element) {
        if (element === '') {
          i++;
          return;
        }

        vm.entities[i++] = vm.LocalManager.createEntity('T_GenericValueField', element);
      });
    }

    function createObject(objectTypeId) {
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
        CreatedByUserID: vm.user.LegacyUserId,
        CreatedDateTime: new Date(), // TODO: Ensure this is the correct way to get current datetime. It currently uses the browser's timezome iirc. - THM
        LastModifiedByUserID: null,
        LastModifiedDateTime: null,
        CheckedOutByUserID: null,
        InternalDescription: null,
        ExternalDescription: null,
        ObjectStatusID: FI_ENUMS.OBJECT_STATUS.OPERATING,
        LocalLocation: null,
        LocalLocationShort: null
      });

      vm.objectId = vm.currentObject.ObjectID;
      vm.objectObjectTypeRow = {
          ObjectID: vm.objectId,
          ObjectTypeID: objectTypeId,
          ObjectSubTypeGuid: null
      };

      //var actor = vm.LocalManager.getEntities('T_Actor')[0];
      vm.T_Object_Actor = vm.LocalManager.createEntity('T_Object_Actor', {
        Actor: vm.Actor,//30786,//facilityId,
        ActorTypeID: FI_ENUMS.ACTOR_TYPE.FACILITY,
        ObjectID: vm.currentObject.ObjectID
      });

      vm.objectObjectTypeRow = vm.LocalManager.createEntity('T_Object_ObjectTypes', vm.objectObjectTypeRow);
    }

    function createComponentObject() {
      vm.inspections = [];
      vm.inspectionTypes = [];
      vm.Calendars=[];
      vm.InspectionCalendars=[];

      createObject(FI_ENUMS.OBJECT_TYPE.QAL_COMPONENT);
      // Create QAL1, QAL2, QAL3 inspections for the new component.
      createInspection(vm.jobId, vm.currentObject, FI_ENUMS.INSPECTION_TYPE.QAL.QUALITY_ASSURANCE_LEVEL1, FI_ENUMS.CALENDAR_ENTRY_TYPE.QAL.QUALITY_ASSURANCE_LEVEL1);
      createInspection(vm.jobId, vm.currentObject, FI_ENUMS.INSPECTION_TYPE.QAL.QUALITY_ASSURANCE_LEVEL2, FI_ENUMS.CALENDAR_ENTRY_TYPE.QAL.QUALITY_ASSURANCE_LEVEL2);
      createInspection(vm.jobId, vm.currentObject, FI_ENUMS.INSPECTION_TYPE.QAL.QUALITY_ASSURANCE_LEVEL3, FI_ENUMS.CALENDAR_ENTRY_TYPE.QAL.QUALITY_ASSURANCE_LEVEL3);
      createInspection(vm.jobId, vm.currentObject, FI_ENUMS.INSPECTION_TYPE.QAL.ANNUAL_SURVAILANCE_TEST, FI_ENUMS.CALENDAR_ENTRY_TYPE.QAL.ANNUAL_SURVAILANCE_TEST);
    }

    function createInspection(jobId, object, inspectionTypeId,calendarTypeId) {
      var cDate = new Date();
      var inspection = vm.LocalManager.createEntity('T_Inspection',
      {
        ObjectID: object.ObjectID,
        CreatedByUserID: vm.user.LegacyUserId,
        CreatedByInitiatorKey: vm.user.InitiatorGuid,
        InspectionDateTime: cDate,
        CreatedDateTime: cDate,
        JobId: vm.Job.JobId,
        InspectionStatusID: FI_ENUMS.INSPECTION_STATUS.IN_PROGRESS
      });

      vm.inspections.push(inspection);

      // T_Inspection_InspectionTypes
      vm.inspectionTypes.push(vm.LocalManager.createEntity('T_Inspection_InspectionTypes', {
        InspectionGUID: inspection.InspectionGUID,
        InspectionTypeID: inspectionTypeId
      }));

      var calendar = vm.LocalManager.createEntity('T_Calendar',
      {
        CalendarGUID: breeze.core.getUuid(),
        Object: object,
        CalendarEntryTypeID: calendarTypeId,
        DateTime: inspection.CreatedDateTime,
        AssignedToUserID: null,
        Done: false,
        CreatedByInspectionGUID: inspection.InspectionGUID,
        ModifiedByInspectionGUID: inspection.InspectionGUID,
        ModifiedOriginalDateTime: null,
        ModifiedOriginalDone: null
      });

      vm.Calendars.push(calendar);

      vm.InspectionCalendars.push(vm.LocalManager.createEntity('T_InspectionCalendar',
      {
        InspectionCalendarGuid: breeze.core.getUuid(),
        InspectionGuid: inspection.InspectionGUID,
        CalendarEntryTypeId: calendar.CalendarEntryTypeID,
        InspectionDatePrevious: null,
        CalendarGuidPrevious: calendar.CalendarGUID,
        InspectionDateNext: null,
        CalendarGuidNext: null
      }));
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

    //MAFH TEST - START
    vm.selectedItemKey = "1E1E4BFD-9E1F-4186-8489-853FDC8EE722";

    if ($routeParams.ItemKey !== undefined) {
      vm.selectedItemKey = $routeParams.ItemKey;
    }

    vm.mID = 592;
    vm.resumeOld = void 0;
    vm.resumeNew = void 0;
    vm.ShowChecklist = false;
    vm.InitialForDebugInfo = false;
    //vm.user = authService.getUserInfo();

    // Only generate the summarytable if an object of the type QalPosition is being shown.
    if (vm.GenericGroupType.Object === vm.genericGroupTypeId
      && vm.typeId === FI_ENUMS.OBJECT_TYPE.QAL_POSITION) {
        vm.GetResumeNewDatabase = GetResumeNewDatabase;
        vm.GetResumeNewDatabase();
    }
        
    function GetResumeNewDatabase() {
      if (vm.ItemId === undefined)
        return;

      var url = QALGRAPH_URL + '/GetResumeNewDatabase' + '?';
      url += '&ItemId=' + vm.ItemId;
      console.log("ITEMID: " + vm.ItemId);

      authService.getAuthToken().then(function (authToken) {
        var promise = $http({
          method: 'GET',
          // GetGraph is the name of the method in the Webservice GraphController.GetResumeNewDatabase()
          url: url,
          contentType: "application/json",
          headers: {
            'X-Auth-Token': authToken
          }
        });

        promise.then(function (response) {
          vm.InitialForDebugInfo = vm.user.Username.toLowerCase() === 'mmi' || vm.user.Username.toLowerCase() === 'txa' || vm.user.Username.toLowerCase() === 'mafh';
          vm.ShowChecklist = false;
          // response.data because of something AngularJs does. normally you could just take response and assign it to traces e.g. vm.traces = response.
          vm.resumeNew = void 0;
          vm.resumeNew = response.data;
          console.log("NEW:" + vm.resumeNew);
        });
      });
  }
    //MAFH TEST - END
    //load component object for instrument type

    function loadCompnentObjectWithMeterInstrumentType() {
      if (vm.MeterComponents.length === 0)
        return;

      console.log(vm.MeterComponents);
      vm.valueRowsItemIdMapping = {};
      var query = new breeze.EntityQuery()
        .where('ItemId', 'in', vm.MeterComponents)
        .expand('T_GenericValueField');

      _.forEach(vm.instrumentTypesForComponent, function (fieldItem) {
        console.log(fieldItem);
        $q.all([
          qAlGenericConfigDataService
            .getGenericValueRow(query)
            .then(function (data) {
              console.log(data);
              _.forEach(data.results, function (vr) {
                _.forEach(vr.T_GenericValueField, function (gvf) {
                  if (gvf.GenericConfigLookupFieldKey === null)
                    return;

                  vm.valueRowsItemIdMapping[gvf.GenericConfigLookupFieldKey.toUpperCase()] = vr.ItemId;
                });
              });
             })
        ]).then(function () {
          dependentFields(fieldItem.GenericConfigFieldKey.toUpperCase());
          loadAdditionalComponents();
        });
      });
    }

    function loadAdditionalComponents() {
      //vm.components.forAddtionalInstrumenttype.push({ 'ItemId': 867126});
      _.forEach(vm.MeterComponents,
        function(mc) {
          if (vm.FoundConfiguredComponents.indexOf(mc) < 0) {
            vm.components.forAddtionalInstrumenttype.push({ 'ItemId': mc, 'IsReadOnly': true });
          }
        });
    }

    function hasDebugInfoReadPermission() {
      return (authService.hasPermission(FAL_ENUMS.PERMISSION.DEBUG_INFO_READ));
    }
  }
})();