$(document)
    .ready(
        function() {

          var search$ = $('#search');
          var result$ = $('#result');
          var insert$ = $('#insert');
          var update$ = $('#update');
          var delete$ = $('#delete');

          var fields$ = $('#editPanel').find('input');

          $('#clearLog').click(clearLog);
          $('#showServices').click(showServices);

          log('warn', 'Starting sending service entries to server ...');

          // hide all action links till after service entry registration ...
          $('a').hide('slow');

          //
          // Register service entries.
          // (Only for demo recommended. Otherwise service entries are already
          // registered at server site eg. by a db table.)
          //
          registerServices(function() {
            log('ok', 'Sending service entries done!');
            $('a').show('slow');
          });

          search$.click(function(e) {

            var searchString = $('#searchString').val();
            searchString = searchString + '%';
            result$.empty().text('-waiting for server reply ...-');

            // call RemoteQuery ...
            rQ.call('selectAddresses', {
              'searchString' : searchString
            }, function(data) {
              var table$, tr$;
              // var list = rQ.toList(data);
              var header = data.header;
              var table = data.table;
              result$.empty();
              if (table && table.length === 0) {
                result$.text('-no data found-');
              } else {
                table$ = $('<table>').appendTo(result$);
                tr$ = $('<tr>').addClass('header').appendTo(table$);
                // header
                $.each(header, function(index, head) {
                  tr$.append($('<th>').text(head));
                });
                // table
                $.each(table, function(index, row) {
                  tr$ = $('<tr>').appendTo(table$);
                  $.each(row, function(index, element) {
                    tr$.append($('<td>').text(element));
                  });
                });
              }
            });
          });

          insert$.click(function(e) {
            callAddressService('insertAddress');
          });
          update$.click(function(e) {
            callAddressService('updateAddress');
          });
          delete$.click(function(e) {
            callAddressService('deleteAddress');
          });

          function callAddressService(serviceId) {
            var parameters = getParameters();
            log('info', 'call ' + serviceId + '  with ' + parameters);
            rQ.call(serviceId, parameters, function(resultData) {
              log('info', serviceId + ' done ...');
              search$.click();
            });
          }

          function getParameters() {
            var parameters = {};
            fields$.each(function() {
              var v$ = $(this);
              parameters[v$.attr('name')] = v$.val();
            });
            return parameters;
          }

          function log(severity, text) {
            var log$ = $('#log');
            log$.append($('<div>', {
              'text' : text,
              'class' : severity
            }));
          }

          function clearLog() {
            $('#log').empty();
          }
          function showServices() {
            rQ.call('ListServices', function(result) {
              if (result.table, result.table.length) {
                $.each(result.table, function(i, row) {
                  var s = '';
                  $.each(row, function(i, e) {
                    s += result.header[i] + '=' + e + ' :: ';
                  });
                  log('info', s);
                });
              }
            });
          }

          /**
           * @memberOf webapp
           */
          function registerServices(doneCb) {
            var s, idWhereClause = 'where FIRST_NAME = :firstName and LAST_NAME = :lastName and CITY = :city';

            var serviceEntries = [
                {
                  'serviceId' : 'selectAddresses',
                  'statements' : 'select FIRST_NAME, LAST_NAME, STREET, CITY, ZIP, COUNTRY from ADDRESS where STREET like :searchString or FIRST_NAME like :searchString or LAST_NAME like :searchString or CITY like :searchString'
                },
                {
                  'serviceId' : 'insertAddress',
                  'statements' : 'insert into ADDRESS (FIRST_NAME, LAST_NAME, STREET, CITY, ZIP, COUNTRY) values (:firstName, :lastName, :street, :city, :zipppp, :country)'
                },

                {
                  'serviceId' : 'updateAddress',
                  'statements' : 'update ADDRESS set STREET = :street, CITY = :city, ZIP = :zip, COUNTRY = :country '
                      + idWhereClause
                }, {
                  'serviceId' : 'deleteAddress',
                  'statements' : 'delete from ADDRESS ' + idWhereClause
                } ];

            s = JSON.stringify(serviceEntries);
            rQ.call(rQ.names.RegisterService, {
              'serviceEntries' : s
            }, function() {
              doneCb();
            });

          }

        });
