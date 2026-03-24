# Gestion des versions

DocTools compile la documentation pour plusieurs versions d'un package simultanement. Chaque version dispose de son propre repertoire de sortie, et le site inclut un menu deroulant pour passer de l'une a l'autre.

## Fonctionnement des versions

- La **version actuelle** est lue depuis le champ `version` de `package.json`
- Les **versions precedentes** sont decouvertes automatiquement a partir des repertoires `v*/` existants dans le dossier de sortie (`Documentation~/`)
- Chaque compilation produit une sortie dans `Documentation~/v{version}/`
- Un fichier `versions.js` liste toutes les versions disponibles pour le menu deroulant
- Un fichier `index.html` a la racine redirige vers la version actuelle

Aucune liste de versions n'a besoin d'etre maintenue manuellement.

## Flux de travail typique

### 1. Commencer avec votre premiere version

Apres `doctools.sh init`, redigez votre documentation et compilez :

```bash
doctools.sh build ./MyPackage
```

Cela cree `Documentation~/v1.0.0/` (en supposant que `version` est `1.0.0` dans `package.json`).

### 2. Incrementer la version

Lors de la publication d'une nouvelle version :

1. Mettez a jour `version` dans `package.json` (par ex., `1.0.0` → `1.1.0`)
2. Mettez a jour votre documentation si necessaire
3. Mettez a jour `CHANGELOG.md`

### 3. Compiler a nouveau

```bash
doctools.sh build ./MyPackage
```

La compilation produit `Documentation~/v1.1.0/` aux cotes du `v1.0.0/` existant. Les deux versions apparaissent dans le menu deroulant des versions.

### 4. Deployer

Poussez un tag git pour declencher le deploiement CI/CD :

```bash
git tag v1.1.0
git push --tags
```

Voir [Flux de travail CI/CD](workflow.md) pour le pipeline de deploiement automatise.

## Selecteur de version

Le site compile inclut un menu deroulant de version dans la barre de fil d'Ariane. Il est alimente a l'execution a partir de `versions.js` :

```javascript
window.DOC_VERSIONS = [{"version":"1.1.0"},{"version":"1.0.0"}];
```

Lorsqu'un utilisateur selectionne une version differente, il est redirige vers la page d'accueil de cette version. Si l'utilisateur consulte une page traduite, le selecteur tente de naviguer vers la meme langue dans la nouvelle version, en se rabattant sur l'anglais si cette langue n'est pas disponible.

## Supprimer d'anciennes versions

Pour supprimer une version precedemment compilee :

```bash
doctools.sh version ./MyPackage remove 1.0.0
```

Cela supprime le repertoire `Documentation~/v1.0.0/`. Vous ne pouvez pas supprimer la version actuelle.

Pour lister toutes les versions :

```bash
doctools.sh version ./MyPackage list
```

## Structure de sortie

Apres plusieurs versions, la sortie ressemble a ceci :

```
Documentation~/
├── index.html          # Redirection vers v1.1.0
├── versions.js         # [{"version":"1.1.0"},{"version":"1.0.0"}]
├── languages.js        # Donnees du selecteur de langue (si multilingue)
├── v1.1.0/             # Version actuelle
│   ├── index.html
│   ├── manual/
│   ├── api/
│   └── styles/
└── v1.0.0/             # Version precedente
    ├── index.html
    ├── manual/
    ├── api/
    └── styles/
```

## Prochaines etapes

- [Traduction](translation.md) — Ajouter le support multilingue
- [Flux de travail CI/CD](workflow.md) — Deploiement automatise lors d'un push de tag
- [Commandes](commands.md) — Reference complete de la CLI
