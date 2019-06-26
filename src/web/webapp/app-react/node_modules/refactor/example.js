var refactor = require('./refactor.js');

var h1 = 'http://mobile.mercadolibre.com.ar'
var h2 = 'http://test2-mobile.mercadolibre.com.ar'
var h3 = 'http://frontend.mercadolibre.com'
var h4 = 'https://api.mercadolibre.com'
var h5 = 'https://www.google.com.ar'
var h6 = 'https://www.google.com.br'

var cases = ['/sites/MLA/search?q=ipod&limit=1', '/sites/MLA/search?q=helado&limit=1'];
var cases2 = ['/#q=ipod', '/#q=helado', '/#q=skate', '/#q=pelota', '/#q=remera', '/#q=fiat', '/#q=ipod', '/#q=helado', '/#q=skate', '/#q=pelota', '/#q=remera', '/#q=fiat', '/#q=ipod', '/#q=helado', '/#q=skate', '/#q=pelota', '/#q=remera', '/#q=fiat'];

// refactor.compare(h1, h2, cases, function(error, diff) {
//     if (error) {
//         console.log(error);
//     }

//     console.log(diff.length);
// });

refactor.bench([h1, h2, h3, h4], cases, function(error, results) {
    if (error) {
        return console.log(error);
    }
    
    Object.keys(results).forEach(function printResult(host) {
        //console.log('Host: %s\n\tavg: %s\n\trpm: %s', host, results[host].time/results[host].calls, 'TODO');
        results[host].errors.forEach(function(error) {
            console.log('[%s] returned error at [%s]: [%j]', error.path, error.host, error.error);
        });
                
        console.log('Host: %s\n\tcalls: %s\n\tavg: %s', host, results[host].calls, results[host].time/results[host].calls);
    });
});