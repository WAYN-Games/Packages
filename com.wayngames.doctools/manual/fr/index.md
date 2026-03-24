# DocTools

**DocTools** est un CLI Bash qui génère des sites de documentation [DocFX](https://dotnet.github.io/docfx/) multi-versions et multilingues pour les packages Unity. Pointez-le vers un package contenant un `package.json`, lancez une seule commande, et obtenez un site HTML complet avec référence API, sélecteur de version, sélecteur de langue, mode sombre/clair et support des diagrammes Mermaid.

## Quand utiliser DocTools

Utilisez DocTools lorsque vous avez besoin de :

- Un site de documentation pour un package Unity avec référence API C#
- Une sortie multi-versions avec un menu déroulant de versions pour les lecteurs
- Un support de traduction automatique avec des builds par langue
- Un thème soigné avec mode sombre, accessibilité et support des diagrammes
- Un déploiement CI/CD vers GitHub Pages

DocTools n'est **pas** destiné aux projets sans `package.json`, à la génération de sites statiques généralistes, ni aux documentations qui n'ont pas besoin de l'extraction de métadonnées C# de DocFX.

## Fonctionnalités clés

- **Convention plutôt que configuration** — lit le `package.json` pour le titre, la version et l'auteur. Aucun fichier de configuration nécessaire.
- **Builds en une commande** — `doctools.sh build` gère l'extraction des métadonnées, le traitement du contenu et la génération de la sortie
- **Sortie multi-versions** — chaque version dans son propre répertoire avec un sélecteur de version global
- **Support multilingue** — traduisez la documentation dans 15 langues avec un suivi des modifications par hash et un sélecteur de langue
- **Modèle personnalisé** — thème sombre/clair, diagrammes Mermaid, coloration syntaxique, accessibilité WCAG AA
- **Prêt pour le CI/CD** — workflow GitHub Actions réutilisable pour déployer sur GitHub Pages lors d'un push de tag

## Liens rapides

- [Installation](install.md) — Prérequis et mise en place
- [Démarrage rapide](quickstart.md) — Initialiser, construire et visualiser en 5 minutes
- [Configuration](configuration.md) — Comment DocTools lit les métadonnées de votre package
- [Commandes](commands.md) — Référence complète du CLI
- [Versionnage](versioning.md) — Workflow multi-versions
- [Traduction](translation.md) — Documentation multilingue
- [Modèle](template.md) — Fonctionnalités du thème et personnalisation
- [Workflow CI/CD](workflow.md) — Déploiement automatisé
- [Dépannage](troubleshooting.md) — Problèmes courants et solutions

## Prérequis

| Prérequis | Notes |
|-----------|-------|
| **Bash** | Git Bash sous Windows, ou natif sous macOS/Linux |
| **DocFX** | Installer avec `dotnet tool install -g docfx` |
| **.NET 9+** | Requis par DocFX pour l'extraction des métadonnées C# |

## Informations sur le package

| | |
|---|---|
| **Nom du package** | `com.wayngames.doctools` |
| **Version** | 0.3.0 |
| **Auteur** | [WAYN Games](https://wayn.games) |

## Étape suivante

Commencez par l'[Installation](install.md) pour configurer DocTools, puis suivez le [Démarrage rapide](quickstart.md) pour construire votre premier site.
