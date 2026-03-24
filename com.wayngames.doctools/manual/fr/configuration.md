# Configuration

DocTools utilise une approche **convention plutôt que configuration**. La plupart des paramètres sont déduits automatiquement de votre `package.json`. Aucun fichier de configuration n'est requis pour les packages Unity standard.

## Fonctionnement de la configuration

Lorsque vous exécutez une commande DocTools, les paramètres sont chargés dans cet ordre :

1. **`package.json`** — Le nom du package, le nom d'affichage, la version et l'auteur sont lus depuis le manifeste standard du package Unity
2. **Conventions** — Le répertoire de sortie, la source des métadonnées, le logo et le pied de page sont déduits de valeurs par défaut raisonnables
3. **`doctools.languages.json`** (optionnel) — Liste les langues pour lesquelles construire les traductions

## Requis : package.json

Votre `package.json` doit contenir ces champs :

```json
{
  "name": "com.company.mypackage",
  "displayName": "My Package",
  "version": "1.0.0",
  "author": {
    "name": "Company Name"
  }
}
```

| Champ | Utilisation |
|-------|-------------|
| `name` | Identifiant du package, segment de chemin URL |
| `displayName` | Titre de la barre de navigation, titres des pages |
| `version` | Numéro de version actuel, nom du répertoire de sortie |
| `author.name` | Texte de copyright du pied de page |

## Paramètres déduits

DocTools calcule automatiquement ces valeurs. Vous n'avez pas besoin de les définir.

| Paramètre | Convention | Description |
|-----------|-----------|-------------|
| Répertoire de sortie | `../Documentation~` | Relatif à `Documentation/`. Le suffixe `~` le masque de la base de données d'assets Unity |
| Source des métadonnées | `..` | Les fichiers source C# sont analysés depuis la racine du package (parent de `Documentation/`) |
| Logo | Avatar de l'organisation GitHub | Utilise `https://avatars.githubusercontent.com/u/{org-id}` |
| Favicon | Identique au logo | |
| Pied de page | `© {year} {author.name}` | Généré automatiquement à partir de l'auteur du package.json |
| Contenu supplémentaire | `LICENSE.md`, `CHANGELOG.md` | Inclus dans la barre de navigation s'ils sont présents à la racine du package |
| Motifs de ressources | `**/*.png` | Images incluses dans la construction |
| Disposition des espaces de noms | `nested` | La documentation de l'API utilise une hiérarchie d'espaces de noms imbriquée |
| Autoriser les erreurs de compilation | `true` | Les métadonnées DocFX réussissent même si le C# a des références manquantes |

## Détection de la disposition

DocTools prend en charge deux dispositions de documentation :

### Nouvelle disposition (recommandée)

Les fichiers Markdown se trouvent directement dans `Documentation/`. La table des matières racine est générée automatiquement lors de la construction. C'est la disposition par défaut pour les nouveaux projets.

```
MyPackage/
├── package.json
└── Documentation/
    ├── toc.md              # Navigation latérale
    ├── index.md            # Page d'accueil
    ├── install.md          # Vos pages...
    └── Images/
```

### Disposition héritée

Les fichiers Markdown se trouvent dans `Documentation/Manual/` avec un fichier `doctools.config` séparé. DocTools détecte automatiquement cette disposition si un répertoire `Manual/` ou un fichier `doctools.config` existe.

```
MyPackage/
├── package.json
└── Documentation/
    ├── doctools.config     # Fichier de configuration hérité
    ├── toc.md              # Navigation racine
    ├── index.md
    └── Manual/
        ├── toc.md          # Navigation latérale
        └── *.md
```

> [!NOTE]
> La disposition héritée est entièrement prise en charge mais n'est pas recommandée pour les nouveaux projets. Utilisez la nouvelle disposition sauf si vous avez une raison spécifique d'utiliser `Manual/`.

## Configuration des langues

Pour activer les constructions multilingues, créez `Documentation/doctools.languages.json` :

```json
["fr", "zh", "ja"]
```

Cela indique à DocTools de construire les versions française, chinoise et japonaise en plus de l'anglais. Consultez [Traduction](translation.md) pour le flux de travail complet.

Si ce fichier n'existe pas, seul l'anglais est construit et aucun sélecteur de langue n'apparaît.

## Détection des versions

DocTools détecte automatiquement les versions :

- **Version actuelle** : lue depuis le champ `version` du `package.json`
- **Versions précédentes** : découvertes à partir des répertoires `v*/` existants dans le dossier de sortie

Aucune liste de versions n'a besoin d'être maintenue manuellement. Lorsque vous incrémentez la version dans `package.json` et reconstruisez, la nouvelle version apparaît aux côtés des anciennes.

## doctools.config hérité

Les projets plus anciens peuvent utiliser un fichier Bash `doctools.config` au lieu de s'appuyer sur les conventions. Si ce fichier existe, DocTools le charge pour assurer la rétrocompatibilité. Consultez la [référence de la configuration héritée](#référence-de-la-configuration-héritée) ci-dessous.

### Référence de la configuration héritée

| Champ | Type | Description |
|-------|------|-------------|
| `PACKAGE_NAME` | string | Identifiant du package |
| `PACKAGE_TITLE` | string | Titre d'affichage |
| `CURRENT_VERSION` | string | Version actuelle |
| `ALL_VERSIONS` | array | Toutes les versions à construire |
| `PACKAGE_FOOTER` | string | HTML du pied de page |
| `METADATA_SRC` | string | Chemin des sources C# |
| `LOGO_PATH` | string | Chemin de l'image du logo |
| `OUTPUT_DIR` | string | Répertoire de sortie de la construction |
| `EXTRA_CONTENT` | array | Fichiers supplémentaires à inclure |
| `RESOURCE_PATTERNS` | array | Motifs glob pour les images |

> [!TIP]
> Pour migrer de la configuration héritée vers les conventions : supprimez `doctools.config`, assurez-vous que votre `package.json` contient les champs requis, et déplacez les fichiers de documentation de `Manual/` directement dans `Documentation/`.

## Prochaines étapes

- [Commandes](commands.md) — Référence complète de la CLI
- [Gestion des versions](versioning.md) — Flux de travail multi-versions
- [Traduction](translation.md) — Support multilingue
