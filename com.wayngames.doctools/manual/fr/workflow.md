---
uid: workflow
---

# Cycle de vie des packages et workflow de documentation

Cette page decrit le workflow de bout en bout pour gerer des packages avec une documentation versionnee, de la configuration initiale au deploiement automatise.

## Cycle de vie des packages

### Configuration initiale

Ces etapes sont effectuees une seule fois par package.

#### 1. Creer le package

Creez un package Unity avec un fichier `package.json` contenant au minimum :

```json
{
  "name": "com.wayngames.mypackage",
  "displayName": "My Package",
  "version": "1.0.0",
  "author": { "name": "WAYN Games" }
}
```

#### 2. Initialiser la documentation

```bash
doctools.sh init ./MyPackage
```

Cela cree le dossier `Documentation/` avec une table des matieres, une page d'accueil et un guide de demarrage.

#### 3. Ajouter des langues (optionnel)

Si vous souhaitez une documentation multilingue, ajoutez chaque langue cible :

```bash
doctools.sh add-language ./MyPackage fr
doctools.sh add-language ./MyPackage zh
```

#### 4. Ajouter le workflow CI/CD

Ajoutez `.github/workflows/docs.yml` au depot du package (voir [Configuration CI/CD](#adding-a-new-package) ci-dessous).

#### 5. Premier commit et release

```bash
git add Documentation/ .github/
git commit -m "docs: initialize documentation"
git tag v1.0.0
git push origin main --tags
```

---

### Workflow de developpement de fonctionnalites

Ceci est le workflow quotidien pour ajouter des fonctionnalites avec leur documentation.

#### 1. Creer une branche de fonctionnalite

```bash
git checkout main
git pull origin main
git checkout -b feature/my-feature
```

#### 2. Developper

Ecrivez du code, ajoutez ou mettez a jour les pages de documentation dans `Documentation/`, et commitez au fur et a mesure :

```bash
# ... modifier les fichiers ...
git add src/MyNewFeature.cs Documentation/my-new-feature.md Documentation/toc.md
git commit -m "feat: add my new feature"
```

Repetez autant que necessaire — plusieurs commits sur la branche de fonctionnalite sont tout a fait acceptables.

#### 3. Fusionner dans main

Lorsque la fonctionnalite est terminee :

```bash
git checkout main
git pull origin main
git merge --no-ff feature/my-feature
git branch -d feature/my-feature
```

---

### Workflow de release

Lorsque vous etes pret a publier une nouvelle version avec une documentation mise a jour.

#### 1. Incrementer la version

Modifiez `package.json` pour mettre a jour le champ `version` (par exemple, `1.0.0` → `1.1.0`). Mettez a jour `CHANGELOG.md` avec les modifications.

```bash
# Modifier package.json et CHANGELOG.md, puis :
git add package.json CHANGELOG.md
git commit -m "chore: bump version to 1.1.0"
```

#### 2. Traduire la documentation (si multilingue)

Verifiez quels fichiers necessitent une traduction :

```bash
doctools.sh translate ./MyPackage
```

Pour chaque fichier indique comme en attente, traduisez la source anglaise et ecrivez le fichier traduit vers le chemin cible affiche dans la sortie. Ensuite, marquez chaque fichier comme termine :

```bash
# Apres avoir ecrit le fichier traduit dans .doctools/locales/fr/my-page.md :
doctools.sh translate-done ./MyPackage fr my-page.md

# Repeter pour chaque fichier et chaque langue
doctools.sh translate-done ./MyPackage fr another-page.md
doctools.sh translate-done ./MyPackage zh my-page.md
doctools.sh translate-done ./MyPackage zh another-page.md
```

Commitez les traductions :

```bash
git add Documentation/.doctools/locales/
git commit -m "docs: update translations for v1.1.0"
```

#### 3. Compiler et verifier localement

```bash
doctools.sh build ./MyPackage
```

Ouvrez la sortie pour verifier :

```bash
# Windows (Git Bash)
start MyPackage/Documentation~/v1.1.0/index.html

# macOS
open MyPackage/Documentation~/v1.1.0/index.html
```

Verifiez que :
- Les pages en anglais s'affichent correctement
- Les pages traduites s'affichent avec la bonne mise en page
- Le selecteur de version liste toutes les versions
- Le selecteur de langue apparait et fonctionne correctement

#### 4. Taguer et pousser

```bash
git tag v1.1.0
git push origin main --tags
```

Le push du tag declenche le workflow GitHub Actions, qui :
1. Clone le depot du package et DocTools
2. Recupere les versions precedemment publiees depuis `gh-pages`
3. Execute `doctools.sh build`
4. Deploie sur GitHub Pages

Le site en ligne inclut le selecteur de version, le selecteur de langue et toutes les traductions.

## Workflow de documentation (CI/CD)

Lorsqu'un tag de version (`v*`) est pousse, le flux automatise suivant s'execute :

```text
Package Repo                    DocTools                        Packages (gh-pages)
    |                               |                               |
    |-- tag v1.0.0 pushed --------->|                               |
    |   docs.yml triggers           |                               |
    |   calls deploy-docs.yml ----->|                               |
    |                               |-- checkout package repo       |
    |                               |-- checkout DocTools            |
    |                               |-- install .NET + DocFX        |
    |                               |-- fetch existing v*/ -------->|
    |                               |   from gh-pages               |
    |                               |-- doctools.sh build           |
    |                               |-- deploy Documentation~/ --->|
    |                               |   to /{package.name}/         |
    |                               |                               |-- GitHub Pages serves
    |                               |                               |   docs.wayn.games/{name}/
```

### Etape par etape

1. Un push de tag `v*` declenche le fichier `.github/workflows/docs.yml` du depot du package
2. `docs.yml` appelle le workflow reutilisable situe dans `WAYN-Games/DocTools/.github/workflows/deploy-docs.yml`
3. Le workflow reutilisable :
   - Clone le depot du package et DocTools (depot prive, authentifie via PAT)
   - Installe le SDK .NET et DocFX
   - Effectue un sparse-checkout des repertoires de versions precedemment publiees depuis la branche `gh-pages` de `WAYN-Games/Packages` dans `Documentation~/` afin que le selecteur de version inclue toutes les versions historiques
   - Execute `doctools.sh build` qui genere le site DocFX, `versions.js` et une redirection `index.html` a la racine
   - Deploie l'ensemble de la sortie `Documentation~/` vers `WAYN-Games/Packages` gh-pages sous `/{package.name}/` en utilisant `keep_files: true` (preservant la documentation des autres packages)
4. GitHub Pages sert le contenu a l'adresse `docs.wayn.games/{package.name}/`
5. Le menu deroulant du selecteur de version (alimente par `versions.js`) permet aux utilisateurs de naviguer entre les versions

## Roles des depots

| Depot | Visibilite | Role |
|---|---|---|
| `WAYN-Games/DocTools` | Prive | CLI de build + workflow GitHub Actions reutilisable |
| `WAYN-Games/Packages` | Public | Heberge la branche `gh-pages`, domaine personnalise `docs.wayn.games` |
| Depots de packages | Prive ou Public | Code source + source de documentation, chacun possede un workflow appelant `docs.yml` |

## Structure des URL

Toute la documentation est servie depuis un seul domaine :

```text
docs.wayn.games/
  index.html                              <- page d'accueil listant tous les packages
  com.wayngames.doctools/
    index.html                            <- redirection vers la derniere version
    versions.js                           <- manifeste de versions pour le menu deroulant
    v0.1.0/                               <- sortie DocFX pour v0.1.0
    v0.2.0/                               <- sortie DocFX pour v0.2.0
  com.wayngamesassets.mgm.charactercontroller/
    index.html
    versions.js
    v0.3.0/
```

Le segment de chemin dans l'URL correspond au champ `name` du fichier `package.json` du package.

## Ajouter un nouveau package

1. Initialiser la documentation : `doctools.sh init ./NewPackage`
2. Rediger la documentation et compiler localement : `doctools.sh build ./NewPackage`
3. Ajouter `.github/workflows/docs.yml` au depot du package :

    ```yaml
    name: Documentation

    on:
      push:
        tags: ['v*']
      workflow_dispatch:

    jobs:
      docs:
        uses: WAYN-Games/DocTools/.github/workflows/deploy-docs.yml@main
        with:
          package-name: com.wayngames.newpackage
        secrets:
          doctools-token: ${{ secrets.DOCTOOLS_READ_TOKEN }}
          deploy-token: ${{ secrets.PACKAGES_DEPLOY_TOKEN }}
    ```

4. Ajouter les secrets `DOCTOOLS_READ_TOKEN` et `PACKAGES_DEPLOY_TOKEN` au depot (ou utiliser des secrets au niveau de l'organisation)
5. Taguer une release (`git tag v1.0.0 && git push --tags`) pour declencher le deploiement
6. Ajouter une entree dans `landing/index.html` du depot Packages

## Supprimer une version

Pour supprimer une version publiee :

1. Localement : `doctools.sh version ./PackageDir remove X.Y.Z`
2. Relancer le workflow de documentation (via `workflow_dispatch`) pour mettre a jour `versions.js` et la redirection racine sur le site deploye

## Configuration de l'authentification

### Acces au workflow reutilisable

Etant donne que DocTools est un depot prive, ses workflows reutilisables doivent etre explicitement partages avec l'organisation :

1. Allez dans **https://github.com/WAYN-Games/DocTools/settings/actions**
2. Sous **Access**, selectionnez **"Accessible from repositories in the 'WAYN-Games' organization"**

Il s'agit d'une configuration unique. Sans cela, les autres depots ne peuvent pas appeler le workflow reutilisable.

### Personal Access Tokens

Le systeme utilise deux Personal Access Tokens a granularite fine pour un acces a moindre privilege :

| Nom du secret | Portee | Permission | Objectif |
|---|---|---|---|
| `DOCTOOLS_READ_TOKEN` | `WAYN-Games/DocTools` | Contents: Read-only | Cloner l'outillage de build prive |
| `PACKAGES_DEPLOY_TOKEN` | `WAYN-Games/Packages` | Contents: Read and write | Pousser la documentation compilee vers `gh-pages` |

Les deux secrets doivent etre ajoutes a chaque depot de package (ou stockes comme secrets au niveau de l'organisation pour plus de commodite).
