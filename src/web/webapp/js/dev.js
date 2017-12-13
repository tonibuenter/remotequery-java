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

    ui = rQ_ui.templateUi();
    view$ = ui.view().addClass('edit-service-ui');
    header$ = rQ_ui.div('section').append(reload$ = rQ_ui.button('reload'),
        ' ', save$ = rQ_ui.button('save'), ' ',
        run$ = rQ_ui.button('save and run'), ' ', back$ = rQ_ui.button('back'));

    serviceIdUi = rQ_ui.inputUi({
      'label' : 'Service Id',
      'type' : 'text'
    });

    rolesUi = rQ_ui.inputUi({
      'label' : 'Roles',
      'type' : 'text'
    });

    statementsUi = rQ_ui.textareaUi({
      'label' : 'Statements',
      'type' : 'text'
    });

    body$ = rQ_ui.div('row').append(
        rQ_ui.div('col s12 red-text darken-4-text ').append(serviceIdUi.view(),
            rolesUi.view()),
        rQ_ui.div(' col s12 grey lighten-2 statements').append(
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
      rQ_ui.toast('relaod...');
      rQ.call('RQService.get', {
        'serviceId' : serviceIdUi.value()
      }, function(data) {
        var e = rQ.toList(data)[0];
        if (e) {
          rQ_ui.toast('relaod done!');
          value(e);
        } else {
          rQ_ui.toast('no service found for:' + serviceIdUi.value());
        }
      });
    }

    function saveService(doneCb) {
      rQ_ui.toast('save service...');
      rQ.call('RQService.save', {
        'SERVICE_ID' : serviceIdUi.value(),
        'ROLES' : rolesUi.value(),
        'statements' : statementsUi.value()
      }, function() {
        rQ_ui.toast('service saved!')
        _.isFunction(doneCb) ? doneCb() : 0;
      });
    }
    function runService() {
      saveService(function() {
        var serviceId = serviceIdUi.value();
        rQ_ui.toast('run: ' + serviceId + '...');
        rQ.call(serviceId, {}, function(data) {
          rQ_ui.toast('done: ' + serviceId);
          rQ_ui.toast('header.length: ' + data.header.length);
          rQ_ui.toast('tablelength: ' + data.table.length);
          rQ_ui.toast('from: ' + data.from);
          rQ_ui.toast('totalCount: ' + data.totalCount);
          rQ_ui.toast('rowsAffected: ' + data.rowsAffected);
        });
      });
    }
  }
  rQ_ui.serviceEditUi = serviceEditUi;

  // EDIT SERVICE UI -end-
})();