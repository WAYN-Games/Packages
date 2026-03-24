# Diagnostic des problemes

Problemes courants organises par symptome.

## La compilation echoue avec "command not found: docfx"

**Cause** : DocFX n'est pas installe ou n'est pas dans votre PATH.

**Solution** :
1. Installez DocFX : `dotnet tool install -g docfx`
2. Verifiez : `docfx --version`
3. S'il est installe mais introuvable, ajoutez le repertoire des outils .NET a votre PATH (generalement `~/.dotnet/tools`)

## La compilation affiche des avertissements de compilation C#

**Cause** : DocFX analyse vos fichiers source C# et signale des erreurs de compilation lorsque les assemblies specifiques a Unity (UnityEngine, UnityEditor) ne peuvent pas etre resolues en dehors de l'editeur Unity.

**Solution** : Ces avertissements sont attendus et n'affectent pas la compilation. La documentation de l'API est tout de meme generee. La convention `METADATA_ALLOW_COMPILATION_ERRORS` est definie sur `true` par defaut pour gerer ce cas.

## Le selecteur de version n'apparait pas

**Cause** : Il n'y a qu'une seule version dans le repertoire de sortie, ou `versions.js` n'a pas ete genere.

**Solution** :
1. Verifiez que `Documentation~/versions.js` existe apres la compilation
2. Compilez avec au moins deux versions presentes (incrementez `version` dans `package.json` et recompilez)
3. Si vous consultez le site localement via `file://`, certains navigateurs bloquent le chargement de JavaScript — utilisez plutot un serveur HTTP local

## Le selecteur de langue n'apparait pas

**Cause** : Le fichier `doctools.languages.json` n'existe pas, ou aucune traduction n'a ete compilee.

**Solution** :
1. Ajoutez une langue : `doctools.sh add-language ./MyPackage fr`
2. Traduisez au moins un fichier et marquez-le comme termine
3. Recompilez — le selecteur de langue apparait lorsque plus d'une langue est compilee
4. Verifiez que `Documentation~/languages.js` existe et liste plusieurs langues

## Les pages traduites ont une mise en page incorrecte

**Cause** : Le fichier `toc.md` traduit est utilise comme table des matieres racine de la barre de navigation au lieu de la table des matieres de la barre laterale.

**Solution** : Le fichier `toc.md` traduit ne doit contenir que la navigation de la barre laterale (la liste des pages du manuel). La table des matieres racine de la barre de navigation (Manual, API, LICENSE, CHANGELOG) est toujours generee a partir de l'anglais. Si vous voyez la liste complete des pages dans la barre de navigation, verifiez que le `toc.md` de votre locale est structure comme une table des matieres de barre laterale, et non comme une table des matieres racine.

## Les liens vers LICENSE/CHANGELOG sont casses dans les pages traduites

**Cause** : Les chemins relatifs dans la table des matieres racine ne tiennent pas compte de l'imbrication du sous-repertoire de langue.

**Solution** : Mettez a jour DocTools vers la derniere version. Ce probleme a ete corrige dans la generation de la table des matieres racine tenant compte des langues.

## Les diagrammes Mermaid ne s'affichent pas

**Cause** : Le bloc de code n'est pas marque avec le bon langage.

**Solution** : Utilisez `` ```mermaid `` (et non `` ```mmd `` ou `` ```diagram ``). Le moteur de rendu Mermaid recherche la classe CSS `lang-mermaid`, que DocFX genere a partir de la balise de langage du bloc de code delimite.

## La recherche ne fonctionne pas

**Cause** : L'index de recherche (`index.json`) n'a pas ete genere, ou JavaScript est bloque.

**Solution** :
1. Verifiez que `index.json` existe dans le repertoire de sortie de la version
2. Si vous consultez le site via `file://`, passez a un serveur HTTP local — les navigateurs restreignent `fetch()` et `XMLHttpRequest` sur les origines `file://`
3. Pour la recherche inter-paquets, verifiez que `packages.json` existe a la racine du site

## La compilation reussit mais le site n'a pas de style

**Cause** : Le chemin du modele n'est pas resolu correctement.

**Solution** :
1. Verifiez que le depot DocTools est intact (le repertoire `templates/` existe)
2. Verifiez que `styles/main.css` existe dans la sortie de compilation (`Documentation~/v{version}/styles/`)
3. Si le repertoire du modele a ete deplace, assurez-vous que le chemin dans `docfx-gen.sh` est resolu correctement

## "No languages configured" lors de l'execution de translate

**Cause** : Le fichier `Documentation/doctools.languages.json` n'existe pas.

**Solution** : Ajoutez d'abord une langue :
```bash
doctools.sh add-language ./MyPackage fr
```

## La traduction affiche des fichiers comme en attente apres translate-done

**Cause** : Le fichier source anglais a ete modifie apres l'execution de `translate-done`, ou le mauvais chemin relatif a ete utilise.

**Solution** :
1. Verifiez que le chemin relatif correspond exactement (par exemple, `getting-started.md`, et non `./getting-started.md`)
2. Verifiez que le fichier source anglais n'a pas ete modifie depuis l'execution de `translate-done`
3. Relancez `translate-done` si le fichier source anglais a ete mis a jour

## Etapes suivantes

- [Commandes](commands.md) — Reference des commandes CLI
- [Configuration](configuration.md) — Fonctionnement des parametres
- [Modele](template.md) — Fonctionnalites du modele
