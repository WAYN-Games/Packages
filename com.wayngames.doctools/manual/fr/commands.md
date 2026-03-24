# Commandes

Toutes les commandes DocTools suivent le meme modele :

```bash
doctools.sh <command> <package-dir> [arguments]
```

`<package-dir>` est le chemin vers la racine du package Unity (le repertoire contenant `package.json`).

---

## init

Genere un repertoire `Documentation/` avec des fichiers de depart.

```bash
doctools.sh init <package-dir> [--title TITLE]
```

| Option | Description | Valeur par defaut |
|--------|-------------|-------------------|
| `--title TITLE` | Remplace le titre de la documentation | `displayName` de `package.json` |

### Ce qui est cree

```
Documentation/
├── toc.md              # Table des matieres laterale
├── index.md            # Page d'accueil
├── getting-started.md  # Guide de demarrage
└── Images/
    └── logo.png        # Logo par defaut
```

Si un repertoire `Documentation/` existe deja, la commande se termine sans ecraser les fichiers existants.

### Exemple

```bash
doctools.sh init ./MyPackage
doctools.sh init ./MyPackage --title "My Custom Docs"
```

---

## build

Construit le site de documentation. C'est la commande principale que vous executez pendant le developpement et dans la CI.

```bash
doctools.sh build <package-dir>
```

### Ce que fait cette commande

1. Charge la configuration depuis `package.json` et les conventions
2. Execute `docfx metadata` pour extraire la documentation de l'API C#
3. Execute `docfx build` pour la version anglaise
4. Pour chaque langue configuree avec des fichiers traduits, execute `docfx build` pour cette langue
5. Genere `versions.js`, `languages.js` et un fichier `index.html` de redirection a la racine

### Structure de sortie

```
Documentation~/
├── index.html          # Redirection vers la version courante
├── versions.js         # Donnees du selecteur de version
├── languages.js        # Donnees du selecteur de langue
└── v1.0.0/
    ├── index.html      # Page d'accueil
    ├── manual/         # Pages du manuel (HTML)
    ├── api/            # Reference de l'API (HTML)
    ├── styles/         # CSS et JavaScript
    ├── fr/             # Traduction francaise (si configuree)
    │   ├── manual/
    │   └── ...
    └── zh/             # Traduction chinoise (si configuree)
        └── ...
```

### Exemple

```bash
doctools.sh build ./MyPackage
```

---

## translate

Liste tous les fichiers de documentation necessitant une traduction pour chaque langue configuree.

```bash
doctools.sh translate <package-dir>
```

Pour chaque langue dans `doctools.languages.json`, cette commande compare le hash SHA-256 de chaque fichier source anglais avec le hash stocke lors de la derniere traduction. Les fichiers nouveaux ou modifies depuis leur derniere traduction sont listes avec leurs chemins source et cible.

### Exemple de sortie

```
=== Checking translations for: fr (Français) ===
  Files needing translation:
    - getting-started.md
      Source: /path/to/Documentation/getting-started.md
      Target: /path/to/Documentation/.doctools/locales/fr/getting-started.md

=== Checking translations for: zh (中文) ===
  All files up to date.
```

Voir [Traduction](translation.md) pour le flux de travail complet.

---

## translate-done

Marque un fichier comme traduit en mettant a jour son hash dans le manifeste de traduction.

```bash
doctools.sh translate-done <package-dir> <lang-code> <relative-path>
```

| Argument | Description |
|----------|-------------|
| `<lang-code>` | Code de langue (par ex., `fr`, `zh`, `ja`) |
| `<relative-path>` | Chemin relatif a `Documentation/` (par ex., `getting-started.md`) |

Executez cette commande apres avoir ecrit le fichier traduit dans `.doctools/locales/{lang}/{path}`. Elle enregistre le hash actuel du fichier source anglais afin que le fichier n'apparaisse plus comme en attente tant que la source anglaise ne change pas.

### Exemple

```bash
doctools.sh translate-done ./MyPackage fr getting-started.md
doctools.sh translate-done ./MyPackage zh recipes/add-a-new-state.md
```

---

## add-language

Ajoute une langue a la configuration de traduction du package.

```bash
doctools.sh add-language <package-dir> <lang-code>
```

Cette commande cree ou met a jour `Documentation/doctools.languages.json`. Executez-la sans code de langue pour voir tous les codes pris en charge.

### Langues prises en charge

| Code | Langue | Code | Langue |
|------|--------|------|--------|
| `fr` | Francais | `ja` | 日本語 |
| `de` | Deutsch | `ko` | 한국어 |
| `es` | Espanol | `zh` | 中文 |
| `it` | Italiano | `ru` | Русский |
| `pt` | Portugues | `ar` | العربية |
| `nl` | Nederlands | `pl` | Polski |
| `tr` | Turkce | `sv` | Svenska |

### Exemple

```bash
doctools.sh add-language ./MyPackage fr
doctools.sh add-language ./MyPackage zh
```

---

## version

Gere les versions de la documentation.

### version list

Liste la version courante et toutes les versions precedemment construites.

```bash
doctools.sh version <package-dir> list
```

La version courante (depuis `package.json`) est marquee avec `*`. Les versions precedentes sont decouvertes a partir des repertoires `v*/` dans le dossier de sortie.

### version remove

Supprime une version precedemment construite du repertoire de sortie.

```bash
doctools.sh version <package-dir> remove <version>
```

Vous ne pouvez pas supprimer la version courante.

### Exemple

```bash
doctools.sh version ./MyPackage list
doctools.sh version ./MyPackage remove 0.1.0
```

---

## Etapes suivantes

- [Configuration](configuration.md) — Comment les parametres sont determines
- [Gestion des versions](versioning.md) — Flux de travail multi-versions
- [Traduction](translation.md) — Flux de travail pour la documentation multilingue
