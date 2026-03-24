# Bonnes pratiques

Recommandations pour tirer le meilleur parti de DocTools.

## Structure du projet

### Utiliser la nouvelle disposition

Placez les fichiers markdown directement dans `Documentation/` plutôt que dans un sous-répertoire `Manual/`. La nouvelle disposition génère automatiquement la table des matières racine et ne nécessite aucun fichier `doctools.config`.

### Garder Documentation/ ciblé

Ne placez que les fichiers liés à la documentation dans `Documentation/` :
- Pages markdown (`.md`)
- Images (sous-répertoire `Images/`)
- Fichiers de traduction (`.doctools/locales/`)

Les fichiers générés vont dans `Documentation~/` (automatiquement exclu de la base de données d'assets Unity).

### Organiser avec des sous-répertoires

Pour les packages comportant de nombreuses pages, utilisez des sous-répertoires pour regrouper le contenu associé :

```
Documentation/
├── toc.md
├── index.md
├── install.md
├── quickstart.md
├── recipes/
│   ├── add-a-feature.md
│   └── integrate-with-x.md
├── integrations/
│   ├── input-system.md
│   └── cinemachine.md
└── Images/
```

Mettez à jour `toc.md` pour refléter la hiérarchie :

```markdown
# [Installation](install.md)
# [Quickstart](quickstart.md)
# Recipes
## [Add a Feature](recipes/add-a-feature.md)
## [Integrate with X](recipes/integrate-with-x.md)
```

## Rédiger pour la traduction

Si votre documentation sera traduite, suivez ces directives pour produire des traductions plus propres :

### Garder les phrases courtes et directes

Les phrases courtes se traduisent plus fidèlement que les longues phrases composées.

### Éviter les expressions idiomatiques et familières

Des expressions comme « out of the box » ou « under the hood » ne se traduisent pas bien. Utilisez un langage précis à la place.

### Utiliser une terminologie cohérente

Définissez les termes clés une seule fois et utilisez-les de manière cohérente. N'alternez pas entre des synonymes (par exemple, « entity » vs « object » vs « item » pour le même concept).

### Garder les blocs de code et les termes techniques en anglais

Les traducteurs ne doivent pas traduire le code, les noms de classes, les noms de méthodes ou les termes spécifiques à Unity. Les règles de traduction de DocTools imposent cela, mais rédiger en gardant cela à l'esprit est utile.

## Versionnage

### Mettre à jour la version avant de compiler

Mettez toujours à jour `version` dans `package.json` avant d'exécuter `doctools.sh build`. Le numéro de version détermine le nom du répertoire de sortie.

### Conserver les versions précédentes

Ne supprimez pas les anciens répertoires de version de `Documentation~/` sauf si nécessaire. Le sélecteur de version permet aux utilisateurs d'accéder à la documentation correspondant à la version qu'ils utilisent.

### Mettre à jour CHANGELOG.md

Maintenez un fichier `CHANGELOG.md` à la racine de votre package. DocTools l'inclut automatiquement dans la barre de navigation aux côtés de LICENSE.

## Diagrammes Mermaid

### Utiliser Mermaid pour les diagrammes d'architecture et de flux

Les diagrammes Mermaid s'affichent directement dans la documentation sans fichiers image à maintenir. Ils se mettent à jour automatiquement lorsque vous modifiez le markdown.

### Garder les diagrammes simples

Les diagrammes complexes avec de nombreux nœuds deviennent difficiles à lire. Divisez les grands diagrammes en plusieurs plus petits, chacun centré sur un aspect.

### Tester les diagrammes localement

Compilez et visualisez le site pour vérifier que les diagrammes s'affichent correctement. Les erreurs de syntaxe Mermaid produisent des blocs vides sans message d'erreur visible dans le résultat compilé.

## Images

### Utiliser des chemins relatifs

Référencez les images avec des chemins relatifs depuis le fichier markdown :

```markdown
![Screenshot](Images/screenshot.png)
```

### Garder les images dans Documentation/Images/

Stockez toutes les images dans le sous-répertoire `Images/`. Le motif de ressources par défaut (`**/*.png`) les détecte automatiquement.

### Utiliser PNG pour les captures d'écran, SVG pour les diagrammes

Le format PNG convient bien aux captures d'écran et aux captures d'interface. Pour les diagrammes, préférez Mermaid (rendu au moment de la compilation) aux fichiers image statiques.

## CI/CD

### Déployer lors du push d'un tag

Utilisez le workflow réutilisable GitHub Actions pour déployer la documentation automatiquement lorsque vous poussez un tag de version. Voir [Workflow CI/CD](workflow.md).

### Commiter les traductions dans le dépôt

Les fichiers traduits dans `.doctools/locales/` doivent être commités dans le contrôle de version. La compilation CI les récupère automatiquement — aucune traduction n'a lieu pendant la CI.

## Étapes suivantes

- [Quickstart](quickstart.md) — Démarrer avec DocTools
- [Translation](translation.md) — Workflow multilingue
- [Template](template.md) — Fonctionnalités et personnalisation du template
