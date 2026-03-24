# Traduction

DocTools prend en charge la traduction de la documentation du manuel en plusieurs langues. L'anglais est toujours la langue source. Les traductions sont stockées aux côtés de la source anglaise et compilées dans des chemins URL distincts avec un sélecteur de langue pour les lecteurs.

## Fonctionnement

1. Vous rédigez la documentation en anglais
2. Vous ajoutez les langues cibles à votre package
3. Un outil externe (tel que Claude Code) lit chaque fichier anglais, le traduit et l'écrit dans un répertoire de locale
4. DocTools suit les fichiers traduits via des hachages SHA-256
5. La compilation produit un site séparé pour chaque langue sous `v{version}/{lang}/`
6. Un menu déroulant de sélection de langue apparaît lorsque plus d'une langue est compilée

Le système de traduction gère uniquement la comptabilité — il suit les fichiers nécessitant une traduction et ceux qui sont à jour. La traduction proprement dite est effectuée en externe.

## Langues prises en charge

| Code | Langue | Code | Langue |
|------|--------|------|--------|
| `fr` | Français | `ja` | 日本語 |
| `de` | Deutsch | `ko` | 한국어 |
| `es` | Español | `zh` | 中文 |
| `it` | Italiano | `ru` | Русский |
| `pt` | Português | `ar` | العربية |
| `nl` | Nederlands | `pl` | Polski |
| `tr` | Türkçe | `sv` | Svenska |

## Ajouter une langue

Ajoutez une langue à votre package :

```bash
doctools.sh add-language ./MyPackage fr
```

Cela crée ou met à jour `Documentation/doctools.languages.json` :

```json
["fr"]
```

Vous pouvez ajouter plusieurs langues :

```bash
doctools.sh add-language ./MyPackage zh
doctools.sh add-language ./MyPackage ja
```

## Flux de travail de traduction

### Étape 1 : Vérifier ce qui doit être traduit

```bash
doctools.sh translate ./MyPackage
```

Cela liste chaque fichier `.md` manquant ou obsolète pour chaque langue configurée, en affichant les chemins source et cible :

```
=== Checking translations for: fr (Français) ===
  Files needing translation:
    - getting-started.md
      Source: /path/to/Documentation/getting-started.md
      Target: /path/to/Documentation/.doctools/locales/fr/getting-started.md
```

### Étape 2 : Traduire chaque fichier

Pour chaque fichier en attente, lisez la source anglaise, traduisez le contenu et écrivez le fichier traduit au chemin cible.

**Règles de traduction :**

- Traduisez tout le texte en prose, les titres, les éléments de liste, le texte des cellules de tableau et le texte alternatif
- Ne traduisez **pas** : les blocs de code, le code en ligne, les chemins de fichiers, les noms de classes/méthodes, les clés du front matter YAML, les URL des liens, les chemins d'images
- Préservez exactement tout le formatage Markdown, la structure des liens et la structure du document
- Conservez les termes techniques qui sont des noms propres (par exemple, « Unity », « DocFX », « Mermaid ») sans les traduire

### Étape 3 : Marquer chaque fichier comme terminé

Après avoir écrit un fichier traduit, mettez à jour le manifeste de hachages :

```bash
doctools.sh translate-done ./MyPackage fr getting-started.md
```

Cela enregistre le hachage SHA-256 actuel du fichier source anglais. Le fichier n'apparaîtra plus comme en attente tant que la source anglaise ne sera pas modifiée.

### Étape 4 : Compiler

Lancez la compilation pour inclure toutes les traductions :

```bash
doctools.sh build ./MyPackage
```

Les langues avec des fichiers traduits sont compilées dans des sous-répertoires (par exemple, `v1.0.0/fr/`). Les langues sans fichiers traduits sont ignorées avec un avertissement.

## Structure des fichiers

Les fichiers traduits sont stockés dans `.doctools/locales/{lang}/`, en reproduisant la structure des fichiers anglais :

```
Documentation/
├── getting-started.md                              # Source anglaise
├── architecture.md
├── .doctools/
│   └── locales/
│       ├── fr/
│       │   ├── .translation-hashes.json            # Manifeste de hachages
│       │   ├── getting-started.md                  # Traduction française
│       │   └── architecture.md
│       └── zh/
│           ├── .translation-hashes.json
│           ├── getting-started.md                  # Traduction chinoise
│           └── architecture.md
```

## Suivi des modifications par hachage

Le fichier `.translation-hashes.json` dans chaque répertoire de langue stocke le hachage SHA-256 du fichier source anglais au moment de la traduction :

```json
{
  "getting-started.md": "a1b2c3d4...",
  "architecture.md": "e5f6g7h8..."
}
```

Lorsque vous exécutez `doctools.sh translate`, il compare le hachage actuel de chaque fichier anglais avec le hachage stocké. S'ils diffèrent, le fichier apparaît comme nécessitant une nouvelle traduction. Cela signifie :

- Les nouveaux fichiers apparaissent toujours comme en attente (pas de hachage stocké)
- Les fichiers anglais modifiés apparaissent comme en attente (hachage modifié)
- Les fichiers inchangés sont ignorés (hachage identique)

## Fonctionnement des compilations traduites

Pour chaque langue, le système de compilation :

1. Copie tout le contenu anglais dans un répertoire de préparation
2. Superpose les fichiers traduits par-dessus (en remplaçant les originaux anglais)
3. Compile un site DocFX séparé à `v{version}/{lang}/`
4. Génère `languages.js` listant toutes les langues compilées

Cela signifie que les fichiers non traduits utilisent automatiquement la version anglaise — vous n'avez pas besoin de traduire chaque fichier avant de compiler.

## Sélecteur de langue

Le modèle inclut un menu déroulant de langue dans la barre de navigation. Il apparaît automatiquement lorsque plus d'une langue est compilée. Il lit `languages.js` et réécrit les URL pour basculer entre les chemins de langue :

- Anglais : `/v1.0.0/manual/getting-started.html`
- Français : `/v1.0.0/fr/manual/getting-started.html`
- Chinois : `/v1.0.0/zh/manual/getting-started.html`

## Documentation API

La documentation API (générée à partir des commentaires XML C#) n'est **pas traduite**. La même documentation API en anglais est partagée entre toutes les compilations de langues.

## Prochaines étapes

- [Commandes](commands.md) — Référence CLI pour les commandes de traduction
- [Configuration](configuration.md) — Configuration des langues
- [Bonnes pratiques](best-practices.md) — Rédiger une documentation qui se traduit bien
