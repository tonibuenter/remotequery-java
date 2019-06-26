# refactor
A node script to compare http responses. Designed for comparing api responses after a big refactor

- Benchmarking

```javascript
var refactor = require('refactor');

var ar = 'https://www.google.com.ar'
var br = 'https://www.google.com.br'

var cases = ['/#q=ipod', '/#q=helado', '/#q=skate', '/#q=pelota', '/#q=remera', '/#q=fiat'];

refactor.bench([ar, br], cases, function(error, results) {
    if (error) {
        return console.log(error);
    }
    
    Object.keys(results).forEach(function printResult(host) {
        results[host].errors.forEach(function(error) {
            console.log('[%s] returned error at [%s]: [%j]', error.path, error.host, error.error);
        });

        console.log('Host: %s\n\tcalls: %s\n\tavg: %s', 
          host,
          results[host].calls,
          results[host].time/results[host].calls);
    });
});
```

- Comparing (differences is an instance of [diff](https://github.com/flitbit/diff#differences))

```javascript
var refactor = require('refactor');

var ar = 'https://www.google.com.ar'
var br = 'https://www.google.com.br'

var cases = ['/#q=ipod', '/#q=helado', '/#q=skate', '/#q=pelota', '/#q=remera', '/#q=fiat'];

refactor.compare(ar, br, cases, function(error, differences) {
    if (error) {
      return console.log(error);
    }
    console.log(differences);
});
```
