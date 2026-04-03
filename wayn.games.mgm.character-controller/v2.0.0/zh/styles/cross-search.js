(function () {
  var manifest = null;
  var remoteIndexes = {};
  var currentPackage = null;
  var debounceTimer = null;
  var container = null;
  var fetching = false;

  // Detect current package from URL path: /wayn.games.tools.package-forge/v1.0.0/...
  var pathParts = window.location.pathname.split('/').filter(Boolean);
  if (pathParts.length >= 2) {
    currentPackage = pathParts[0];
  }

  // Detect current language from URL: /pkg/vX.Y.Z/{lang}/... or /pkg/vX.Y.Z/...
  var currentSearchLang = '';
  if (pathParts.length >= 3) {
    // pathParts[1] = vX.Y.Z, pathParts[2] might be a 2-3 char lang code
    var maybeLang = pathParts[2];
    if (maybeLang && /^[a-z]{2,3}(-[A-Za-z]+)?$/.test(maybeLang) &&
        maybeLang !== 'api' && maybeLang !== 'manual') {
      currentSearchLang = maybeLang + '/';
    }
  }

  // Find the site root (go up from /pkg/vX.Y.Z/some/page.html to /)
  var siteRoot = '/';

  function fetchJSON(url) {
    return new Promise(function (resolve, reject) {
      var xhr = new XMLHttpRequest();
      xhr.open('GET', url, true);
      xhr.onload = function () {
        if (xhr.status === 200) {
          try { resolve(JSON.parse(xhr.responseText)); }
          catch (e) { reject(e); }
        } else {
          reject(new Error('HTTP ' + xhr.status));
        }
      };
      xhr.onerror = function () { reject(new Error('Network error')); };
      xhr.send();
    });
  }

  function loadManifestAndIndexes() {
    if (manifest !== null) return Promise.resolve();
    if (fetching) return Promise.resolve();
    fetching = true;

    return fetchJSON(siteRoot + 'packages.json')
      .then(function (pkgs) {
        manifest = pkgs;
        var promises = [];
        manifest.forEach(function (pkg) {
          if (pkg.name === currentPackage) return;
          if (remoteIndexes[pkg.name]) return;
          var url = siteRoot + pkg.name + '/v' + pkg.latestVersion + '/index.json';
          promises.push(
            fetchJSON(url)
              .then(function (entries) {
                remoteIndexes[pkg.name] = {
                  entries: entries,
                  title: pkg.title,
                  version: pkg.latestVersion
                };
              })
              .catch(function () { /* skip unavailable packages */ })
          );
        });
        return Promise.all(promises);
      })
      .catch(function () { manifest = []; })
      .then(function () { fetching = false; });
  }

  function scoreEntry(entry, words) {
    var score = 0;
    var title = (entry.title || '').toLowerCase();
    var keywords = (entry.keywords || '').toLowerCase();
    var summary = (entry.summary || '').toLowerCase();

    for (var i = 0; i < words.length; i++) {
      var w = words[i];
      if (title.indexOf(w) !== -1) score += 10;
      if (keywords.indexOf(w) !== -1) score += 5;
      if (summary.indexOf(w) !== -1) score += 2;
    }
    return score;
  }

  function search(query) {
    var words = query.toLowerCase().split(/\s+/).filter(function (w) { return w.length > 0; });
    if (words.length === 0) return [];

    var results = [];
    Object.keys(remoteIndexes).forEach(function (pkgName) {
      var idx = remoteIndexes[pkgName];
      var pkgResults = [];

      Object.keys(idx.entries).forEach(function (key) {
        var entry = idx.entries[key];
        var score = scoreEntry(entry, words);
        if (score > 0) {
          pkgResults.push({
            title: entry.title,
            href: siteRoot + pkgName + '/v' + idx.version + '/' + currentSearchLang + entry.href,
            summary: entry.summary || '',
            score: score,
            packageTitle: idx.title
          });
        }
      });

      pkgResults.sort(function (a, b) { return b.score - a.score; });
      results = results.concat(pkgResults.slice(0, 5));
    });

    return results;
  }

  function getOrCreateContainer() {
    if (container) return container;
    var searchResults = document.getElementById('search-results');
    if (!searchResults) return null;

    container = document.createElement('div');
    container.id = 'cross-search-results';
    searchResults.appendChild(container);
    return container;
  }

  function render(results, query) {
    var el = getOrCreateContainer();
    if (!el) return;

    if (results.length === 0) {
      el.innerHTML = '';
      return;
    }

    var html = '<div class="cross-search-header">Results from other packages</div>';
    var words = query.split(/\s+/).filter(function (w) { return w.length > 0; });

    results.forEach(function (r) {
      var brief = r.summary;
      if (brief.length > 200) brief = brief.substring(0, 200) + '...';

      // Simple highlight
      words.forEach(function (w) {
        var re = new RegExp('(' + w.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + ')', 'gi');
        brief = brief.replace(re, '<mark>$1</mark>');
      });

      var titleHtml = r.title;
      words.forEach(function (w) {
        var re = new RegExp('(' + w.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + ')', 'gi');
        titleHtml = titleHtml.replace(re, '<mark>$1</mark>');
      });

      html += '<div class="sr-item">';
      html += '<div class="item-title"><a href="' + r.href + '?q=' + encodeURIComponent(query) + '">' + titleHtml + '</a>';
      html += '<span class="cross-search-badge">' + r.packageTitle + '</span></div>';
      html += '<div class="item-href">' + r.href + '</div>';
      html += '<div class="item-brief">' + brief + '</div>';
      html += '</div>';
    });

    el.innerHTML = html;
  }

  function clearResults() {
    if (container) container.innerHTML = '';
  }

  function onSearchInput() {
    var input = document.getElementById('search-query');
    if (!input) return;

    input.addEventListener('keyup', function () {
      var query = input.value;
      clearTimeout(debounceTimer);

      if (!query || query.length < 3) {
        clearResults();
        return;
      }

      debounceTimer = setTimeout(function () {
        loadManifestAndIndexes().then(function () {
          var results = search(query);
          render(results, query);
        });
      }, 300);
    });
  }

  // Initialize when DOM is ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', onSearchInput);
  } else {
    onSearchInput();
  }
})();
