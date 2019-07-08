var INDEX = {};

(function() {

  function ready() {

    var range = _.range(5);

    var ctable$;

    ctable$ = $('#ctable');
    
    
    _.each(range, function(i) {
      var pfix = '00' + i;
      var vs$ = [];
      var vs = [ 'ui', 're' ];

      pfix = pfix.substring(pfix.length - 2);

      ctable$.append(rQ.tr(pfix).append(

      rQ.td().append(rQ.pre().append(vs$[0] = rQ.code('', 'javascript'))),
      //
      rQ.td().append(rQ.pre().append(vs$[1] = rQ.code('', 'javascript')))));

      _.each(vs, function(folder, i) {
        $.ajax({
          'url' : folder + '/code' + pfix + '.txt',
          'dataType' : "text",
          'success' : function(data) {
            vs$[i].text(data);
            hljs.highlightBlock(vs$[i].get(0));
          }
        });
      });

    });

  }

  INDEX.ready = ready;
})();
