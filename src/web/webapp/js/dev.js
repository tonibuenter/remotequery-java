(function() {

  //
  // EDIT SERVICE UI -start-
  //
  /**
   * @memberOf rQ_ui_md_dev
   */
  function serviceEditUi(settings) {

    var ui, view$, header$, body$, serviceIdUi, rolesUi;
    var reload$, save$, run$, back$;

    ui = rQ.ui();
    view$ = ui.view().addClass('edit-service-ui');
    header$ = rQ.div('section').append(reload$ = rQ.button('reload'),
        ' ', save$ = rQ.button('save'), ' ',
        run$ = rQ.button('save and run'), ' ', back$ = rQ.button('back'));

    serviceIdUi = rQ.inputUi({
      'label' : 'Service Id',
      'type' : 'text'
    });

    rolesUi = rQ.inputUi({
      'label' : 'Roles',
      'type' : 'text'
    });

    statementsUi = rQ.textareaUi({
      'label' : 'Statements',
      'type' : 'text'
    });

    body$ = rQ.div('row').append(
        rQ.div('col s12 red-text darken-4-text ').append(serviceIdUi.view(),
            rolesUi.view()),
        rQ.div(' col s12 grey lighten-2 statements').append(
            statementsUi.view()));

    view$.append(header$, body$);

    ui.value = value;

    save$.click(saveService);
    reload$.click(reload);
    run$.click(runService);
    back$.click(ui.done);

    return ui;

    function value(e) {
      if (e) {
        serviceIdUi.value(e.serviceId);
        rolesUi.value(e.roles);
        statementsUi.value(e.statements);
      }
      return {
        'serviceId' : serviceIdUi.value(),
        'roles' : rolesUi.value(),
        'statements' : statementsUi.value()
      }
    }

    function reload() {
      rQ.toast('relaod...');
      rQ.call('RQService.get', {
        'serviceId' : serviceIdUi.value()
      }, function(data) {
        var e = rQ.toList(data)[0];
        if (e) {
          rQ.toast('relaod done!');
          value(e);
        } else {
          rQ.toast('no service found for:' + serviceIdUi.value());
        }
      });
    }

    function saveService(doneCb) {
      rQ.toast('save service...');
      rQ.call('RQService.save', {
        'SERVICE_ID' : serviceIdUi.value(),
        'ROLES' : rolesUi.value(),
        'statements' : statementsUi.value()
      }, function() {
        rQ.toast('service saved!')
        _.isFunction(doneCb) ? doneCb() : 0;
      });
    }
    function runService() {
      saveService(function() {
        var serviceId = serviceIdUi.value();
        rQ.toast('run: ' + serviceId + '...');
        rQ.call(serviceId, {}, function(data) {
          rQ.toast('done: ' + serviceId);
          rQ.toast('header.length: ' + data.header.length);
          rQ.toast('tablelength: ' + data.table.length);
          rQ.toast('from: ' + data.from);
          rQ.toast('totalCount: ' + data.totalCount);
          rQ.toast('rowsAffected: ' + data.rowsAffected);
        });
      });
    }
  }
  rQ.serviceEditUi = serviceEditUi;

  // EDIT SERVICE UI -end-
})();